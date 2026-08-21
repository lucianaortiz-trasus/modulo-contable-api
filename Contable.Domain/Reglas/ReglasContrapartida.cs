namespace Contable.Domain.Reglas;

/// <summary>Las dos formas de dar vuelta un asiento ya registrado.</summary>
public enum TipoDeContrapartida
{
    /// <summary>Reversa: recalcula la fecha contable segun el ejercicio.</summary>
    Reversa,

    /// <summary>Anulacion: conserva la fecha contable del asiento original.</summary>
    Anulacion,
}

/// <summary>Renglon ya resuelto, listo para insertar.</summary>
public record RenglonPlanificado(int IdCtaContable, decimal? Debe, decimal? Haber, int? IdRecursoDh, int? IdCuentaBancaria = null);

/// <summary>Asiento completo listo para insertar.</summary>
public record AsientoPlanificado(
    string Leyenda,
    string? Observaciones,
    DateTime FecContable,
    int IdTipoAsiento,
    List<RenglonPlanificado> Renglones,
    int? IdAsientoRel = null);

/// <summary>El asiento que se quiere dar vuelta.</summary>
public record AsientoOriginal(
    int IdAsiento,
    int Numero,
    DateTime FecContable,
    int IdTipoAsiento,
    string? Leyenda,
    string? Observaciones,
    int Ejercicio,
    bool YaAnulado,
    bool YaTieneContrapartida);

/// <summary>
/// Reversa y anulacion de asientos.
/// </summary>
/// <remarks>
/// En el sistema viejo son dos procedimientos que hacen casi lo mismo con distinto nombre:
/// `INSERTAR_REVERSA` y `ELIMINAR`. Ninguno de los dos borra: los dos generan un contraasiento con
/// el debe y el haber invertidos y marcan `ANULADO = 'S'` en el original y en la contrapartida.
///
/// La diferencia real entre ellos es la fecha contable:
///
/// - La <b>reversa</b> la recalcula: si el año contable del original es anterior al año en curso,
///   la contrapartida va al 31/12 de su ejercicio; si no, va a la fecha de hoy. Asi la reversa de
///   un asiento viejo no ensucia el ejercicio corriente.
/// - La <b>anulacion</b> conserva la fecha del original, con lo que el par queda en el mismo
///   periodo y se cancela dentro de el.
///
/// Tambien cambia la leyenda: "REVERSA ASIENTO {numero}" contra "CONTRAASIENTO{leyenda original}".
/// </remarks>
public static class ReglasContrapartida
{
    public const string PrefijoAnulacion = "CONTRAASIENTO";

    public static string LeyendaDeReversa(int numeroOriginal) => $"REVERSA ASIENTO {numeroOriginal}";

    public static string LeyendaDeAnulacion(string? leyendaOriginal) => $"{PrefijoAnulacion}{leyendaOriginal}";

    /// <summary>
    /// Fecha contable de la contrapartida.
    /// </summary>
    /// <param name="tipo">Reversa o anulacion.</param>
    /// <param name="original">Asiento de origen.</param>
    /// <param name="hoy">Fecha del dia, explicita para que la regla sea testeable.</param>
    public static DateTime FechaContable(TipoDeContrapartida tipo, AsientoOriginal original, DateTime hoy)
    {
        if (tipo == TipoDeContrapartida.Anulacion) return original.FecContable;

        return original.FecContable.Year < hoy.Year
            ? new DateTime(original.Ejercicio, 12, 31)
            : hoy;
    }

    public static Validacion Validar(AsientoOriginal original)
    {
        var validacion = new Validacion();

        validacion.AgregarSi(
            original.YaAnulado,
            $"El asiento {original.Numero} ya esta anulado.");

        validacion.AgregarSi(
            original.YaTieneContrapartida,
            $"El asiento {original.Numero} ya tiene un contraasiento registrado.");

        return validacion;
    }

    /// <summary>
    /// Arma la contrapartida invirtiendo el debe y el haber de cada renglon.
    /// </summary>
    /// <param name="tipo">Reversa o anulacion.</param>
    /// <param name="original">Asiento de origen.</param>
    /// <param name="renglones">Renglones del asiento de origen.</param>
    /// <param name="hoy">Fecha del dia.</param>
    public static AsientoPlanificado Planificar(
        TipoDeContrapartida tipo,
        AsientoOriginal original,
        IEnumerable<RenglonPlanificado> renglones,
        DateTime hoy)
    {
        var fecha = FechaContable(tipo, original, hoy);

        var invertidos = renglones
            .Select(r => r with { Debe = r.Haber, Haber = r.Debe })
            .ToList();

        return new AsientoPlanificado(
            Leyenda: tipo == TipoDeContrapartida.Reversa
                ? LeyendaDeReversa(original.Numero)
                : LeyendaDeAnulacion(original.Leyenda),
            Observaciones: tipo == TipoDeContrapartida.Reversa
                ? original.Observaciones
                : $"{PrefijoAnulacion}{original.Observaciones}",
            FecContable: fecha,
            IdTipoAsiento: original.IdTipoAsiento,
            Renglones: invertidos,
            IdAsientoRel: original.IdAsiento);
    }
}
