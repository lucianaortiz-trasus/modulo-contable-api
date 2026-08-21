using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Balance de sumas y saldos: consolidacion de movimientos hacia arriba del arbol.
/// </summary>
public class ReglasSumasYSaldosTests
{
    private sealed record Cuenta(int Id, int? IdPadre, string? Codigo, int? Nivel, bool EsImputable, string? Nombre)
        : INodoPlanDeCuentas, ICuentaConNombre;

    /// <summary>Arbol chico con la misma forma que el plan real: nivel 1 sin ceros, el resto con.</summary>
    private static Cuenta[] PlanDePrueba() =>
    [
        new(1, null, "1", 1, false, "ACTIVO"),
        new(2, 1, "01.01", 2, false, "Activo Corriente"),
        new(3, 2, "01.01.01", 3, false, "Disponibilidades"),
        new(4, 3, "01.01.01.01", 4, true, "Caja"),
        new(5, 3, "01.01.01.02", 4, true, "Bancos"),
        new(6, null, "4", 1, false, "INGRESOS"),
        new(7, 6, "04.01", 2, true, "Ingresos Operativos"),
    ];

    private static FilaSumasYSaldos Fila(List<FilaSumasYSaldos> filas, string codigo)
        => filas.Single(f => f.Codigo == codigo);

    [Fact]
    public void Las_cuentas_de_nivel_uno_reciben_el_total_de_su_rama()
    {
        // Es el caso que el reporte viejo falla. Acumulaba con `codigo LIKE padre || '%'`, y como
        // el nivel 1 guarda "1" mientras el nivel 2 guarda "01.01", el LIKE no engancha: ACTIVO,
        // PASIVO, INGRESOS y EGRESOS salian en cero. Verificado contra el stage.
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [
                new MovimientoDeCuenta(4, DebePeriodo: 1000m, HaberPeriodo: 400m, DebeAcumulado: 0m, HaberAcumulado: 0m),
                new MovimientoDeCuenta(5, DebePeriodo: 500m, HaberPeriodo: 100m, DebeAcumulado: 0m, HaberAcumulado: 0m),
            ]);

        Fila(filas, "1").DebePeriodo.Should().Be(1500m);
        Fila(filas, "1").HaberPeriodo.Should().Be(500m);
    }

    [Fact]
    public void Cada_nivel_intermedio_suma_lo_de_sus_hijas()
    {
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [
                new MovimientoDeCuenta(4, 1000m, 400m, 0m, 0m),
                new MovimientoDeCuenta(5, 500m, 100m, 0m, 0m),
            ]);

        Fila(filas, "01.01.01").DebePeriodo.Should().Be(1500m);
        Fila(filas, "01.01").DebePeriodo.Should().Be(1500m);
        Fila(filas, "01.01.01.01").DebePeriodo.Should().Be(1000m, "una hoja lleva solo lo suyo");
    }

    [Fact]
    public void El_acumulado_anterior_se_consolida_igual_que_el_periodo()
    {
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [new MovimientoDeCuenta(4, 100m, 0m, DebeAcumulado: 700m, HaberAcumulado: 200m)]);

        Fila(filas, "1").DebeAcumulado.Should().Be(700m);
        Fila(filas, "1").HaberAcumulado.Should().Be(200m);
    }

    [Fact]
    public void El_saldo_cae_de_un_solo_lado()
    {
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [
                new MovimientoDeCuenta(4, 1000m, 400m, 0m, 0m),
                new MovimientoDeCuenta(7, 0m, 900m, 0m, 0m),
            ]);

        var caja = Fila(filas, "01.01.01.01");
        caja.SaldoDeudor.Should().Be(600m);
        caja.SaldoAcreedor.Should().Be(0m);

        var ingresos = Fila(filas, "04.01");
        ingresos.SaldoAcreedor.Should().Be(900m);
        ingresos.SaldoDeudor.Should().Be(0m);
    }

    [Fact]
    public void El_total_del_debe_suma_el_acumulado_mas_el_periodo()
    {
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [new MovimientoDeCuenta(4, 100m, 30m, 700m, 200m)]);

        var caja = Fila(filas, "01.01.01.01");
        caja.DebeTotal.Should().Be(800m);
        caja.HaberTotal.Should().Be(230m);
        caja.SaldoDeudor.Should().Be(570m);
    }

    [Fact]
    public void Los_totales_del_balance_cuadran_y_solo_cuentan_las_hojas()
    {
        // Sumar tambien las cuentas de agrupacion contaria cada importe una vez por nivel.
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [
                new MovimientoDeCuenta(4, 1000m, 0m, 0m, 0m),
                new MovimientoDeCuenta(7, 0m, 1000m, 0m, 0m),
            ]);

        var (deudor, acreedor, cuadra) = ReglasSumasYSaldos.Totales(filas);

        deudor.Should().Be(1000m);
        acreedor.Should().Be(1000m);
        cuadra.Should().BeTrue();
    }

    [Fact]
    public void El_nivel_maximo_recorta_el_detalle_sin_perder_los_totales_de_arriba()
    {
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [new MovimientoDeCuenta(4, 1000m, 0m, 0m, 0m)],
            nivelMaximo: 3);

        filas.Should().NotContain(f => f.Nivel > 3);
        Fila(filas, "01.01.01").DebePeriodo.Should().Be(1000m, "el total sigue estando aunque no se vea el detalle");
    }

    [Fact]
    public void Se_pueden_descartar_las_cuentas_sin_movimiento()
    {
        var filas = ReglasSumasYSaldos.Armar(
            PlanDePrueba(),
            [new MovimientoDeCuenta(4, 1000m, 0m, 0m, 0m)],
            soloConMovimiento: true);

        filas.Should().NotContain(f => f.Codigo == "04.01");
        filas.Should().Contain(f => f.Codigo == "01.01.01.01");
    }

    [Fact]
    public void Las_filas_salen_ordenadas_por_codigo_normalizado()
    {
        var filas = ReglasSumasYSaldos.Armar(PlanDePrueba(), []);

        filas.Select(f => f.Codigo)
            .Should().ContainInOrder("1", "01.01", "01.01.01", "01.01.01.01", "01.01.01.02", "4", "04.01");
    }

    [Fact]
    public void Una_cuenta_que_se_apunta_a_si_misma_no_cuelga_la_consolidacion()
    {
        // El plan real tiene una: la 10309 "Honorarios Profesionales".
        var plan = new Cuenta[]
        {
            new(1, null, "05.01", 2, false, "EGRESOS OPERATIVOS"),
            new(2, 2, "05.01.01", 3, true, "Se apunta a si misma"),
        };

        var filas = ReglasSumasYSaldos.Armar(plan, [new MovimientoDeCuenta(2, 500m, 0m, 0m, 0m)]);

        filas.Should().HaveCount(2);
        filas.Single(f => f.Codigo == "05.01.01").DebePeriodo.Should().Be(500m);
    }

    [Fact]
    public void Un_plan_sin_movimientos_da_todas_las_filas_en_cero()
    {
        var filas = ReglasSumasYSaldos.Armar(PlanDePrueba(), []);

        filas.Should().HaveCount(7);
        filas.Should().OnlyContain(f => !f.TieneMovimiento);
    }
}

