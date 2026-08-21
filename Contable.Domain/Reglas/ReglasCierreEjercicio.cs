namespace Contable.Domain.Reglas;

/// <summary>Saldo de una cuenta, tal como lo devuelve la vista de saldos.</summary>
/// <param name="IdCtaContable">Cuenta.</param>
/// <param name="Saldo">Importe absoluto del saldo.</param>
/// <param name="TipoSaldo">'D' si el saldo es deudor, 'H' si es acreedor. Es el saldo calculado.</param>
/// <param name="IdNaturaleza">Naturaleza de la cuenta.</param>
/// <param name="IdRecursoDebe">Recurso a usar cuando el renglon va al debe.</param>
/// <param name="IdRecursoHaber">Recurso a usar cuando el renglon va al haber.</param>
public record SaldoDeCuenta(
    int IdCtaContable,
    decimal Saldo,
    string TipoSaldo,
    int? IdNaturaleza,
    int? IdRecursoDebe,
    int? IdRecursoHaber);

/// <summary>
/// Renglon de un asiento de cierre, con los recursos de su cuenta.
/// </summary>
/// <remarks>
/// La apertura necesita los dos recursos de la cuenta, no el que quedo grabado en el renglon: al
/// invertir el importe, el recurso pasa al otro lado.
/// </remarks>
public record RenglonDeCierre(
    int IdCtaContable,
    decimal? Debe,
    decimal? Haber,
    int? IdRecursoDebe,
    int? IdRecursoHaber);

/// <summary>
/// Parametros contables del cierre, que cambian de un municipio a otro.
/// </summary>
/// <remarks>
/// En el PL/SQL provincial estaban escritos a mano dentro del procedimiento: la cuenta de
/// balanceo fija en 11270 y la unidad administrativa buscada por el literal
/// 'Administracion Central'. Ninguno de los dos existe en Morteros, y por eso el cierre estaba
/// desactivado. Ver _analisis/REGLAS.md, defectos D1 y D2.
/// </remarks>
public class ConfiguracionCierre
{
    /// <summary>Cuenta que recibe la diferencia residual. Provincial: 11270.</summary>
    public int CuentaBalanceo { get; init; }

    /// <summary>Unidad administrativa a la que se imputan cierre y apertura.</summary>
    public int UnidadAdministrativa { get; init; }

    public int RecursoBalanceoHaber { get; init; }

    public int RecursoBalanceoDebe { get; init; }

    /// <summary>Naturalezas de cuentas de resultado. Provincial: 4 y 5.</summary>
    public int[] NaturalezasResultado { get; init; } = [];

    /// <summary>Naturalezas patrimoniales. Provincial: 1 y 2.</summary>
    public int[] NaturalezasPatrimoniales { get; init; } = [];

    public int TipoAsientoCierre { get; init; }

    /// <summary>
    /// Tipo del asiento de apertura. El PL/SQL escribia la apertura con el tipo de cierre y
    /// despues la releia como si fuera un cierre: ver defecto D3.
    /// </summary>
    public int TipoAsientoApertura { get; init; }

    public Validacion Validar()
    {
        var validacion = new Validacion();

        validacion.AgregarSi(
            CuentaBalanceo <= 0,
            "Falta configurar la cuenta contable de balanceo del cierre (Contabilidad:CuentaBalanceoCierre).");

        validacion.AgregarSi(
            UnidadAdministrativa <= 0,
            "Falta configurar la unidad administrativa del cierre (Contabilidad:UnidadAdministrativaCierre).");

        validacion.AgregarSi(
            NaturalezasResultado.Length == 0,
            "Falta configurar las naturalezas de resultado (Contabilidad:NaturalezasResultado).");

        validacion.AgregarSi(
            NaturalezasPatrimoniales.Length == 0,
            "Falta configurar las naturalezas patrimoniales (Contabilidad:NaturalezasPatrimoniales).");

        validacion.AgregarSi(TipoAsientoCierre <= 0, "Falta configurar el tipo de asiento de cierre.");
        validacion.AgregarSi(TipoAsientoApertura <= 0, "Falta configurar el tipo de asiento de apertura.");

        validacion.AgregarSi(
            TipoAsientoCierre > 0 && TipoAsientoCierre == TipoAsientoApertura,
            "El tipo de asiento de apertura no puede ser el mismo que el de cierre: la apertura del " +
            "ejercicio siguiente leeria su propia apertura como si fuera un cierre.");

        return validacion;
    }
}

