using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Cierre y apertura de ejercicio, con los parametros contables externalizados.
/// </summary>
public class ReglasCierreEjercicioTests
{
    /// <summary>Configuracion propuesta para Morteros: la 10176 es "Resultado del ejercicio".</summary>
    private static ConfiguracionCierre Config(int cuentaBalanceo = 10176, int tipoApertura = 7) => new()
    {
        CuentaBalanceo = cuentaBalanceo,
        UnidadAdministrativa = 1619,
        RecursoBalanceoHaber = 3,
        RecursoBalanceoDebe = 4,
        NaturalezasResultado = [4, 5],
        NaturalezasPatrimoniales = [1, 2],
        TipoAsientoCierre = 6,
        TipoAsientoApertura = tipoApertura,
    };

    private static SaldoDeCuenta Deudor(int cuenta, decimal saldo, int naturaleza)
        => new(cuenta, saldo, "D", naturaleza, IdRecursoDebe: 4, IdRecursoHaber: 3);

    private static SaldoDeCuenta Acreedor(int cuenta, decimal saldo, int naturaleza)
        => new(cuenta, saldo, "H", naturaleza, IdRecursoDebe: 4, IdRecursoHaber: 3);

    private static IEnumerable<RenglonAsiento> ComoRenglones(AsientoPlanificado asiento)
        => asiento.Renglones.Select(r => new RenglonAsiento(r.IdCtaContable, r.Debe, r.Haber));

    // ---- Cierre ----------------------------------------------------------

    [Fact]
    public void El_cierre_genera_dos_asientos_uno_de_resultado_y_otro_patrimonial()
    {
        var saldos = new[]
        {
            Deudor(10205, 1000m, 5),   // EGRESOS
            Acreedor(10186, 1000m, 4), // INGRESOS
            Deudor(10001, 500m, 1),    // ACTIVO
            Acreedor(10091, 500m, 2),  // PASIVO
        };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        plan.Validacion.EsValida.Should().BeTrue(plan.Validacion.Mensaje);
        plan.Asientos.Should().HaveCount(2);
        plan.Resultado.Should().NotBeNull();
        plan.Patrimonial.Should().NotBeNull();
    }

    [Fact]
    public void Cada_cuenta_se_cancela_contra_el_lado_opuesto_de_su_saldo()
    {
        var saldos = new[] { Deudor(10205, 1000m, 5), Acreedor(10186, 1000m, 4) };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        var deudora = plan.Resultado!.Renglones.Single(r => r.IdCtaContable == 10205);
        deudora.Haber.Should().Be(1000m);
        deudora.Debe.Should().BeNull();
        deudora.IdRecursoDh.Should().Be(3, "una cuenta que se acredita usa el recurso del haber");

        var acreedora = plan.Resultado!.Renglones.Single(r => r.IdCtaContable == 10186);
        acreedora.Debe.Should().Be(1000m);
        acreedora.Haber.Should().BeNull();
        acreedora.IdRecursoDh.Should().Be(4);
    }

    [Fact]
    public void El_descuadre_residual_va_entero_a_la_cuenta_de_balanceo()
    {
        // Deudores 1000, acreedores 400: el asiento queda con 1000 al haber y 400 al debe.
        var saldos = new[] { Deudor(10205, 1000m, 5), Acreedor(10186, 400m, 4) };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        var balanceo = plan.Resultado!.Renglones.Single(r => r.IdCtaContable == 10176);
        balanceo.Debe.Should().Be(600m);
        balanceo.IdRecursoDh.Should().Be(4);
    }

    [Fact]
    public void El_balanceo_cae_del_otro_lado_cuando_pesan_mas_los_acreedores()
    {
        var saldos = new[] { Deudor(10205, 400m, 5), Acreedor(10186, 1000m, 4) };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        var balanceo = plan.Resultado!.Renglones.Single(r => r.IdCtaContable == 10176);
        balanceo.Haber.Should().Be(600m);
        balanceo.IdRecursoDh.Should().Be(3);
    }

