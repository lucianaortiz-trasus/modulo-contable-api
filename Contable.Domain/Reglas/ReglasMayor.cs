namespace Contable.Domain.Reglas;

/// <summary>Un movimiento tal como entra al mayor, antes de calcular el saldo corriente.</summary>
public record MovimientoMayor(
    int IdAsiento,
    int NumeroAsiento,
    DateTime FecContable,
    string? Leyenda,
    decimal? Debe,
    decimal? Haber,
    bool Anulado);

/// <summary>Un renglon del mayor, ya con el saldo acumulado hasta esa linea.</summary>
public record RenglonMayor
{
    public required int IdAsiento { get; init; }

    public required int NumeroAsiento { get; init; }

    public required DateTime FecContable { get; init; }

    public string? Leyenda { get; init; }

    public decimal Debe { get; init; }

    public decimal Haber { get; init; }

    /// <summary>Saldo de la cuenta despues de este movimiento, en valor absoluto.</summary>
    public decimal Saldo { get; init; }

    /// <summary>'D' si el saldo quedo deudor, 'H' si quedo acreedor.</summary>
    public required string TipoSaldo { get; init; }

    public bool Anulado { get; init; }
}

/// <summary>El mayor de una cuenta: saldo anterior, movimientos y saldo final.</summary>
public class Mayor
{
    public required int IdCtaContable { get; init; }

    public required string Codigo { get; init; }

    public required string Nombre { get; init; }

    /// <summary>Saldo con el que la cuenta llega al periodo, en valor absoluto.</summary>
    public decimal SaldoAnterior { get; init; }

    public required string TipoSaldoAnterior { get; init; }

    public List<RenglonMayor> Renglones { get; init; } = [];

    public decimal TotalDebe { get; init; }

    public decimal TotalHaber { get; init; }

    public decimal SaldoFinal { get; init; }

    public required string TipoSaldoFinal { get; init; }
}

/// <summary>
/// Mayor de una cuenta contable.
/// </summary>
/// <remarks>
/// Portado de `PKG_REPORTES_CONTABILIDAD.PRC_MAYORES`. El mayor es el detalle cronologico de una
/// cuenta: arranca del saldo con el que viene del periodo anterior y va acumulando movimiento a
/// movimiento.
///
/// El criterio del saldo es el mismo que usa la vista `VT_SALDO_CTACONTABLE` y que aplica el
/// cierre de ejercicio: el saldo se informa en valor absoluto y aparte se dice de que lado quedo.
/// Asi las tres cosas —vista, cierre y mayor— hablan el mismo idioma.
/// </remarks>
public static class ReglasMayor
{
    public const string Deudor = "D";

    public const string Acreedor = "H";

    /// <summary>Lado del saldo a partir de la diferencia con signo.</summary>
    public static string TipoDeSaldo(decimal diferencia) => diferencia >= 0 ? Deudor : Acreedor;

    /// <summary>
    /// Arma el mayor de una cuenta.
    /// </summary>
    /// <param name="idCtaContable">Cuenta.</param>
    /// <param name="codigo">Codigo de la cuenta.</param>
    /// <param name="nombre">Denominacion de la cuenta.</param>
    /// <param name="diferenciaAnterior">
    /// Debe menos haber de todo lo anterior al periodo, con signo. Positivo es saldo deudor.
    /// </param>
    /// <param name="movimientos">Movimientos del periodo, en orden cronologico.</param>
    public static Mayor Armar(
        int idCtaContable,
        string codigo,
        string nombre,
        decimal diferenciaAnterior,
        IEnumerable<MovimientoMayor> movimientos)
    {
        var ordenados = movimientos
            .OrderBy(m => m.FecContable)
            .ThenBy(m => m.NumeroAsiento)
            .ToList();

        var corriente = ReglasBalanceAsiento.Redondear(diferenciaAnterior);
        var renglones = new List<RenglonMayor>(ordenados.Count);

        decimal totalDebe = 0;
        decimal totalHaber = 0;

        foreach (var movimiento in ordenados)
        {
            var debe = ReglasBalanceAsiento.Redondear(movimiento.Debe ?? 0);
            var haber = ReglasBalanceAsiento.Redondear(movimiento.Haber ?? 0);

            totalDebe += debe;
            totalHaber += haber;
            corriente = ReglasBalanceAsiento.Redondear(corriente + debe - haber);

            renglones.Add(new RenglonMayor
            {
                IdAsiento = movimiento.IdAsiento,
                NumeroAsiento = movimiento.NumeroAsiento,
                FecContable = movimiento.FecContable,
                Leyenda = movimiento.Leyenda,
                Debe = debe,
                Haber = haber,
                Saldo = Math.Abs(corriente),
                TipoSaldo = TipoDeSaldo(corriente),
                Anulado = movimiento.Anulado,
            });
        }

        return new Mayor
        {
            IdCtaContable = idCtaContable,
            Codigo = codigo,
            Nombre = nombre,
            SaldoAnterior = Math.Abs(ReglasBalanceAsiento.Redondear(diferenciaAnterior)),
            TipoSaldoAnterior = TipoDeSaldo(diferenciaAnterior),
            Renglones = renglones,
            TotalDebe = ReglasBalanceAsiento.Redondear(totalDebe),
            TotalHaber = ReglasBalanceAsiento.Redondear(totalHaber),
            SaldoFinal = Math.Abs(corriente),
            TipoSaldoFinal = TipoDeSaldo(corriente),
        };
    }
}
