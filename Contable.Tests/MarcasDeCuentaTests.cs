using Contable.Domain.ModelsEjecucion;
using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Fija las convenciones de las marcas de un caracter de T_CUENTAS_CONTABLES.
/// </summary>
/// <remarks>
/// La tabla no usa una sola convencion: ACTIVA va con 'A' / 'I' mientras IMPUTABLE, AUXILIAR,
/// ASIENTO_OPS e INVENTARIABLE van con 'S' / 'N'. Es contraintuitivo y facil de "corregir" por
/// error, asi que queda cubierto: las 350 cuentas del plan tienen ACTIVA = 'A', y unificar la
/// convencion sin migrar los datos dejaria el plan entero como inactivo.
/// </remarks>
public class MarcasDeCuentaTests
{
    [Fact]
    public void Activa_usa_A_e_I_y_no_S_y_N()
    {
        ReglasJerarquiaCuentas.CuentaActiva.Should().Be("A");
        ReglasJerarquiaCuentas.CuentaInactiva.Should().Be("I");
    }

    [Fact]
    public void Las_demas_marcas_usan_S_y_N()
    {
        ReglasJerarquiaCuentas.MarcaSi.Should().Be("S");
        ReglasJerarquiaCuentas.MarcaNo.Should().Be("N");
    }

    [Theory]
    [InlineData("A", true)]
    [InlineData("I", false)]
    [InlineData("S", false)]
    [InlineData("N", false)]
    [InlineData(null, false)]
    public void EstaActiva_solo_es_verdadero_con_la_marca_A(string? activa, bool esperado)
    {
        var cuenta = new TCuentaContable { Codigo = "01.01", Nombre = "x", Activa = activa!, TipoSaldo = "D", Imputable = "N" };

        cuenta.EstaActiva.Should().Be(esperado);
    }

    [Theory]
    [InlineData("S", true)]
    [InlineData("N", false)]
    [InlineData("A", false)]
    [InlineData(null, false)]
    public void EsImputable_solo_es_verdadero_con_la_marca_S(string? imputable, bool esperado)
    {
        var cuenta = new TCuentaContable { Codigo = "01.01", Nombre = "x", Activa = "A", TipoSaldo = "D", Imputable = imputable! };

        cuenta.EsImputable.Should().Be(esperado);
    }

    [Fact]
    public void El_tipo_de_saldo_acreedor_comparte_la_letra_A_con_la_marca_de_activa()
    {
        // Son columnas distintas con el mismo caracter: TIPO_SALDO = 'A' es acreedor, ACTIVA = 'A'
        // es habilitada. No hay que mezclarlas.
        ReglasJerarquiaCuentas.SaldoAcreedor.Should().Be(ReglasJerarquiaCuentas.CuentaActiva);
    }
}