    [Fact]
    public void Los_asientos_de_cierre_siempre_quedan_cuadrados()
    {
        var saldos = new[]
        {
            Deudor(10205, 1234.56m, 5),
            Acreedor(10186, 987.65m, 4),
            Deudor(10001, 500m, 1),
            Acreedor(10091, 1500m, 2),
        };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        foreach (var asiento in plan.Asientos)
        {
            ReglasBalanceAsiento.Cuadra(ComoRenglones(asiento))
                .Should().BeTrue($"el asiento '{asiento.Leyenda}' tiene que cuadrar");
        }
    }

    [Fact]
    public void El_patrimonio_neto_queda_fuera_del_cierre_porque_es_donde_cae_el_balanceo()
    {
        var saldos = new[]
        {
            Deudor(10205, 1000m, 5),
            Acreedor(10186, 1000m, 4),
            Acreedor(10176, 700m, 3), // naturaleza 3, patrimonio neto
        };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        plan.Resultado!.Renglones.Should().NotContain(r => r.IdCtaContable == 10176);
        plan.Patrimonial.Should().BeNull("no hay cuentas de naturaleza 1 o 2 con saldo");
    }

    [Fact]
    public void El_cierre_se_fecha_el_31_de_diciembre_del_ejercicio()
    {
        var plan = ReglasCierreEjercicio.PlanificarCierre(
            [Deudor(10205, 100m, 5), Acreedor(10186, 100m, 4)], 2026, Config(), yaTieneCierre: false);

        plan.Resultado!.FecContable.Should().Be(new DateTime(2026, 12, 31));
        plan.Resultado!.IdTipoAsiento.Should().Be(6);
        plan.Resultado!.Leyenda.Should().Be("ASIENTO DE CIERRE");
    }

    [Fact]
    public void Las_cuentas_con_saldo_cero_no_entran_al_cierre()
    {
        var saldos = new[] { Deudor(10205, 100m, 5), Acreedor(10186, 100m, 4), Deudor(10207, 0m, 5) };

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, 2026, Config(), yaTieneCierre: false);

