using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Armado del arbol del plan de cuentas, con los datos sucios que tiene el plan real.
/// </summary>
public class ArbolDeCuentasTests
{
    private sealed record Cuenta(int Id, int? IdPadre, string? Codigo, int? Nivel, bool EsImputable)
        : INodoPlanDeCuentas;

    private static Cuenta C(int id, int? padre, string codigo, int nivel, bool imputable = false)
        => new(id, padre, codigo, nivel, imputable);

    [Fact]
    public void Arma_la_jerarquia_por_la_relacion_padre_hijo()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "1", 1),
            C(2, 1, "01.01", 2),
            C(3, 2, "01.01.01", 3),
        ]);

        arbol.Raices.Should().HaveCount(1);
        arbol.Raices[0].Cuenta.Id.Should().Be(1);
        arbol.Raices[0].Hijas.Should().HaveCount(1);
        arbol.Raices[0].Hijas[0].Hijas[0].Cuenta.Id.Should().Be(3);
    }

    [Fact]
    public void El_salto_de_nivel_uno_a_dos_no_se_marca_como_anomalia()
    {
        // "1" no es prefijo textual de "01.01", pero normalizado si cuelga.
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "1", 1),
            C(2, 1, "01.01", 2),
        ]);

        arbol.PorId[2].Anomalias.Should().Be(AnomaliaJerarquia.Ninguna);
    }

    [Fact]
    public void Una_cuenta_que_se_apunta_a_si_misma_no_desaparece_del_arbol()
    {
        // Caso real: la cuenta 10309 "Honorarios Profesionales" tiene id_cta_contable_rel = 10309.
        // Un armado que arranque solo desde las raices la perderia junto con sus dos hermanas.
        var arbol = ArbolDeCuentas.Armar([
            C(10308, null, "05.01.03.04", 4),
            C(10309, 10309, "05.01.03.04.01", 5, imputable: true),
            C(10310, 10309, "05.01.03.04.02", 5, imputable: true),
            C(10311, 10309, "05.01.03.04.03", 5, imputable: true),
        ]);

        arbol.PorId.Should().HaveCount(4);
        arbol.PorId[10309].Anomalias.Should().HaveFlag(AnomaliaJerarquia.SeApuntaASiMisma);

        // Se promueve a raiz para que siga siendo visible, y sus hijas cuelgan de ella.
        arbol.Raices.Select(r => r.Cuenta.Id).Should().Contain(10309);
        arbol.PorId[10309].Hijas.Should().HaveCount(2);
    }

    [Fact]
    public void Un_ciclo_entre_dos_cuentas_no_cuelga_el_armado()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, 2, "01.01", 2),
            C(2, 1, "01.02", 2),
        ]);

        arbol.PorId.Should().HaveCount(2);
        arbol.ConAnomalias.Should().NotBeEmpty();
        arbol.Raices.Should().NotBeEmpty();
    }

    [Fact]
    public void Una_cuenta_con_padre_inexistente_queda_como_raiz_marcada()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "1", 1),
            C(9, 999, "01.09", 2),
        ]);

        arbol.PorId[9].Anomalias.Should().HaveFlag(AnomaliaJerarquia.PadreInexistente);
        arbol.Raices.Select(r => r.Cuenta.Id).Should().Contain(9);
    }

    [Fact]
    public void Marca_el_nivel_desalineado_del_padre()
    {
        // Caso real: "01.01.02.01" (nivel 4) cuelga de "01.01.05.01", que tambien es nivel 4.
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "01.01.05.01", 4),
            C(2, 1, "01.01.02.01", 4),
        ]);

        arbol.PorId[2].Anomalias.Should().HaveFlag(AnomaliaJerarquia.NivelInconsistente);
    }

    [Fact]
    public void Marca_el_codigo_que_no_cuelga_del_padre()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "01.02.02.02", 4),
            C(2, 1, "01.01.03.01", 4),
        ]);

        arbol.PorId[2].Anomalias.Should().HaveFlag(AnomaliaJerarquia.CodigoNoCuelgaDelPadre);
    }

    [Fact]
    public void Marca_la_imputable_que_tiene_hijas()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "01.01", 2, imputable: true),
            C(2, 1, "01.01.01", 3),
        ]);

        arbol.PorId[1].Anomalias.Should().HaveFlag(AnomaliaJerarquia.ImputableConHijas);
        arbol.PorId[2].Anomalias.Should().Be(AnomaliaJerarquia.Ninguna);
    }

    [Fact]
    public void Las_hijas_salen_ordenadas_por_codigo_normalizado()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "1", 1),
            C(4, 1, "01.10", 2),
            C(2, 1, "01.02", 2),
            C(3, 1, "01.03", 2),
        ]);

        arbol.Raices[0].Hijas.Select(h => h.Cuenta.Codigo)
            .Should().ContainInOrder("01.02", "01.03", "01.10");
    }

    [Fact]
    public void Calcula_la_profundidad_real_de_cada_nodo()
    {
        var arbol = ArbolDeCuentas.Armar([
            C(1, null, "1", 1),
            C(2, 1, "01.01", 2),
            C(3, 2, "01.01.01", 3),
        ]);

        arbol.PorId[1].Profundidad.Should().Be(1);
        arbol.PorId[2].Profundidad.Should().Be(2);
        arbol.PorId[3].Profundidad.Should().Be(3);
    }

    [Fact]
    public void Un_plan_vacio_da_un_arbol_vacio()
    {
        var arbol = ArbolDeCuentas.Armar(Array.Empty<Cuenta>());

        arbol.Raices.Should().BeEmpty();
        arbol.PorId.Should().BeEmpty();
    }

    [Fact]
    public void Ninguna_cuenta_se_pierde_por_mas_sucios_que_esten_los_datos()
    {
        // Mezcla de todos los casos raros que aparecen en el plan real.
        var cuentas = new[]
        {
            C(1, null, "1", 1),
            C(2, 1, "01.01", 2),
            C(3, 3, "01.02", 2),          // se apunta a si misma
            C(4, 777, "01.03", 2),        // padre inexistente
            C(5, 6, "01.04", 2),          // ciclo con 6
            C(6, 5, "01.05", 2),
            C(7, 2, "09.09.09", 3),       // codigo que no cuelga
        };

        var arbol = ArbolDeCuentas.Armar(cuentas);

        arbol.PorId.Should().HaveCount(cuentas.Length);

        var enElArbol = new HashSet<int>();
        var pila = new Stack<NodoCuenta<Cuenta>>(arbol.Raices);

        while (pila.Count > 0)
        {
            var nodo = pila.Pop();
            enElArbol.Add(nodo.Cuenta.Id).Should().BeTrue("ningun nodo se visita dos veces");
            foreach (var hija in nodo.Hijas) pila.Push(hija);
        }

        enElArbol.Should().BeEquivalentTo(cuentas.Select(c => c.Id));
    }

    [Fact]
    public void CuentaContableResponse_ResuelveCuentaSuperiorYNaturaleza()
    {
        var resp = new Contable.Application.ResponseDto.Dtos.PlanDeCuentas.CuentaContableResponse
        {
            Codigo = "02.01.01.02",
            Nombre = "Acreedores Varios",
            CodigoPadre = "02.01.01",
            NombrePadre = "Deudas Comerciales",
            IdNaturaleza = 2,
        };

        resp.CuentaSuperior.Should().Be("02.01.01 Deudas Comerciales");
        resp.Naturaleza.Should().Be("PASIVO");

        var raiz = new Contable.Application.ResponseDto.Dtos.PlanDeCuentas.CuentaContableResponse
        {
            Codigo = "1",
            Nombre = "ACTIVO",
            IdNaturaleza = null,
        };

        raiz.CuentaSuperior.Should().Be("No posee");
        raiz.Naturaleza.Should().Be("ACTIVO");
    }
}
