namespace Contable.Domain.Reglas;

/// <summary>Qué tan grave es un hallazgo de auditoría.</summary>
public enum Severidad
{
    /// <summary>Rompe la contabilidad: hay que corregirlo.</summary>
    Error,

    /// <summary>Puede ser legítimo, pero conviene mirarlo.</summary>
    Advertencia,

    /// <summary>Nada roto: una oportunidad de mejorar los datos.</summary>
    Sugerencia,
}

/// <summary>Un hallazgo concreto, con lo necesario para ir a corregirlo.</summary>
public record Hallazgo(
    string Tipo,
    Severidad Severidad,
    string Descripcion,
    int? IdAsiento = null,
    int? Numero = null,
    int? IdCtaContable = null,
    string? Codigo = null,
    decimal? Importe = null);

/// <summary>Un grupo de hallazgos del mismo tipo.</summary>
public class GrupoDeHallazgos
{
    public required string Tipo { get; init; }

    public required string Titulo { get; init; }

    public required Severidad Severidad { get; init; }

    /// <summary>Qué significa el hallazgo y qué conviene hacer.</summary>
    public required string Explicacion { get; init; }

    public List<Hallazgo> Hallazgos { get; init; } = [];

    public int Cantidad => Hallazgos.Count;
}

/// <summary>
/// Catálogo de controles de la auditoría contable.
/// </summary>
/// <remarks>
/// Cada control tiene un título, una severidad y una explicación de qué hacer. Están acá y no
/// desperdigados por el servicio para que agregar uno nuevo sea agregar una entrada, y para que el
/// texto que ve el contador esté junto a la definición del control y no dentro de una consulta.
///
/// La severidad importa: un asiento descuadrado es un error que rompe la contabilidad, mientras
/// que una partida sin cuenta asignada en la matriz es apenas una sugerencia de completar datos.
/// Mezclarlos en una sola lista hace que nadie mire ninguna.
/// </remarks>
public static class ReglasAuditoria
{
    public const string AsientoDescuadrado = "asiento-descuadrado";

    public const string AsientoSinRenglones = "asiento-sin-renglones";

    public const string RenglonHuerfano = "renglon-huerfano";

    public const string CuentaNoImputable = "cuenta-no-imputable";

    public const string CuentaInactiva = "cuenta-inactiva";

    public const string FechaFueraDeVigencia = "fecha-fuera-de-vigencia";

    public const string PosibleDuplicado = "posible-duplicado";

    public const string CuentaSinMatriz = "cuenta-sin-matriz";

    public const string PartidaSinCuenta = "partida-sin-cuenta";

    public const string EstructuraDelPlan = "estructura-del-plan";

    public static GrupoDeHallazgos Grupo(string tipo, IEnumerable<Hallazgo> hallazgos)
    {
        var (titulo, severidad, explicacion) = Definicion(tipo);

        return new GrupoDeHallazgos
        {
            Tipo = tipo,
            Titulo = titulo,
            Severidad = severidad,
            Explicacion = explicacion,
            Hallazgos = [.. hallazgos],
        };
    }

