using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Mayor de una cuenta: saldo anterior, movimientos y saldo corriente.
/// </summary>
public class ReglasMayorTests
{
    private static MovimientoMayor Mov(int dia, decimal? debe, decimal? haber, int numero = 1, bool anulado = false)
        => new(1000 + numero, numero, new DateTime(2026, 3, dia), $"Asiento {numero}", debe, haber, anulado);

    private static Mayor Armar(decimal anterior, params MovimientoMayor[] movimientos)
        => ReglasMayor.Armar(10005, "01.01.01.01.01", "Caja en pesos", anterior, movimientos);

    [Fact]
    public void El_saldo_va_acumulando_movimiento_a_movimiento()
    {
        var mayor = Armar(0m, Mov(1, 1000m, null, 1), Mov(2, null, 400m, 2), Mov(3, 250m, null, 3));

        mayor.Renglones.Select(r => r.Saldo).Should().ContainInOrder(1000m, 600m, 850m);
        mayor.SaldoFinal.Should().Be(850m);
        mayor.TipoSaldoFinal.Should().Be("D");
    }

    [Fact]
    public void Arranca_del_saldo_que_viene_del_periodo_anterior()
    {
        var mayor = Armar(500m, Mov(1, 100m, null));

        mayor.SaldoAnterior.Should().Be(500m);
        mayor.TipoSaldoAnterior.Should().Be("D");
        mayor.SaldoFinal.Should().Be(600m);
    }

    [Fact]
    public void El_saldo_anterior_acreedor_se_informa_en_valor_absoluto_y_con_su_lado()
    {
        // Mismo criterio que la vista de saldos y que el cierre: el importe va sin signo y el
        // lado va aparte.
        var mayor = Armar(-800m);

        mayor.SaldoAnterior.Should().Be(800m);
        mayor.TipoSaldoAnterior.Should().Be("H");
    }

    [Fact]
    public void El_saldo_puede_cambiar_de_lado_en_el_medio_del_periodo()
    {
        var mayor = Armar(0m, Mov(1, 300m, null, 1), Mov(2, null, 900m, 2), Mov(3, 1000m, null, 3));

        mayor.Renglones[0].TipoSaldo.Should().Be("D");
        mayor.Renglones[1].TipoSaldo.Should().Be("H");
        mayor.Renglones[1].Saldo.Should().Be(600m);
        mayor.Renglones[2].TipoSaldo.Should().Be("D");
        mayor.Renglones[2].Saldo.Should().Be(400m);
    }

    [Fact]
    public void Los_movimientos_salen_en_orden_cronologico_aunque_lleguen_desordenados()
    {
        var mayor = Armar(0m, Mov(10, 100m, null, 3), Mov(1, 200m, null, 1), Mov(5, 300m, null, 2));

        mayor.Renglones.Select(r => r.NumeroAsiento).Should().ContainInOrder(1, 2, 3);
        mayor.Renglones.Select(r => r.Saldo).Should().ContainInOrder(200m, 500m, 600m);
    }

    [Fact]
    public void Con_la_misma_fecha_desempata_el_numero_de_asiento()
    {
        var mayor = Armar(0m, Mov(5, 100m, null, 9), Mov(5, 200m, null, 4));

        mayor.Renglones.Select(r => r.NumeroAsiento).Should().ContainInOrder(4, 9);
    }

    [Fact]
    public void Los_totales_del_periodo_suman_cada_lado_por_separado()
    {
        var mayor = Armar(0m, Mov(1, 1000m, null, 1), Mov(2, 500m, null, 2), Mov(3, null, 200m, 3));

        mayor.TotalDebe.Should().Be(1500m);
        mayor.TotalHaber.Should().Be(200m);
    }

    [Fact]
    public void Un_saldo_que_queda_en_cero_se_informa_como_deudor()
    {
        // Es una convencion, pero tiene que ser una sola y estar fijada.
        var mayor = Armar(0m, Mov(1, 500m, null, 1), Mov(2, null, 500m, 2));

        mayor.SaldoFinal.Should().Be(0m);
        mayor.TipoSaldoFinal.Should().Be("D");
    }

    [Fact]
    public void Una_cuenta_sin_movimientos_conserva_el_saldo_anterior()
    {
        var mayor = Armar(1234.56m);

        mayor.Renglones.Should().BeEmpty();
        mayor.SaldoFinal.Should().Be(1234.56m);
        mayor.TotalDebe.Should().Be(0m);
    }

    [Fact]
    public void Los_movimientos_anulados_quedan_marcados_en_el_renglon()
    {
        var mayor = Armar(0m, Mov(1, 100m, null, 1, anulado: true));

        mayor.Renglones.Single().Anulado.Should().BeTrue();
    }

    [Fact]
    public void El_redondeo_no_deja_arrastrar_decimales()
    {
        var mayor = Armar(0m, Mov(1, 10.005m, null, 1), Mov(2, 10.005m, null, 2));

        mayor.Renglones[0].Saldo.Should().Be(10.00m);
        mayor.SaldoFinal.Should().Be(20.00m);
    }
}