/// <summary>
/// El caso de las cuentas marcadas como imputables que ademas tienen hijas.
/// </summary>
public class SumasYSaldosConCuentasImputablesConHijasTests
{
    private sealed record Cuenta(int Id, int? IdPadre, string? Codigo, int? Nivel, bool EsImputable, string? Nombre)
        : INodoPlanDeCuentas, ICuentaConNombre;

    [Fact]
    public void Los_totales_no_cuentan_dos_veces_a_una_imputable_que_tiene_hijas()
    {
        // El plan de Morteros tiene tres cuentas asi (defecto D28). Su fila consolidada incluye
        // los importes de sus hijas, asi que sumarla junto con ellas descuadra el balance. Con los
        // 400 asientos de prueba la diferencia era de 358.931,09.
        var plan = new Cuenta[]
        {
            new(1, null, "01.01", 2, false, "Agrupadora"),
            new(2, 1, "01.01.05", 3, true, "Imputable con hijas"),
            new(3, 2, "01.01.05.01", 4, true, "Hija"),
            new(4, null, "04.01", 2, true, "Contrapartida"),
        };

        var filas = ReglasSumasYSaldos.Armar(
            plan,
            [
                new MovimientoDeCuenta(3, DebePeriodo: 1000m, HaberPeriodo: 0m, DebeAcumulado: 0m, HaberAcumulado: 0m),
                new MovimientoDeCuenta(4, DebePeriodo: 0m, HaberPeriodo: 1000m, DebeAcumulado: 0m, HaberAcumulado: 0m),
            ]);

        // La cuenta 2 es imputable y su fila trae los 1000 de su hija.
        var conHijas = filas.Single(f => f.Codigo == "01.01.05");
        conHijas.Imputable.Should().BeTrue();
        conHijas.EsHoja.Should().BeFalse();
        conHijas.DebePeriodo.Should().Be(1000m);

        // Aun asi el balance tiene que cuadrar: los totales solo miran las hojas.
        var (deudor, acreedor, cuadra) = ReglasSumasYSaldos.Totales(filas);

        deudor.Should().Be(1000m);
        acreedor.Should().Be(1000m);
        cuadra.Should().BeTrue();
    }

    [Fact]
    public void Una_hoja_no_imputable_igual_entra_en_los_totales()
    {
        // Si una cuenta sin hijas recibio movimientos, su saldo es real aunque la marca de
        // imputable diga otra cosa. Esconderlo descuadraria el balance.
        var plan = new Cuenta[]
        {
            new(1, null, "01.01", 2, false, "Hoja no imputable"),
            new(2, null, "04.01", 2, true, "Contrapartida"),
        };

        var filas = ReglasSumasYSaldos.Armar(
            plan,
            [
                new MovimientoDeCuenta(1, 500m, 0m, 0m, 0m),
                new MovimientoDeCuenta(2, 0m, 500m, 0m, 0m),
            ]);

        ReglasSumasYSaldos.Totales(filas).Cuadra.Should().BeTrue();
    }
}
