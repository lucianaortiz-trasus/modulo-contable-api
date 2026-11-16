namespace Contable.Domain.Reglas;

/// <summary>Tipo de partida presupuestaria que una fila de matriz relaciona con una cuenta.</summary>
public enum TipoDePartida
{
    Gasto,
    Recurso,
}

/// <summary>Fila de matriz que se quiere dar de alta o modificar.</summary>
/// <param name="IdCtaContable">Cuenta contable que recibe el movimiento.</param>
/// <param name="IdObjetoGasto">Partida de gasto, o null si la fila es de recurso.</param>
/// <param name="IdRecurso">Partida de recurso, o null si la fila es de gasto.</param>
/// <param name="IdVigencia">Ejercicio al que pertenece la relacion.</param>
public record DatosMatriz(int? IdCtaContable, int? IdObjetoGasto, int? IdRecurso, int? IdVigencia);

/// <summary>Lo que hay que consultar en la base para poder validar una fila de matriz.</summary>
/// <param name="CuentaExiste">La cuenta contable existe.</param>
/// <param name="CuentaEsImputable">La cuenta puede recibir movimientos.</param>
/// <param name="CuentaEstaActiva">La cuenta esta habilitada.</param>
/// <param name="PartidaExiste">La partida indicada existe.</param>
/// <param name="PartidaEsDeLaVigencia">La partida pertenece a la vigencia de la relacion.</param>
/// <param name="PartidaEsImputable">La partida es hoja del clasificador.</param>
/// <param name="RelacionYaCargada">Ya hay una fila con la misma cuenta, partida y vigencia.</param>
/// <param name="PartidaYaAsignadaA">
/// Codigo de la cuenta a la que ya esta asignada esa partida en la misma vigencia, si la hubiera.
/// </param>
public record ContextoMatriz(
    bool CuentaExiste = false,
    bool CuentaEsImputable = false,
    bool CuentaEstaActiva = false,
    bool PartidaExiste = false,
    bool PartidaEsDeLaVigencia = false,
    bool PartidaEsImputable = false,
    bool RelacionYaCargada = false,
    string? PartidaYaAsignadaA = null);

/// <summary>
/// Reglas de la matriz contable: que cuenta recibe cada partida presupuestaria.
/// </summary>
/// <remarks>
/// Reemplaza los controles de PKG_T_MATRIZ_CONTABLE.INSERTAR, que estaban mal de dos formas
/// distintas (defecto D9):
///
/// - Para gasto controlaba el par objeto + cuenta, pero **sin mirar la vigencia**, con lo que la
///   misma relacion no se podia volver a cargar en el ejercicio siguiente.
/// - Para recurso controlaba **solo el recurso**, sin cuenta y sin vigencia: un recurso mapeado
///   una vez no se podia mapear nunca mas, ni a otra cuenta ni en otro ejercicio.
///
/// Sumado a que la importacion entre ejercicios solo trae gastos (D10), el lado recurso de la
/// matriz quedaba congelado para siempre despues de la primera carga. Aca el control es simetrico
/// y siempre por (cuenta, partida, vigencia).
/// </remarks>
public static class ReglasMatrizContable
{
    /// <summary>Determina si la fila relaciona un gasto o un recurso.</summary>
    public static TipoDePartida? TipoDe(DatosMatriz datos)
    {
        var tieneGasto = datos.IdObjetoGasto is > 0;
        var tieneRecurso = datos.IdRecurso is > 0;

        if (tieneGasto == tieneRecurso) return null;

        return tieneGasto ? TipoDePartida.Gasto : TipoDePartida.Recurso;
    }

    public static Validacion ValidarAlta(DatosMatriz datos, ContextoMatriz contexto)
    {
        var validacion = Validar(datos, contexto);

        validacion.AgregarSi(
            contexto.RelacionYaCargada,
            "Esa partida ya esta relacionada con esa cuenta en este ejercicio.");

        // Una partida asignada a otra cuenta no es un error de datos, pero si un aviso: al generar
        // el asiento automatico el sistema no sabria cual de las dos usar.
        validacion.AgregarSi(
            contexto.PartidaYaAsignadaA is not null,
            $"La partida ya esta relacionada con la cuenta {contexto.PartidaYaAsignadaA} en este ejercicio.");

        return validacion;
    }

