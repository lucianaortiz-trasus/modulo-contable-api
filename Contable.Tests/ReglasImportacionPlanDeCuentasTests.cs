using Contable.Domain.Reglas;
using FluentAssertions;
using Xunit;

namespace Contable.Tests;

public class ReglasImportacionPlanDeCuentasTests
{
    private static PlanDeImportacionDeCuentas Evaluar(
        IEnumerable<FilaImportada> filas,
        IEnumerable<string>? existentes = null,
        IEnumerable<string>? imputables = null)
        => ReglasImportacionPlanDeCuentas.Evaluar(
            filas,
            new HashSet<string>(existentes ?? [], StringComparer.Ordinal),
            new HashSet<string>(imputables ?? [], StringComparer.Ordinal));

    private static FilaImportada Fila(int n, string codigo, string nombre = "Una cuenta",
        string saldo = "D", string imputable = "N")
        => new(n, codigo, nombre, saldo, imputable, null);

    [Fact]
    public void Un_archivo_con_el_arbol_completo_se_crea_entero()
    {
        var plan = Evaluar([
            Fila(1, "01", "Activo"),
            Fila(2, "01.01", "Activo Corriente"),
            Fila(3, "01.01.01", "Caja", imputable: "S"),
        ]);

        plan.ACrear.Should().Be(3);
        plan.Rechazadas.Should().Be(0);
    }

    [Fact]
    public void El_padre_puede_venir_despues_que_la_hija_en_el_archivo()
    {
        // Se evalua en orden de codigo, no en orden de archivo: si no, una hija listada primero
        // se rechazaria por "no existe el padre" aunque el padre venga dos filas mas abajo.
        var plan = Evaluar([
            Fila(1, "01.01.01", "Caja"),
            Fila(2, "01"),
            Fila(3, "01.01"),
        ]);

        plan.ACrear.Should().Be(3);
    }

    [Fact]
    public void Las_filas_vuelven_en_el_orden_del_archivo()
    {
        // El usuario busca el error por numero de fila, asi que el orden de salida es el del
        // archivo aunque por dentro se haya evaluado por codigo.
        var plan = Evaluar([Fila(1, "01.01"), Fila(2, "01")]);

        plan.Filas.Select(f => f.Fila).Should().ContainInOrder(1, 2);
    }

    [Fact]
    public void Lo_que_se_crea_sale_ordenado_con_cada_padre_antes_que_sus_hijas()
    {
        var plan = Evaluar([
            Fila(1, "01.01.01"),
            Fila(2, "01"),
            Fila(3, "01.01"),
        ]);

        plan.ParaCrear.Select(f => f.Codigo).Should().ContainInOrder("01", "01.01", "01.01.01");
    }

    [Fact]
    public void Una_cuenta_que_ya_esta_en_el_plan_se_informa_y_no_se_toca()
    {
        var plan = Evaluar([Fila(1, "01", "Otro nombre")], existentes: ["01"]);

        plan.YaExisten.Should().Be(1);
        plan.ACrear.Should().Be(0);
        plan.Rechazadas.Should().Be(0);
        plan.Filas[0].Motivo.Should().Contain("Ya esta en el plan");
    }

    [Fact]
    public void Una_hija_cuelga_de_un_padre_que_ya_estaba_en_el_plan()
    {
        var plan = Evaluar([Fila(1, "01.01")], existentes: ["01"]);

        plan.ACrear.Should().Be(1);
    }

    [Fact]
    public void Sin_padre_en_la_base_ni_en_el_archivo_se_rechaza()
    {
        var plan = Evaluar([Fila(1, "01.01")]);

        plan.Rechazadas.Should().Be(1);
        plan.Filas[0].Motivo.Should().Contain("cuenta padre 01");
    }

    [Fact]
    public void Un_padre_rechazado_arrastra_a_sus_hijas()
    {
        // El padre se cae por falta de nombre; la hija no puede crearse igual, porque colgaria de
        // una cuenta que no va a existir.
        var plan = Evaluar([
            Fila(1, "01", nombre: ""),
            Fila(2, "01.01"),
        ]);

        plan.ACrear.Should().Be(0);
        plan.Rechazadas.Should().Be(2);
    }

    [Fact]
    public void No_se_puede_colgar_de_una_cuenta_imputable_que_ya_existe()
    {
        var plan = Evaluar([Fila(1, "01.01")], existentes: ["01"], imputables: ["01"]);

        plan.Rechazadas.Should().Be(1);
        plan.Filas[0].Motivo.Should().Contain("imputable");
    }

