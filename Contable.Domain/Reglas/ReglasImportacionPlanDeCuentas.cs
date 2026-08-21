namespace Contable.Domain.Reglas;

/// <summary>Una fila tal como vino del archivo, sin interpretar.</summary>
/// <param name="Fila">Numero de fila en la planilla, para poder senalar el error.</param>
public record FilaImportada(
    int Fila,
    string? Codigo,
    string? Nombre,
    string? TipoSaldo,
    string? Imputable,
    int? IdNaturaleza);

/// <summary>Que se va a hacer con una fila.</summary>
public enum EstadoDeLaFila
{
    /// <summary>Se va a dar de alta.</summary>
    Crear,

    /// <summary>El codigo ya esta en el plan. No se toca: importar no pisa lo que existe.</summary>
    YaExiste,

    /// <summary>No se puede crear. El motivo dice por que.</summary>
    Rechazada,
}

/// <summary>Una fila ya evaluada contra el plan actual y contra el resto del archivo.</summary>
public record FilaEvaluada
{
    public required int Fila { get; init; }

    public required string CodigoOriginal { get; init; }

    /// <summary>Codigo normalizado. Vacio si no se pudo interpretar.</summary>
    public string Codigo { get; init; } = string.Empty;

    public string? Nombre { get; init; }

    public int Nivel { get; init; }

    public string TipoSaldo { get; init; } = ReglasJerarquiaCuentas.SaldoDeudor;

    public bool Imputable { get; init; }

    public int? IdNaturaleza { get; init; }

    /// <summary>Codigo del padre. Vacio en las de nivel 1.</summary>
    public string CodigoPadre { get; init; } = string.Empty;

    public required EstadoDeLaFila Estado { get; init; }

    public string? Motivo { get; init; }
}

/// <summary>Lo que va a pasar si se confirma la importacion.</summary>
public class PlanDeImportacionDeCuentas
{
    public List<FilaEvaluada> Filas { get; init; } = [];

    public int Total => Filas.Count;

    public int ACrear => Filas.Count(f => f.Estado == EstadoDeLaFila.Crear);

    public int YaExisten => Filas.Count(f => f.Estado == EstadoDeLaFila.YaExiste);

    public int Rechazadas => Filas.Count(f => f.Estado == EstadoDeLaFila.Rechazada);

    /// <summary>Las que se van a crear, en orden de codigo: cada padre antes que sus hijas.</summary>
    public IEnumerable<FilaEvaluada> ParaCrear =>
        Filas.Where(f => f.Estado == EstadoDeLaFila.Crear)
             .OrderBy(f => CodigoCuenta.Normalizar(f.Codigo), StringComparer.Ordinal);
}

/// <summary>
/// Carga masiva del plan de cuentas desde una planilla.
/// </summary>
/// <remarks>
/// El archivo puede traer un plan entero, con padres e hijas mezclados, asi que la evaluacion se
/// hace <b>en orden de codigo</b>: cuando se llega a una hija, su padre ya fue evaluado y se sabe
/// si va a existir. Un padre rechazado arrastra a sus hijas, porque colgarian de una cuenta que no
/// se va a crear.
///
/// Un codigo que ya esta en el plan **no es un error**: se informa y se omite. Importar agrega, no
/// pisa. Reescribir cuentas existentes desde una planilla es la clase de operacion que rompe la
/// contabilidad sin dejar rastro, y no se hace desde aca.
///
/// Nada de esto escribe: devuelve el plan para que el usuario lo mire antes de confirmar.
/// </remarks>
public static class ReglasImportacionPlanDeCuentas
{
    public const int LargoMaximoDelNombre = 500;

    /// <summary>Tope de filas por archivo. Mas que esto no se revisa, se acepta a ciegas.</summary>
    public const int MaximoDeFilas = 5000;

    /// <summary>
    /// Evalua el archivo contra el plan actual.
    /// </summary>
    /// <param name="filas">Filas leidas de la planilla.</param>
    /// <param name="codigosExistentes">Codigos que ya estan en el plan, normalizados.</param>
    /// <param name="imputablesExistentes">
    /// De los existentes, cuales estan marcados como imputables. Una cuenta imputable no puede
    /// recibir hijas.
    /// </param>
    public static PlanDeImportacionDeCuentas Evaluar(
        IEnumerable<FilaImportada> filas,
        ISet<string> codigosExistentes,
        ISet<string> imputablesExistentes)
    {
        // Se evalua en orden de codigo para que el padre siempre llegue antes que la hija, pero se
        // devuelve en el orden del archivo: el usuario busca por numero de fila.
        var enOrdenDeArchivo = filas.ToList();

        var enOrdenDeCodigo = enOrdenDeArchivo
            .OrderBy(f => CodigoCuenta.Normalizar(f.Codigo), StringComparer.Ordinal)
            .ThenBy(f => f.Fila)
            .ToList();

        var evaluadas = new Dictionary<int, FilaEvaluada>();

        // Codigos que van a existir despues de importar: los del plan mas los que se van creando.
        var vanAExistir = new HashSet<string>(codigosExistentes, StringComparer.Ordinal);
        var imputables = new HashSet<string>(imputablesExistentes, StringComparer.Ordinal);
        var vistosEnElArchivo = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var fila in enOrdenDeCodigo)
        {
            var evaluada = EvaluarFila(fila, vanAExistir, imputables, vistosEnElArchivo);

            evaluadas[fila.Fila] = evaluada;

            if (evaluada.Estado == EstadoDeLaFila.Crear)
            {
                vanAExistir.Add(evaluada.Codigo);
                vistosEnElArchivo[evaluada.Codigo] = fila.Fila;

                if (evaluada.Imputable) imputables.Add(evaluada.Codigo);
            }
            else if (!string.IsNullOrEmpty(evaluada.Codigo) && !vistosEnElArchivo.ContainsKey(evaluada.Codigo))
            {
                vistosEnElArchivo[evaluada.Codigo] = fila.Fila;
            }
        }

