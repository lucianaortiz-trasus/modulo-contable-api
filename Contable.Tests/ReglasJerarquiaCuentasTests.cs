using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Validaciones estructurales del plan de cuentas al dar de alta, editar o eliminar.
/// </summary>
public class ReglasJerarquiaCuentasTests
{
    private static DatosCuenta Cuenta(
        string codigo = "01.01.01",
        string nombre = "Caja",
        int nivel = 3,
        string tipoSaldo = "D",
        bool imputable = false,
        bool activa = true)
        => new(codigo, nombre, nivel, tipoSaldo, imputable, activa);

    [Fact]
    public void Un_alta_bien_formada_pasa()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(),
            new ContextoCuenta(PadreExiste: true, CodigoPadre: "01.01", NivelPadre: 2));

        validacion.EsValida.Should().BeTrue(validacion.Mensaje);
    }

    [Fact]
    public void El_nombre_y_el_codigo_son_obligatorios()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(codigo: "", nombre: "  "),
            new ContextoCuenta(PadreExiste: true, CodigoPadre: "01.01", NivelPadre: 2));

        validacion.EsValida.Should().BeFalse();
        validacion.Errores.Should().Contain(e => e.Contains("nombre"));
        validacion.Errores.Should().Contain(e => e.Contains("codigo"));
    }

    [Fact]
    public void No_se_repite_un_codigo_ya_usado()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(),
            new ContextoCuenta(CodigoYaExiste: true, PadreExiste: true, CodigoPadre: "01.01", NivelPadre: 2));

        validacion.Errores.Should().Contain(e => e.Contains("Ya existe"));
    }

    [Fact]
    public void El_nivel_declarado_tiene_que_coincidir_con_los_segmentos_del_codigo()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(codigo: "01.01.01", nivel: 4),
            new ContextoCuenta(PadreExiste: true, CodigoPadre: "01.01.01", NivelPadre: 3));

        validacion.Errores.Should().Contain(e => e.Contains("segmento"));
    }

    [Fact]
    public void Una_cuenta_no_puede_colgar_de_una_de_su_mismo_nivel()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(codigo: "01.01.02", nivel: 3),
            new ContextoCuenta(PadreExiste: true, CodigoPadre: "01.01.01", NivelPadre: 3));

        validacion.Errores.Should().Contain(e => e.Contains("nivel 2"));
    }

    [Fact]
    public void No_se_le_cuelgan_hijas_a_una_cuenta_imputable()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(),
            new ContextoCuenta(PadreExiste: true, CodigoPadre: "01.01", NivelPadre: 2, PadreEsImputable: true));

        validacion.Errores.Should().Contain(e => e.Contains("imputable"));
    }

    [Fact]
    public void El_codigo_de_la_hija_tiene_que_colgar_del_codigo_del_padre()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(codigo: "09.09.09"),
            new ContextoCuenta(PadreExiste: true, CodigoPadre: "01.01", NivelPadre: 2));

        validacion.Errores.Should().Contain(e => e.Contains("no cuelga"));
    }

    [Fact]
    public void Una_raiz_no_necesita_padre()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(codigo: "06", nivel: 1, tipoSaldo: "A"),
            new ContextoCuenta());

        validacion.EsValida.Should().BeTrue(validacion.Mensaje);
    }

    [Theory]
    [InlineData("D", true)]
    [InlineData("A", true)]
    [InlineData("X", false)]
    [InlineData(null, false)]
    public void El_tipo_de_saldo_solo_admite_deudor_o_acreedor(string? tipoSaldo, bool esValido)
    {
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(
            Cuenta(codigo: "06", nivel: 1, tipoSaldo: tipoSaldo!),
            new ContextoCuenta());

        validacion.EsValida.Should().Be(esValido, validacion.Mensaje);
    }

    [Fact]
    public void No_se_marca_imputable_una_cuenta_que_ya_tiene_hijas()
    {
        // Es la anomalia que tienen tres cuentas del plan de Morteros. Se admite lo que ya esta
        // cargado, pero no se deja crear una nueva.
        var validacion = ReglasJerarquiaCuentas.ValidarEdicion(
            Cuenta(imputable: true),
            new ContextoCuenta(CantidadDeHijas: 3));

        validacion.Errores.Should().Contain(e => e.Contains("hija"));
    }

    [Fact]
    public void Se_puede_marcar_imputable_una_cuenta_hoja()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarEdicion(
            Cuenta(imputable: true),
            new ContextoCuenta(CantidadDeHijas: 0));

        validacion.EsValida.Should().BeTrue(validacion.Mensaje);
    }

    [Fact]
    public void No_se_elimina_una_cuenta_con_hijas()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarEliminacion(new ContextoCuenta(CantidadDeHijas: 2));

        validacion.EsValida.Should().BeFalse();
        validacion.Errores.Should().Contain(e => e.Contains("hija"));
    }

    [Fact]
    public void No_se_elimina_una_cuenta_con_asientos_imputados()
    {
        var validacion = ReglasJerarquiaCuentas.ValidarEliminacion(new ContextoCuenta(TieneMovimientos: true));

        validacion.EsValida.Should().BeFalse();
        validacion.Errores.Should().Contain(e => e.Contains("asientos"));
    }

    [Fact]
    public void Una_hoja_sin_movimientos_se_puede_eliminar()
    {
        ReglasJerarquiaCuentas.ValidarEliminacion(new ContextoCuenta()).EsValida.Should().BeTrue();
    }
}
