namespace Contable.Domain.Reglas;

/// <summary>Un renglon de asiento, para las reglas que no necesitan la entidad completa.</summary>
/// <param name="IdCtaContable">Cuenta imputada.</param>
/// <param name="Debe">Importe al debe, o null.</param>
/// <param name="Haber">Importe al haber, o null.</param>
public record RenglonAsiento(int IdCtaContable, decimal? Debe, decimal? Haber);

/// <summary>
/// La partida doble: un asiento cuadra cuando la suma del debe iguala la del haber.
/// </summary>
/// <remarks>
/// El sistema viejo hacia esta validacion en la pantalla, no en la base: `t_asientosABM.aspx.cs`
/// sumaba los importes de la grilla y comparaba con `impDebe == impHaber` justo antes de grabar.
/// Nada impedia insertar un asiento descuadrado por otra via, y de hecho la vista de saldos no
/// filtra los anulados justamente porque asume que todo lo grabado cuadra.
///
/// El redondeo es explicito y no un efecto de la base: el ABM capturaba cinco decimales contra una
/// columna NUMBER(18,2), asi que Oracle truncaba en silencio y dos importes que en pantalla
/// cuadraban podian quedar descuadrados al grabarse.
/// </remarks>
public static class ReglasBalanceAsiento
{
    /// <summary>Decimales que admite la base en DEBE y HABER.</summary>
    public const int Decimales = 2;

    /// <summary>Redondeo bancario a dos decimales, igual que la columna de la base.</summary>
    public static decimal Redondear(decimal importe) => Math.Round(importe, Decimales, MidpointRounding.ToEven);

    public static decimal TotalDebe(IEnumerable<RenglonAsiento> renglones)
        => Redondear(renglones.Sum(r => Redondear(r.Debe ?? 0)));

    public static decimal TotalHaber(IEnumerable<RenglonAsiento> renglones)
        => Redondear(renglones.Sum(r => Redondear(r.Haber ?? 0)));

    public static decimal Diferencia(IEnumerable<RenglonAsiento> renglones)
    {
        var lista = renglones as IReadOnlyCollection<RenglonAsiento> ?? [.. renglones];
        return Redondear(TotalDebe(lista) - TotalHaber(lista));
    }

    public static bool Cuadra(IEnumerable<RenglonAsiento> renglones) => Diferencia(renglones) == 0m;

    /// <summary>
    /// Valida el conjunto de renglones de un asiento.
    /// </summary>
    public static Validacion Validar(IEnumerable<RenglonAsiento> renglones)
    {
        var validacion = new Validacion();
        var lista = renglones?.ToList() ?? [];

        if (lista.Count == 0)
        {
            return validacion.Agregar("El asiento no tiene renglones.");
        }

        validacion.AgregarSi(lista.Count < 2, "Un asiento necesita al menos dos renglones.");

        for (var i = 0; i < lista.Count; i++)
        {
            var renglon = lista[i];
            var numero = i + 1;

            var debe = Redondear(renglon.Debe ?? 0);
            var haber = Redondear(renglon.Haber ?? 0);

            validacion.AgregarSi(
                renglon.IdCtaContable <= 0,
                $"El renglon {numero} no tiene cuenta contable.");

            validacion.AgregarSi(
                debe < 0 || haber < 0,
                $"El renglon {numero} tiene un importe negativo. Para invertir el signo se usa el otro lado del asiento.");

            validacion.AgregarSi(
                debe == 0 && haber == 0,
                $"El renglon {numero} no tiene importe.");

            validacion.AgregarSi(
                debe > 0 && haber > 0,
                $"El renglon {numero} tiene importe en el debe y en el haber a la vez.");
        }

        var diferencia = Diferencia(lista);

        validacion.AgregarSi(
            diferencia != 0,
            $"El asiento no cuadra: el debe suma {TotalDebe(lista):N2} y el haber {TotalHaber(lista):N2}, " +
            $"una diferencia de {Math.Abs(diferencia):N2}.");

        return validacion;
    }

    /// <summary>
    /// Importe que habria que cargar del lado contrario para que el asiento cuadre.
    /// Es lo que usa la pantalla para proponer la contrapartida.
    /// </summary>
    public static (string Lado, decimal Importe)? ContrapartidaQueCuadra(IEnumerable<RenglonAsiento> renglones)
    {
        var diferencia = Diferencia(renglones);

        if (diferencia == 0) return null;

        return diferencia > 0
            ? ("Haber", diferencia)
            : ("Debe", Math.Abs(diferencia));
    }
}
