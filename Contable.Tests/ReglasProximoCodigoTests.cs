using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Calculo del proximo codigo libre, que reemplaza a las FUNC_PROXIMO_NIVEL1..7 del PL/SQL.
/// </summary>
public class ReglasProximoCodigoTests
{
    [Fact]
    public void La_primera_hija_de_un_padre_sin_hijas_es_el_uno()
    {
        var resultado = ReglasProximoCodigo.Calcular("01.01", 2, []);

        resultado.Codigo.Should().Be("01.01.01");
        resultado.Nivel.Should().Be(3);
        resultado.Segmento.Should().Be(1);
    }

    [Fact]
    public void Con_hijas_correlativas_sigue_por_la_siguiente()
    {
        var resultado = ReglasProximoCodigo.Calcular("01.01", 2, ["01.01.01", "01.01.02", "01.01.03"]);

        resultado.Codigo.Should().Be("01.01.04");
    }

    [Fact]
    public void Toma_el_primer_hueco_y_no_el_maximo_mas_uno()
    {
        // Al borrar la 02 el codigo queda libre y se reutiliza: es lo que espera quien mira el arbol.
        var resultado = ReglasProximoCodigo.Calcular("01.01", 2, ["01.01.01", "01.01.03", "01.01.04"]);

        resultado.Codigo.Should().Be("01.01.02");
        resultado.Segmento.Should().Be(2);
    }

    [Fact]
    public void El_padre_de_nivel_uno_guardado_sin_ceros_igual_produce_una_hija_normalizada()
    {
        // En la base ACTIVO es "1" y sus hijas son "01.01" y "01.02".
        var resultado = ReglasProximoCodigo.Calcular("1", 1, ["01.01", "01.02"]);

        resultado.Codigo.Should().Be("01.03");
        resultado.Nivel.Should().Be(2);
    }

    [Fact]
    public void Las_hijas_cuyo_codigo_no_cuelga_del_padre_no_entran_en_el_calculo()
    {
        // El plan de Morteros tiene catorce filas asi, por relaciones mal cargadas. Un codigo que
        // no deriva del padre no dice nada sobre que segmentos estan ocupados bajo ese padre.
        var resultado = ReglasProximoCodigo.Calcular(
            "01.01.05.01",
            4,
            ["01.01.05.01.01", "01.01.02.01", "01.01.02.02"]);

        resultado.Codigo.Should().Be("01.01.05.01.02");
        resultado.HijosIgnorados.Should().Be(2);
    }

    [Fact]
    public void Cuando_el_nivel_esta_agotado_avisa_en_vez_de_reventar()
    {
        // El PL/SQL, con los 99 codigos usados, salia de la funcion sin ejecutar ningun RETURN y
        // Oracle levantaba ORA-06503. Ver defecto D6.
        var ocupados = Enumerable.Range(1, 99).Select(i => $"01.{i:D2}").ToArray();

        var resultado = ReglasProximoCodigo.Calcular("1", 1, ocupados);

        resultado.HayDisponible.Should().BeFalse();
        resultado.Codigo.Should().BeNull();
        resultado.Nivel.Should().Be(2);
    }

    [Fact]
    public void El_nivel_siete_llega_hasta_nueve_mil_novecientos_noventa_y_nueve()
    {
        var resultado = ReglasProximoCodigo.Calcular("01.01.01.01.01.01", 6, []);

        resultado.Codigo.Should().Be("01.01.01.01.01.01.0001");
        resultado.Nivel.Should().Be(7);
    }

    [Fact]
    public void No_se_puede_abrir_un_nivel_ocho()
    {
        var accion = () => ReglasProximoCodigo.Calcular("01.01.01.01.01.01.0001", 7, []);

        accion.Should().Throw<InvalidOperationException>()
            .WithMessage("*nivel 7*");
    }

    [Fact]
    public void Sin_padre_calcula_una_raiz()
    {
        var resultado = ReglasProximoCodigo.Calcular(null, 0, ["1", "2", "3", "4", "5"]);

        resultado.Codigo.Should().Be("06");
        resultado.Nivel.Should().Be(1);
    }

    [Fact]
    public void Un_codigo_de_hija_ilegible_no_rompe_el_calculo()
    {
        var resultado = ReglasProximoCodigo.Calcular("01.01", 2, ["01.01.01", null, "01.01.XX"]);

        resultado.Codigo.Should().Be("01.01.02");
    }
}
