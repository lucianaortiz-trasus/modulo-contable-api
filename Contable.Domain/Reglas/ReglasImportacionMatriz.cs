namespace Contable.Domain.Reglas;

/// <summary>Fila de la matriz del ejercicio de origen.</summary>
public record FilaMatrizOrigen(int IdMatrizCble, int? IdCtaContable, int? IdObjetoGasto, int? IdRecurso);

/// <summary>Fila lista para insertar en el ejercicio destino.</summary>
public record FilaMatrizAImportar(int IdCtaContable, int? IdObjetoGasto, int? IdRecurso, TipoDePartida Tipo);

/// <summary>Fila que no se pudo trasladar, con el motivo.</summary>
public record FilaMatrizNoImportable(int IdMatrizOrigen, TipoDePartida? Tipo, long? NumeroPartida, string Motivo);

/// <summary>Lo que va a pasar si se confirma la importacion.</summary>
public class PlanDeImportacion
{
    public List<FilaMatrizAImportar> AImportar { get; init; } = [];

    public List<FilaMatrizNoImportable> NoImportables { get; init; } = [];

    /// <summary>Filas del origen que en el destino ya estaban cargadas.</summary>
    public int YaExistentes { get; init; }

    public int TotalOrigen { get; init; }

    public int CantidadDeGastos => AImportar.Count(f => f.Tipo == TipoDePartida.Gasto);

    public int CantidadDeRecursos => AImportar.Count(f => f.Tipo == TipoDePartida.Recurso);
}

/// <summary>
/// Traslada la matriz contable de un ejercicio al siguiente.
/// </summary>
/// <remarks>
/// Reemplaza a PKG_T_MATRIZ_CONTABLE.IMPORTA_RELACION, que tenia cuatro problemas:
///
/// - Filtraba <c>and r.id_recurso is null</c>, o sea que **solo importaba los gastos**. Las filas
///   de recurso nunca pasaban al ejercicio nuevo (D10).
/// - El control de "existen recursos en la vigencia destino" hacia <c>count(*) + 1</c>, con lo que
///   la condicion era siempre verdadera y la validacion no se disparaba nunca (D11).
/// - Si la partida no tenia equivalente en el ejercicio destino, la subconsulta devolvia null y la
///   fila se insertaba **con la partida en NULL**, sin avisar (D12).
/// - Informaba "Se insertaron N registros" usando el total de la vigencia destino, no la cantidad
///   realmente insertada.
///
/// Aca se importan los dos tipos de partida, lo que no se puede traducir se informa en lugar de
/// insertarse en NULL, y el resultado dice exactamente que paso con cada fila.
///
/// La cuenta contable no se traduce entre ejercicios, igual que en el PL/SQL original: el plan de
/// cuentas es un unico arbol compartido, no uno por vigencia (ver D27).
/// </remarks>
public static class ReglasImportacionMatriz
{
    /// <summary>
    /// Arma el plan de importacion sin tocar la base: permite mostrar al usuario que va a pasar
    /// antes de confirmar.
    /// </summary>
    /// <param name="origen">Filas de la matriz del ejercicio anterior.</param>
    /// <param name="objetosPorNumero">Numero de objeto de gasto del origen, mapeado a su id en el destino.</param>
    /// <param name="recursosPorNumero">Numero de recurso del origen, mapeado a su id en el destino.</param>
    /// <param name="numeroDeObjeto">Id de objeto de gasto del origen, mapeado a su numero.</param>
    /// <param name="numeroDeRecurso">Id de recurso del origen, mapeado a su numero.</param>
    /// <param name="yaEnDestino">Relaciones (cuenta, partida, tipo) que ya existen en el destino.</param>
    public static PlanDeImportacion Planificar(
        IEnumerable<FilaMatrizOrigen> origen,
        IReadOnlyDictionary<long, int> objetosPorNumero,
        IReadOnlyDictionary<long, int> recursosPorNumero,
        IReadOnlyDictionary<int, long> numeroDeObjeto,
        IReadOnlyDictionary<int, long> numeroDeRecurso,
        IReadOnlySet<(int Cuenta, int Partida, TipoDePartida Tipo)> yaEnDestino)
    {
        var aImportar = new List<FilaMatrizAImportar>();
        var noImportables = new List<FilaMatrizNoImportable>();
        var yaExistentes = 0;
        var total = 0;

        // Dentro de la misma corrida tampoco se duplica: dos filas de origen distintas pueden
        // apuntar a partidas que en el destino colapsan en la misma.
        var planificadas = new HashSet<(int, int, TipoDePartida)>();

        foreach (var fila in origen)
        {
            total++;

            var datos = new DatosMatriz(fila.IdCtaContable, fila.IdObjetoGasto, fila.IdRecurso, null);
            var tipo = ReglasMatrizContable.TipoDe(datos);

            if (fila.IdCtaContable is null or <= 0)
            {
                noImportables.Add(new FilaMatrizNoImportable(
                    fila.IdMatrizCble, tipo, null, "La fila de origen no tiene cuenta contable."));
                continue;
            }

            if (tipo is null)
            {
                var ambas = fila.IdObjetoGasto is > 0 && fila.IdRecurso is > 0;

                noImportables.Add(new FilaMatrizNoImportable(
                    fila.IdMatrizCble,
                    null,
                    null,
                    ambas
                        ? "La fila de origen tiene objeto de gasto y recurso a la vez."
                        : "La fila de origen no tiene ninguna partida."));
                continue;
            }

            var esGasto = tipo == TipoDePartida.Gasto;
            var idOrigen = esGasto ? fila.IdObjetoGasto!.Value : fila.IdRecurso!.Value;
            var numeros = esGasto ? numeroDeObjeto : numeroDeRecurso;
            var destinoPorNumero = esGasto ? objetosPorNumero : recursosPorNumero;
            var etiqueta = esGasto ? "objeto de gasto" : "recurso";

            if (!numeros.TryGetValue(idOrigen, out var numero))
            {
                noImportables.Add(new FilaMatrizNoImportable(
                    fila.IdMatrizCble, tipo, null,
                    $"No se encontro el {etiqueta} de origen en el ejercicio anterior."));
                continue;
            }

            if (!destinoPorNumero.TryGetValue(numero, out var idDestino))
            {
                noImportables.Add(new FilaMatrizNoImportable(
                    fila.IdMatrizCble, tipo, numero,
                    $"El {etiqueta} {numero} no existe en el ejercicio destino."));
                continue;
            }

            var clave = (fila.IdCtaContable.Value, idDestino, tipo.Value);

            if (yaEnDestino.Contains(clave) || !planificadas.Add(clave))
            {
                yaExistentes++;
                continue;
            }

            aImportar.Add(new FilaMatrizAImportar(
                fila.IdCtaContable.Value,
                esGasto ? idDestino : null,
                esGasto ? null : idDestino,
                tipo.Value));
        }

        return new PlanDeImportacion
        {
            AImportar = aImportar,
            NoImportables = noImportables,
            YaExistentes = yaExistentes,
            TotalOrigen = total,
        };
    }
}