/// <summary>
/// Cierre y apertura de ejercicio.
/// </summary>
/// <remarks>
/// Portado de `INSERTAR_ASIENTO_CIERRE` e `INSERTAR_ASIENTO_APERTURA`, que estan implementados en
/// el cuerpo del package pero comentados en su interfaz publica.
///
/// El cierre genera <b>dos</b> asientos, no uno: primero el de resultado y despues el patrimonial.
/// Cada uno cancela cuenta por cuenta contra el lado opuesto de su saldo y manda el descuadre
/// residual a una unica cuenta de balanceo. Las cuentas de patrimonio neto quedan fuera del bucle
/// a proposito: son justamente donde cae ese balanceo.
///
/// La apertura reinyecta invertido el cierre patrimonial del ejercicio anterior.
/// </remarks>
public static class ReglasCierreEjercicio
{
    public const string LeyendaCierre = "ASIENTO DE CIERRE";

    public const string LeyendaApertura = "ASIENTO DE APERTURA";

    /// <summary>Resultado de planificar el cierre.</summary>
    public class PlanDeCierre
    {
        public required Validacion Validacion { get; init; }

        /// <summary>Asiento que cancela las cuentas de resultado.</summary>
        public AsientoPlanificado? Resultado { get; init; }

        /// <summary>Asiento que cancela las cuentas patrimoniales.</summary>
        public AsientoPlanificado? Patrimonial { get; init; }

        public List<AsientoPlanificado> Asientos =>
            [.. new[] { Resultado, Patrimonial }.Where(a => a is not null).Select(a => a!)];

        public bool SePuedeGenerar => Validacion.EsValida && Asientos.Count > 0;
    }

    /// <summary>
    /// Arma los dos asientos de cierre a partir de los saldos del ejercicio.
    /// </summary>
    /// <param name="saldos">Saldos de todas las cuentas con movimiento en el ejercicio.</param>
    /// <param name="ejercicio">Año del ejercicio que se cierra.</param>
    /// <param name="config">Parametros contables del municipio.</param>
    /// <param name="yaTieneCierre">Si el ejercicio ya fue cerrado.</param>
    public static PlanDeCierre PlanificarCierre(
        IEnumerable<SaldoDeCuenta> saldos,
        int ejercicio,
        ConfiguracionCierre config,
        bool yaTieneCierre)
    {
        var validacion = config.Validar();

        validacion.AgregarSi(
            yaTieneCierre,
            $"El ejercicio {ejercicio} ya tiene asiento de cierre. Generarlo de nuevo duplicaria los saldos.");

        var lista = saldos?.ToList() ?? [];

        if (!validacion.EsValida) return new PlanDeCierre { Validacion = validacion };

        var fecha = new DateTime(ejercicio, 12, 31);

        var resultado = ArmarAsientoDeCierre(lista, config.NaturalezasResultado, fecha, config);
        var patrimonial = ArmarAsientoDeCierre(lista, config.NaturalezasPatrimoniales, fecha, config);

        validacion.AgregarSi(
            resultado is null && patrimonial is null,
            $"El ejercicio {ejercicio} no tiene saldos para cerrar.");

        return new PlanDeCierre
        {
            Validacion = validacion,
            Resultado = resultado,
            Patrimonial = patrimonial,
        };
    }

