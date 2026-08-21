using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// La partida doble: el debe tiene que igualar al haber.
/// </summary>
public class ReglasBalanceAsientoTests
{
    private static RenglonAsiento Debe(decimal importe, int cuenta = 10259) => new(cuenta, importe, null);

    private static RenglonAsiento Haber(decimal importe, int cuenta = 10094) => new(cuenta, null, importe);

    [Fact]
    public void Un_asiento_cuadrado_pasa()
    {
        // Es el asiento 209998 real del stage.
        var renglones = new[] { Debe(2500000m), Haber(2500000m) };

        ReglasBalanceAsiento.Validar(renglones).EsValida.Should().BeTrue();
        ReglasBalanceAsiento.Cuadra(renglones).Should().BeTrue();
    }

    [Fact]
    public void Un_asiento_descuadrado_dice_cuanto_falta()
    {
        var validacion = ReglasBalanceAsiento.Validar([Debe(1000m), Haber(900m)]);

        validacion.EsValida.Should().BeFalse();
        validacion.Mensaje.Should().Contain("100");
    }

    [Fact]
    public void Cuadra_con_varios_renglones_de_cada_lado()
    {
        var renglones = new[] { Debe(600m), Debe(400m), Haber(250m), Haber(750m) };

        ReglasBalanceAsiento.Validar(renglones).EsValida.Should().BeTrue();
    }

    [Fact]
    public void El_redondeo_a_dos_decimales_es_explicito_y_no_lo_decide_la_base()
    {
        // El ABM viejo capturaba cinco decimales contra un NUMBER(18,2): Oracle truncaba en
        // silencio y lo que en pantalla cuadraba podia quedar descuadrado al grabarse.
        ReglasBalanceAsiento.Redondear(10.005m).Should().Be(10.00m);
        ReglasBalanceAsiento.Redondear(10.015m).Should().Be(10.02m);
        ReglasBalanceAsiento.Redondear(10.12345m).Should().Be(10.12m);
    }

    [Fact]
    public void Dos_importes_que_solo_difieren_en_el_tercer_decimal_se_consideran_iguales()
    {
        ReglasBalanceAsiento.Cuadra([Debe(100.001m), Haber(100.004m)]).Should().BeTrue();
    }

    [Fact]
    public void Un_renglon_no_puede_tener_importe_en_los_dos_lados()
    {
        var validacion = ReglasBalanceAsiento.Validar([new RenglonAsiento(10259, 100m, 100m), Haber(100m)]);

        validacion.Errores.Should().Contain(e => e.Contains("debe y en el haber"));
    }

    [Fact]
    public void Un_renglon_sin_importe_se_rechaza()
    {
        var validacion = ReglasBalanceAsiento.Validar([Debe(100m), new RenglonAsiento(10094, null, null)]);

        validacion.Errores.Should().Contain(e => e.Contains("no tiene importe"));
    }

    [Fact]
    public void No_se_admiten_importes_negativos()
    {
        var validacion = ReglasBalanceAsiento.Validar([Debe(-100m), Haber(-100m)]);

        validacion.Errores.Should().Contain(e => e.Contains("negativo"));
    }

    [Fact]
    public void Un_asiento_de_un_solo_renglon_no_es_un_asiento()
    {
        ReglasBalanceAsiento.Validar([Debe(100m)])
            .Errores.Should().Contain(e => e.Contains("al menos dos renglones"));
    }

    [Fact]
    public void Un_asiento_sin_renglones_se_rechaza()
    {
        ReglasBalanceAsiento.Validar([]).Errores.Should().Contain(e => e.Contains("no tiene renglones"));
    }

    [Fact]
    public void El_renglon_sin_cuenta_se_rechaza()
    {
        ReglasBalanceAsiento.Validar([new RenglonAsiento(0, 100m, null), Haber(100m)])
            .Errores.Should().Contain(e => e.Contains("no tiene cuenta"));
    }

    [Fact]
    public void Propone_la_contrapartida_que_deja_el_asiento_cuadrado()
    {
        // Sobra debe: lo que falta va al haber.
        ReglasBalanceAsiento.ContrapartidaQueCuadra([Debe(1000m), Haber(400m)])
            .Should().Be(("Haber", 600m));

        // Sobra haber: lo que falta va al debe.
        ReglasBalanceAsiento.ContrapartidaQueCuadra([Debe(400m), Haber(1000m)])
            .Should().Be(("Debe", 600m));
    }

    [Fact]
    public void Un_asiento_ya_cuadrado_no_necesita_contrapartida()
    {
        ReglasBalanceAsiento.ContrapartidaQueCuadra([Debe(500m), Haber(500m)]).Should().BeNull();
    }
}