        plan.Resultado!.Renglones.Should().NotContain(r => r.IdCtaContable == 10207);
    }

    [Fact]
    public void No_se_cierra_dos_veces_el_mismo_ejercicio()
    {
        var plan = ReglasCierreEjercicio.PlanificarCierre(
            [Deudor(10205, 100m, 5)], 2026, Config(), yaTieneCierre: true);

        plan.Validacion.Errores.Should().Contain(e => e.Contains("ya tiene asiento de cierre"));
        plan.SePuedeGenerar.Should().BeFalse();
    }

    [Fact]
    public void Sin_saldos_no_hay_nada_que_cerrar()
    {
        var plan = ReglasCierreEjercicio.PlanificarCierre([], 2026, Config(), yaTieneCierre: false);

        plan.Validacion.Errores.Should().Contain(e => e.Contains("no tiene saldos para cerrar"));
    }

    // ---- Configuracion ---------------------------------------------------

    [Fact]
    public void Sin_cuenta_de_balanceo_configurada_el_cierre_no_corre()
    {
        // Arranca en 0 justamente para que falle con un mensaje claro en vez de imputar mal.
        var plan = ReglasCierreEjercicio.PlanificarCierre(
            [Deudor(10205, 100m, 5)], 2026, Config(cuentaBalanceo: 0), yaTieneCierre: false);

        plan.Validacion.Errores.Should().Contain(e => e.Contains("CuentaBalanceoCierre"));
    }

    [Fact]
    public void La_apertura_no_puede_usar_el_mismo_tipo_de_asiento_que_el_cierre()
    {
        // Defecto D3: el PL/SQL grababa la apertura con el tipo de cierre y despues la releia como
        // si fuera un cierre, duplicando los saldos del ejercicio siguiente.
        var validacion = Config(tipoApertura: 6).Validar();

        validacion.Errores.Should().Contain(e => e.Contains("leeria su propia apertura"));
    }

    // ---- Apertura --------------------------------------------------------

    [Fact]
    public void La_apertura_reinyecta_invertido_el_cierre_patrimonial_anterior()
    {
        var cierreAnterior = new[]
        {
            new RenglonDeCierre(10001, Debe: null, Haber: 500m, IdRecursoDebe: 4, IdRecursoHaber: 3),
            new RenglonDeCierre(10091, Debe: 500m, Haber: null, IdRecursoDebe: 4, IdRecursoHaber: 3),
        };

        var (validacion, asiento) = ReglasCierreEjercicio.PlanificarApertura(
            cierreAnterior, 2027, Config(), yaTieneApertura: false);

        validacion.EsValida.Should().BeTrue(validacion.Mensaje);
        asiento!.Renglones.Single(r => r.IdCtaContable == 10001).Debe.Should().Be(500m);
        asiento.Renglones.Single(r => r.IdCtaContable == 10091).Haber.Should().Be(500m);
    }

    [Fact]
    public void Al_invertir_el_renglon_el_recurso_pasa_al_otro_lado()
    {
        // No se arrastra el recurso del cierre: se vuelve a tomar de la cuenta, igual que el PL/SQL.
        var cierreAnterior = new[]
        {
            new RenglonDeCierre(10001, Debe: 500m, Haber: null, IdRecursoDebe: 44, IdRecursoHaber: 33),
        };

        var (_, asiento) = ReglasCierreEjercicio.PlanificarApertura(
            cierreAnterior, 2027, Config(), yaTieneApertura: false);

        var renglon = asiento!.Renglones.Single();
        renglon.Haber.Should().Be(500m);
        renglon.IdRecursoDh.Should().Be(33, "el renglon paso al haber");
    }

    [Fact]
    public void La_apertura_se_fecha_el_primero_de_enero_y_lleva_su_propio_tipo()
    {
        var cierreAnterior = new[] { new RenglonDeCierre(10001, null, 500m, 4, 3) };

        var (_, asiento) = ReglasCierreEjercicio.PlanificarApertura(
            cierreAnterior, 2027, Config(), yaTieneApertura: false);

        asiento!.FecContable.Should().Be(new DateTime(2027, 1, 1));
        asiento.IdTipoAsiento.Should().Be(7, "la apertura no se graba con el tipo de cierre");
        asiento.Leyenda.Should().Be("ASIENTO DE APERTURA");
    }

    [Fact]
    public void La_apertura_queda_cuadrada()
    {
        var cierreAnterior = new[]
        {
            new RenglonDeCierre(10001, null, 1500m, 4, 3),
            new RenglonDeCierre(10091, 900m, null, 4, 3),
            new RenglonDeCierre(10176, 600m, null, 4, 3),
        };

        var (_, asiento) = ReglasCierreEjercicio.PlanificarApertura(
            cierreAnterior, 2027, Config(), yaTieneApertura: false);

        ReglasBalanceAsiento.Cuadra(ComoRenglones(asiento!)).Should().BeTrue();
    }

    [Fact]
    public void No_se_abre_dos_veces_el_mismo_ejercicio()
    {
        var (validacion, asiento) = ReglasCierreEjercicio.PlanificarApertura(
            [new RenglonDeCierre(10001, null, 500m, 4, 3)], 2027, Config(), yaTieneApertura: true);

        validacion.Errores.Should().Contain(e => e.Contains("ya tiene asiento de apertura"));
        asiento.Should().BeNull();
    }

    [Fact]
    public void Sin_cierre_anterior_no_hay_apertura()
    {
        var (validacion, _) = ReglasCierreEjercicio.PlanificarApertura(
            [], 2027, Config(), yaTieneApertura: false);

        validacion.Errores.Should().Contain(e => e.Contains("no tiene asiento de cierre patrimonial"));
    }
}