    [Fact]
    public void No_se_puede_colgar_de_una_cuenta_imputable_que_crea_el_mismo_archivo()
    {
        var plan = Evaluar([
            Fila(1, "01", imputable: "S"),
            Fila(2, "01.01"),
        ]);

        plan.ACrear.Should().Be(1);
        plan.Rechazadas.Should().Be(1);
        plan.Filas[1].Motivo.Should().Contain("imputable");
    }

    [Fact]
    public void Un_codigo_repetido_dentro_del_archivo_senala_la_fila_con_la_que_choca()
    {
        var plan = Evaluar([Fila(1, "01"), Fila(2, "01")]);

        plan.ACrear.Should().Be(1);
        plan.Rechazadas.Should().Be(1);
        plan.Filas[1].Motivo.Should().Contain("fila 1");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Sin_codigo_se_rechaza(string codigo)
    {
        Evaluar([Fila(1, codigo)]).Rechazadas.Should().Be(1);
    }

    [Fact]
    public void Sin_nombre_se_rechaza()
    {
        var plan = Evaluar([Fila(1, "01", nombre: "  ")]);

        plan.Rechazadas.Should().Be(1);
        plan.Filas[0].Motivo.Should().Contain("nombre");
    }

    [Fact]
    public void Un_nombre_demasiado_largo_se_rechaza()
    {
        var largo = new string('x', ReglasImportacionPlanDeCuentas.LargoMaximoDelNombre + 1);

        Evaluar([Fila(1, "01", nombre: largo)]).Rechazadas.Should().Be(1);
    }

    [Fact]
    public void Un_tipo_de_saldo_invalido_se_rechaza()
    {
        var plan = Evaluar([Fila(1, "01", saldo: "X")]);

        plan.Rechazadas.Should().Be(1);
        plan.Filas[0].Motivo.Should().Contain("tipo de saldo");
    }

    [Theory]
    [InlineData("D", "D")]
    [InlineData("d", "D")]
    [InlineData("Deudor", "D")]
    [InlineData("A", "A")]
    [InlineData("Acreedor", "A")]
    public void El_tipo_de_saldo_se_acepta_escrito_completo(string entrada, string esperado)
    {
        ReglasImportacionPlanDeCuentas.NormalizarTipoSaldo(entrada).Should().Be(esperado);
    }

    [Theory]
    [InlineData("S")]
    [InlineData("si")]
    [InlineData("X")]
    [InlineData("1")]
    [InlineData("TRUE")]
    public void La_columna_imputable_acepta_las_formas_habituales(string valor)
    {
        ReglasImportacionPlanDeCuentas.EsAfirmativo(valor).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("N")]
    [InlineData("no")]
    [InlineData(null)]
    public void Imputable_vacio_es_que_no(string? valor)
    {
        // Una cuenta se marca imputable a proposito, nunca por omision: si quedara en true por
        // defecto, un archivo sin esa columna dejaria todo el plan recibiendo movimientos.
        ReglasImportacionPlanDeCuentas.EsAfirmativo(valor).Should().BeFalse();
    }

    [Fact]
    public void El_codigo_se_normaliza_antes_de_comparar()
    {
        // "1.1" y "01.01" son la misma cuenta: si no se normalizara, el archivo crearia una
        // duplicada al lado de la que ya existe.
        var plan = Evaluar([Fila(1, "1.1")], existentes: ["01.01"]);

        plan.YaExisten.Should().Be(1);
    }

    [Fact]
    public void Un_archivo_vacio_no_rompe()
    {
        var plan = Evaluar([]);

        plan.Total.Should().Be(0);
        plan.ACrear.Should().Be(0);
    }

    [Fact]
    public void El_plan_cuenta_las_tres_categorias_por_separado()
    {
        var plan = Evaluar([
            Fila(1, "01"),
            Fila(2, "02"),
            Fila(3, "01.01.01"),
        ], existentes: ["02"]);

        plan.Total.Should().Be(3);
        plan.ACrear.Should().Be(1);
        plan.YaExisten.Should().Be(1);
        plan.Rechazadas.Should().Be(1);
    }

    [Fact]
    public void PadreDe_devuelve_el_codigo_sin_el_ultimo_segmento()
    {
        ReglasImportacionPlanDeCuentas.PadreDe("01.01.01").Should().Be("01.01");
        ReglasImportacionPlanDeCuentas.PadreDe("01").Should().BeEmpty();
    }
}