        return new PlanDeImportacionDeCuentas
        {
            Filas = [.. enOrdenDeArchivo.Select(f => evaluadas[f.Fila])],
        };
    }

    private static FilaEvaluada EvaluarFila(
        FilaImportada fila,
        ISet<string> vanAExistir,
        ISet<string> imputables,
        IDictionary<string, int> vistosEnElArchivo)
    {
        var codigo = CodigoCuenta.Normalizar(fila.Codigo);
        var original = (fila.Codigo ?? string.Empty).Trim();

        FilaEvaluada Rechazar(string motivo) => new()
        {
            Fila = fila.Fila,
            CodigoOriginal = original,
            Codigo = codigo,
            Nombre = fila.Nombre?.Trim(),
            Estado = EstadoDeLaFila.Rechazada,
            Motivo = motivo,
        };

        if (string.IsNullOrWhiteSpace(original)) return Rechazar("Falta el codigo.");

        if (!CodigoCuenta.TieneFormatoValido(codigo))
            return Rechazar($"El codigo \"{original}\" no tiene un formato valido.");

        var nivel = CodigoCuenta.NivelSegunCodigo(codigo);

        if (!CodigoCuenta.EsNivelValido(nivel))
            return Rechazar($"El codigo \"{original}\" da un nivel {nivel}, fuera de 1 a {CodigoCuenta.NivelMaximo}.");

        var nombre = fila.Nombre?.Trim();

        if (string.IsNullOrWhiteSpace(nombre)) return Rechazar("Falta el nombre.");

        if (nombre.Length > LargoMaximoDelNombre)
            return Rechazar($"El nombre supera los {LargoMaximoDelNombre} caracteres.");

        var tipoSaldo = NormalizarTipoSaldo(fila.TipoSaldo);

        if (tipoSaldo is null)
            return Rechazar($"El tipo de saldo \"{fila.TipoSaldo}\" no es valido: se espera D o A.");

        var imputable = EsAfirmativo(fila.Imputable);

        // Duplicado dentro del mismo archivo. Se senala la fila con la que choca, que es lo que
        // hace falta para corregirlo.
        if (vistosEnElArchivo.TryGetValue(codigo, out var filaAnterior) && filaAnterior != fila.Fila)
            return Rechazar($"El codigo {codigo} ya viene en la fila {filaAnterior} del archivo.");

        // Ya esta en el plan: no es un error y no se pisa.
        if (vanAExistir.Contains(codigo) && !vistosEnElArchivo.ContainsKey(codigo))
        {
            return new FilaEvaluada
            {
                Fila = fila.Fila,
                CodigoOriginal = original,
                Codigo = codigo,
                Nombre = nombre,
                Nivel = nivel,
                TipoSaldo = tipoSaldo,
                Imputable = imputable,
                IdNaturaleza = fila.IdNaturaleza,
                Estado = EstadoDeLaFila.YaExiste,
                Motivo = "Ya esta en el plan. No se modifica.",
            };
        }

        var codigoPadre = nivel > 1 ? PadreDe(codigo) : string.Empty;

        if (nivel > 1)
        {
            if (!vanAExistir.Contains(codigoPadre))
                return Rechazar($"No existe la cuenta padre {codigoPadre}, y el archivo tampoco la crea.");

            if (imputables.Contains(codigoPadre))
                return Rechazar($"La cuenta padre {codigoPadre} es imputable, y una cuenta que recibe movimientos no puede tener hijas.");
        }

        return new FilaEvaluada
        {
            Fila = fila.Fila,
            CodigoOriginal = original,
            Codigo = codigo,
            Nombre = nombre,
            Nivel = nivel,
            TipoSaldo = tipoSaldo,
            Imputable = imputable,
            IdNaturaleza = fila.IdNaturaleza,
            CodigoPadre = codigoPadre,
            Estado = EstadoDeLaFila.Crear,
        };
    }

    /// <summary>Codigo del padre: el mismo codigo sin el ultimo segmento.</summary>
    public static string PadreDe(string codigo)
    {
        var segmentos = CodigoCuenta.Descomponer(codigo);

        return segmentos.Length <= 1 ? string.Empty : string.Join(CodigoCuenta.Separador, segmentos[..^1]);
    }

    /// <summary>Acepta D/A y tambien las palabras completas, que es lo que se escribe a mano.</summary>
    public static string? NormalizarTipoSaldo(string? valor)
    {
        var v = (valor ?? string.Empty).Trim().ToUpperInvariant();

        if (v.Length == 0) return null;
        if (v.StartsWith('D')) return ReglasJerarquiaCuentas.SaldoDeudor;
        if (v.StartsWith('A')) return ReglasJerarquiaCuentas.SaldoAcreedor;

        return null;
    }

    /// <summary>
    /// Interpreta la columna Imputable. Vacio es "no": una cuenta se marca imputable a proposito,
    /// nunca por omision.
    /// </summary>
    public static bool EsAfirmativo(string? valor)
    {
        var v = (valor ?? string.Empty).Trim().ToUpperInvariant();

        return v is "S" or "SI" or "SÍ" or "X" or "1" or "TRUE" or "V" or "Y" or "YES";
    }
}