    public static Validacion ValidarEdicion(DatosMatriz datos, ContextoMatriz contexto)
    {
        var validacion = Validar(datos, contexto);

        validacion.AgregarSi(
            contexto.RelacionYaCargada,
            "Ya existe otra fila con esa misma cuenta, partida y ejercicio.");

        validacion.AgregarSi(
            contexto.PartidaYaAsignadaA is not null,
            $"La partida ya esta relacionada con la cuenta {contexto.PartidaYaAsignadaA} en este ejercicio.");

        return validacion;
    }

    /// <summary>
    /// Valida si una relación de la matriz contable puede eliminarse físicamente.
    /// Si la cuenta posee asientos contables registrados, la eliminación física no está permitida (debe desactivarse).
    /// </summary>
    public static Validacion ValidarEliminacion(bool tieneAsientos)
    {
        var validacion = new Validacion();
        validacion.AgregarSi(
            tieneAsientos,
            "No se puede eliminar físicamente la relación porque la cuenta contable posee asientos registrados. Debe ser desactivada.");
        return validacion;
    }

    // ---- Auditoría / Historial de la Relación (T.16409) ------------------

    public const string AccionGenerar = "Generar";
    public const string AccionModificar = "Modificar";
    public const string AccionEliminar = "Eliminar";
    public const string AccionDesactivar = "Desactivar";

    public static string FormatearComentarioAlta(
        string? codigoCuenta,
        string? nombreCuenta,
        string? tipoPartida,
        long? numeroPartida,
        string? nombrePartida)
    {
        var rotuloPartida = string.Equals(tipoPartida, "Recurso", StringComparison.OrdinalIgnoreCase)
            ? "Recurso"
            : "Objeto del Gasto";
        return $"Se crea la relación\nCuenta: {codigoCuenta} - {nombreCuenta}\n{rotuloPartida}: {numeroPartida} - {nombrePartida}";
    }

    public static string FormatearComentarioModificacion(
        string? codigoCuenta,
        string? nombreCuenta,
        string? tipoPartida,
        long? numeroPartida,
        string? nombrePartida)
    {
        var rotuloPartida = string.Equals(tipoPartida, "Recurso", StringComparison.OrdinalIgnoreCase)
            ? "Recurso"
            : "Objeto del Gasto";
        return $"Se modifica la relación. Nuevos datos:\nCuenta: {codigoCuenta} - {nombreCuenta}\n{rotuloPartida}: {numeroPartida} - {nombrePartida}";
    }

    public static string FormatearComentarioEliminacion() => "Se elimina la relación";

    public static string FormatearComentarioDesactivacion() => "Se deja inactiva la relación";

    private static Validacion Validar(DatosMatriz datos, ContextoMatriz contexto)
    {
        var validacion = new Validacion();

        var tipo = TipoDe(datos);

        if (tipo is null)
        {
            var ambas = datos.IdObjetoGasto is > 0 && datos.IdRecurso is > 0;

            validacion.Agregar(ambas
                ? "Una fila de la matriz relaciona la cuenta con una sola partida: objeto de gasto o recurso, no las dos."
                : "Hay que indicar una partida: un objeto de gasto o un recurso.");
        }

        validacion.AgregarSi(datos.IdCtaContable is null or <= 0, "Hay que indicar la cuenta contable.");
        validacion.AgregarSi(datos.IdVigencia is null or <= 0, "Hay que indicar el ejercicio.");

        if (datos.IdCtaContable is > 0)
        {
            validacion.AgregarSi(!contexto.CuentaExiste, "La cuenta contable no existe.");

            if (contexto.CuentaExiste)
            {
                validacion.AgregarSi(
                    !contexto.CuentaEsImputable,
                    "La cuenta no es imputable: no puede recibir el movimiento de una partida.");

                validacion.AgregarSi(!contexto.CuentaEstaActiva, "La cuenta esta dada de baja.");
            }
        }

        if (tipo is not null)
        {
            var nombre = tipo == TipoDePartida.Gasto ? "El objeto de gasto" : "El recurso";

            validacion.AgregarSi(!contexto.PartidaExiste, $"{nombre} no existe.");

            if (contexto.PartidaExiste)
            {
                validacion.AgregarSi(
                    !contexto.PartidaEsDeLaVigencia,
                    $"{nombre} pertenece a otro ejercicio.");

                validacion.AgregarSi(
                    !contexto.PartidaEsImputable,
                    $"{nombre} no es de ultimo nivel: solo se imputan las partidas hoja del clasificador.");
            }
        }

        return validacion;
    }
}