    private static AsientoPlanificado? ArmarAsientoDeCierre(
        List<SaldoDeCuenta> saldos,
        int[] naturalezas,
        DateTime fecha,
        ConfiguracionCierre config)
    {
        var delGrupo = saldos
            .Where(s => s.IdNaturaleza is not null && naturalezas.Contains(s.IdNaturaleza.Value))
            .Where(s => ReglasBalanceAsiento.Redondear(s.Saldo) != 0)
            .ToList();

        if (delGrupo.Count == 0) return null;

        var renglones = new List<RenglonPlanificado>();

        foreach (var saldo in delGrupo)
        {
            var importe = ReglasBalanceAsiento.Redondear(saldo.Saldo);

            // Una cuenta con saldo deudor se cancela acreditandola, y al reves.
            renglones.Add(EsDeudor(saldo.TipoSaldo)
                ? new RenglonPlanificado(saldo.IdCtaContable, null, importe, saldo.IdRecursoHaber)
                : new RenglonPlanificado(saldo.IdCtaContable, importe, null, saldo.IdRecursoDebe));
        }

        var totalDeudor = ReglasBalanceAsiento.Redondear(
            delGrupo.Where(s => EsDeudor(s.TipoSaldo)).Sum(s => Math.Abs(s.Saldo)));

        var totalAcreedor = ReglasBalanceAsiento.Redondear(
            delGrupo.Where(s => !EsDeudor(s.TipoSaldo)).Sum(s => Math.Abs(s.Saldo)));

        var diferencia = ReglasBalanceAsiento.Redondear(totalDeudor - totalAcreedor);

        if (diferencia != 0)
        {
            // El descuadre residual va entero a la cuenta de balanceo, que es la que deja el
            // asiento cuadrado.
            renglones.Add(diferencia < 0
                ? new RenglonPlanificado(config.CuentaBalanceo, null, Math.Abs(diferencia), config.RecursoBalanceoHaber)
                : new RenglonPlanificado(config.CuentaBalanceo, Math.Abs(diferencia), null, config.RecursoBalanceoDebe));
        }

        return new AsientoPlanificado(
            LeyendaCierre,
            LeyendaCierre,
            fecha,
            config.TipoAsientoCierre,
            renglones);
    }

    /// <summary>
    /// Arma el asiento de apertura invirtiendo el cierre patrimonial del ejercicio anterior.
    /// </summary>
    /// <param name="renglonesDelCierreAnterior">
    /// Renglones de los asientos de cierre del ejercicio anterior, ya acotados a las cuentas
    /// patrimoniales.
    /// </param>
    /// <param name="ejercicio">Año del ejercicio que se abre.</param>
    /// <param name="config">Parametros contables del municipio.</param>
    /// <param name="yaTieneApertura">Si el ejercicio ya fue abierto.</param>
    public static (Validacion Validacion, AsientoPlanificado? Asiento) PlanificarApertura(
        IEnumerable<RenglonDeCierre> renglonesDelCierreAnterior,
        int ejercicio,
        ConfiguracionCierre config,
        bool yaTieneApertura)
    {
        var validacion = config.Validar();

        validacion.AgregarSi(
            yaTieneApertura,
            $"El ejercicio {ejercicio} ya tiene asiento de apertura. Generarlo de nuevo duplicaria los saldos iniciales.");

        var origen = renglonesDelCierreAnterior?.ToList() ?? [];

        validacion.AgregarSi(
            origen.Count == 0,
            $"El ejercicio {ejercicio - 1} no tiene asiento de cierre patrimonial: no hay saldos para abrir.");

        if (!validacion.EsValida) return (validacion, null);

        // Al invertir el renglon el recurso tambien cambia de lado: no se arrastra el del cierre,
        // se vuelve a tomar de la cuenta, igual que hace INSERTAR_ASIENTO_APERTURA.
        var invertidos = origen
            .Select(r => r.Debe is > 0
                ? new RenglonPlanificado(r.IdCtaContable, null, r.Debe, r.IdRecursoHaber)
                : new RenglonPlanificado(r.IdCtaContable, r.Haber, null, r.IdRecursoDebe))
            .ToList();

        var asiento = new AsientoPlanificado(
            LeyendaApertura,
            LeyendaApertura,
            new DateTime(ejercicio, 1, 1),
            config.TipoAsientoApertura,
            invertidos);

        return (validacion, asiento);
    }

    private static bool EsDeudor(string? tipoSaldo)
        => string.Equals(tipoSaldo, "D", StringComparison.OrdinalIgnoreCase);
}