    private static (string Titulo, Severidad Severidad, string Explicacion) Definicion(string tipo) => tipo switch
    {
        AsientoDescuadrado => (
            "Asientos descuadrados",
            Severidad.Error,
            "El debe y el haber del asiento no coinciden. Un asiento así rompe la partida doble y " +
            "desbalancea el balance de sumas y saldos. El módulo no deja registrar asientos " +
            "descuadrados, así que si aparece alguno viene de una carga anterior o de otra vía."),

        AsientoSinRenglones => (
            "Asientos sin renglones",
            Severidad.Error,
            "La cabecera del asiento existe pero no tiene detalle. Suele ser una carga interrumpida " +
            "del sistema viejo, donde la cabecera y el detalle se grababan por separado."),

        RenglonHuerfano => (
            "Renglones sin cabecera",
            Severidad.Error,
            "Hay renglones que apuntan a un asiento que no existe. Esos importes no aparecen en el " +
            "libro diario pero sí pueden colarse en los saldos."),

        CuentaNoImputable => (
            "Movimientos sobre cuentas no imputables",
            Severidad.Advertencia,
            "Se imputó a una cuenta de agrupación en lugar de a una hoja del plan. El importe existe " +
            "pero queda fuera del nivel de detalle esperado, y puede contarse dos veces al consolidar."),

        CuentaInactiva => (
            "Movimientos sobre cuentas dadas de baja",
            Severidad.Advertencia,
            "La cuenta está marcada como inactiva pero tiene movimientos en el período. O el " +
            "movimiento no corresponde, o la cuenta no debería estar dada de baja."),

        FechaFueraDeVigencia => (
            "Asientos con la fecha fuera de su ejercicio",
            Severidad.Error,
            "La fecha contable del asiento no cae dentro del ejercicio al que está asignado. El " +
            "asiento va a aparecer en un período y a sumar en otro."),

        PosibleDuplicado => (
            "Posibles asientos duplicados",
            Severidad.Advertencia,
            "Varios asientos comparten fecha, importe y leyenda. Puede ser legítimo —un pago " +
            "repetido, una cuota— pero también una carga hecha dos veces."),

        CuentaSinMatriz => (
            "Cuentas imputables sin relación en la matriz",
            Severidad.Sugerencia,
            "La cuenta recibe movimientos manuales pero no está relacionada con ninguna partida " +
            "presupuestaria, así que los asientos automáticos nunca la van a alcanzar."),

        PartidaSinCuenta => (
            "Partidas presupuestarias sin cuenta asignada",
            Severidad.Sugerencia,
            "La partida no tiene cuenta contable en la matriz del ejercicio. Cuando se ejecute, el " +
            "sistema no va a saber a qué cuenta imputar el asiento."),

        EstructuraDelPlan => (
            "Problemas de estructura del plan de cuentas",
            Severidad.Advertencia,
            "Cuentas que se declaran padre de sí mismas, con nivel desalineado del padre o marcadas " +
            "como imputables teniendo hijas. No rompen los asientos, pero distorsionan los totales " +
            "consolidados de los reportes."),

        _ => (tipo, Severidad.Advertencia, string.Empty),
    };
}

/// <summary>
/// Detección de asientos posiblemente duplicados.
/// </summary>
/// <remarks>
/// Se agrupan por fecha contable, importe total y leyenda normalizada. La normalización importa:
/// dos cargas del mismo asiento rara vez tienen la leyenda idéntica carácter por carácter, pero
/// sí las mismas palabras.
/// </remarks>
public static class ReglasDuplicados
{
    /// <summary>Datos mínimos de un asiento para buscar duplicados.</summary>
    public record AsientoParaComparar(int IdAsiento, int Numero, DateTime FecContable, decimal Total, string? Leyenda);

    /// <summary>Un grupo de asientos que parecen el mismo.</summary>
    public record GrupoDuplicado(DateTime FecContable, decimal Total, string Leyenda, List<AsientoParaComparar> Asientos);

    public static List<GrupoDuplicado> Detectar(IEnumerable<AsientoParaComparar> asientos)
    {
        return
        [
            .. asientos
                .Where(a => a.Total != 0)
                .GroupBy(a => new
                {
                    a.FecContable.Date,
                    a.Total,
                    Clave = Normalizar(a.Leyenda),
                })
                .Where(g => g.Count() > 1)
                .Select(g => new GrupoDuplicado(
                    g.Key.Date,
                    g.Key.Total,
                    g.First().Leyenda ?? string.Empty,
                    [.. g.OrderBy(a => a.Numero)]))
                .OrderByDescending(g => g.Asientos.Count)
                .ThenByDescending(g => g.Total)
        ];
    }

    /// <summary>Leyenda reducida a sus palabras significativas, ordenadas.</summary>
    public static string Normalizar(string? leyenda)
        => string.Join(' ', ReglasSugerenciaDeImputacion.Palabras(leyenda).OrderBy(p => p, StringComparer.Ordinal));
}
