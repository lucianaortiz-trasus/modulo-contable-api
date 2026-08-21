using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Formato posicional del codigo de cuenta: NN.NN.NN.NN.NN.NN.NNNN.
/// </summary>
public class ReglasCodigoCuentaTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 5)]
    [InlineData(3, 8)]
    [InlineData(4, 11)]
    [InlineData(5, 14)]
    [InlineData(6, 17)]
    [InlineData(7, 22)]
    public void El_largo_normalizado_sale_de_sumar_los_anchos_de_cada_nivel(int nivel, int largo)
    {
        CodigoCuenta.LargoNormalizado(nivel).Should().Be(largo);
    }

    [Fact]
    public void El_nivel_siete_lleva_cuatro_digitos_y_no_dos()
    {
        // El PL/SQL truncaba con nivel*2+nivel-1, que da 20 para el nivel 7 en vez de 22, y se
        // comia los dos ultimos digitos del septimo segmento. Ver defecto D8.
        CodigoCuenta.AnchoDeSegmento[6].Should().Be(4);
        CodigoCuenta.ValorMaximo(7).Should().Be(9999);
        CodigoCuenta.ValorMaximo(6).Should().Be(99);
    }

    [Theory]
    [InlineData("1", "01")]
    [InlineData("01.01", "01.01")]
    [InlineData("1.1.1", "01.01.01")]
    [InlineData("05.01.03.04.01", "05.01.03.04.01")]
    public void Normalizar_completa_cada_segmento_con_ceros(string entrada, string esperado)
    {
        CodigoCuenta.Normalizar(entrada).Should().Be(esperado);
    }

    [Fact]
    public void El_codigo_del_nivel_uno_no_es_prefijo_del_nivel_dos_sin_normalizar()
    {
        // Este es el detalle que obliga a armar el arbol por la relacion padre-hijo: en la base la
        // cuenta ACTIVO figura como "1" y su hija como "01.01".
        "01.01".StartsWith("1.").Should().BeFalse();

        // Normalizado si cuelga, que es como hay que comparar siempre.
        CodigoCuenta.CuelgaDe("01.01", "1").Should().BeTrue();
    }

    [Theory]
    [InlineData("01.01.01", "01.01", true)]
    [InlineData("01.02.01", "01.01", false)]
    [InlineData("01.01", "01.01", false)]
    [InlineData("01.010", "01.01", false)]
    public void CuelgaDe_solo_acepta_el_prefijo_completo_hasta_el_separador(string hijo, string padre, bool esperado)
    {
        CodigoCuenta.CuelgaDe(hijo, padre).Should().Be(esperado);
    }

    [Fact]
    public void Componer_arma_el_codigo_desde_los_valores_de_cada_nivel()
    {
        CodigoCuenta.Componer([1, 1, 5, 1, 1]).Should().Be("01.01.05.01.01");
        CodigoCuenta.Componer([5]).Should().Be("05");
        CodigoCuenta.Componer([1, 1, 1, 1, 1, 1, 25]).Should().Be("01.01.01.01.01.01.0025");
    }

    [Fact]
    public void Componer_rechaza_un_segmento_que_no_entra_en_su_ancho()
    {
        var accion = () => CodigoCuenta.Componer([1, 100]);
        accion.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ComponerDesdePadre_normaliza_el_prefijo_del_padre()
    {
        // El padre de nivel 1 esta guardado como "1", pero la hija tiene que quedar "01.03".
        CodigoCuenta.ComponerDesdePadre("1", 2, 3).Should().Be("01.03");
        CodigoCuenta.ComponerDesdePadre("01.01.05", 4, 12).Should().Be("01.01.05.12");
    }

    [Fact]
    public void ComponerDesdePadre_no_pide_padre_para_una_raiz()
    {
        CodigoCuenta.ComponerDesdePadre(null, 1, 6).Should().Be("06");
    }

    [Theory]
    [InlineData("01.01.05.01.01", 5)]
    [InlineData("1", 1)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void El_nivel_se_deduce_de_la_cantidad_de_segmentos(string? codigo, int nivel)
    {
        CodigoCuenta.NivelSegunCodigo(codigo).Should().Be(nivel);
    }

    [Theory]
    [InlineData("01.02.03", 2, 2)]
    [InlineData("01.02.03", 3, 3)]
    [InlineData("01.02.03", 4, null)]
    [InlineData("01.AB", 2, null)]
    public void SegmentoDeNivel_devuelve_el_valor_del_nivel_pedido(string codigo, int nivel, int? esperado)
    {
        CodigoCuenta.SegmentoDeNivel(codigo, nivel).Should().Be(esperado);
    }

    [Theory]
    [InlineData("01.01", true)]
    [InlineData("1", true)]
    [InlineData("01.01.01.01.01.01.0001", true)]
    [InlineData("01.1", false)]
    [InlineData("01.01.01.01.01.01.01.01", false)]
    [InlineData("AB.01", false)]
    [InlineData("", false)]
    public void TieneFormatoValido_controla_ancho_y_digitos(string codigo, bool esperado)
    {
        CodigoCuenta.TieneFormatoValido(codigo).Should().Be(esperado);
    }
}
