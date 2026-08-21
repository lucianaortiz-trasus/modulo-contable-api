using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Traslado de la matriz contable de un ejercicio al siguiente.
/// </summary>
public class ReglasImportacionMatrizTests
{
    // Ejercicio anterior: objeto 500 tiene numero 2070400, recurso 700 tiene numero 1110100.
    private static readonly Dictionary<int, long> NumeroDeObjeto = new() { [500] = 2070400, [501] = 2070500 };

    private static readonly Dictionary<int, long> NumeroDeRecurso = new() { [700] = 1110100, [701] = 1110200 };

    // Ejercicio destino: los mismos numeros con ids nuevos.
    private static readonly Dictionary<long, int> ObjetosDestino = new() { [2070400] = 900, [2070500] = 901 };

    private static readonly Dictionary<long, int> RecursosDestino = new() { [1110100] = 950, [1110200] = 951 };

    private static PlanDeImportacion Planificar(
        IEnumerable<FilaMatrizOrigen> origen,
        IReadOnlySet<(int, int, TipoDePartida)>? yaEnDestino = null)
        => ReglasImportacionMatriz.Planificar(
            origen,
            ObjetosDestino,
            RecursosDestino,
            NumeroDeObjeto,
            NumeroDeRecurso,
            yaEnDestino ?? new HashSet<(int, int, TipoDePartida)>());

    [Fact]
    public void Traduce_el_objeto_de_gasto_al_id_del_ejercicio_destino()
    {
        var plan = Planificar([new FilaMatrizOrigen(1, 10259, 500, null)]);

        plan.AImportar.Should().ContainSingle();
        plan.AImportar[0].IdCtaContable.Should().Be(10259);
        plan.AImportar[0].IdObjetoGasto.Should().Be(900);
        plan.AImportar[0].IdRecurso.Should().BeNull();
    }

    [Fact]
    public void Importa_tambien_las_filas_de_recurso()
    {
        // Defecto D10: el PL/SQL filtraba "and r.id_recurso is null" y dejaba afuera todo el lado
        // recurso de la matriz, ejercicio tras ejercicio.
        var plan = Planificar([
            new FilaMatrizOrigen(1, 10259, 500, null),
            new FilaMatrizOrigen(2, 10300, null, 700),
        ]);

        plan.AImportar.Should().HaveCount(2);
        plan.CantidadDeGastos.Should().Be(1);
        plan.CantidadDeRecursos.Should().Be(1);
        plan.AImportar.Single(f => f.Tipo == TipoDePartida.Recurso).IdRecurso.Should().Be(950);
    }

    [Fact]
    public void La_partida_sin_equivalente_se_informa_y_no_se_inserta_en_null()
    {
        // Defecto D12: antes la subconsulta devolvia null y la fila entraba con la partida vacia.
        var numerosHuerfanos = new Dictionary<int, long> { [999] = 8888888 };

        var plan = ReglasImportacionMatriz.Planificar(
            [new FilaMatrizOrigen(7, 10259, 999, null)],
            ObjetosDestino,
            RecursosDestino,
            numerosHuerfanos,
            NumeroDeRecurso,
            new HashSet<(int, int, TipoDePartida)>());

        plan.AImportar.Should().BeEmpty();
        plan.NoImportables.Should().ContainSingle();
        plan.NoImportables[0].NumeroPartida.Should().Be(8888888);
        plan.NoImportables[0].Motivo.Should().Contain("no existe en el ejercicio destino");
    }

    [Fact]
    public void Una_relacion_que_ya_esta_en_el_destino_no_se_duplica()
    {
        var yaEnDestino = new HashSet<(int, int, TipoDePartida)> { (10259, 900, TipoDePartida.Gasto) };

        var plan = Planificar([new FilaMatrizOrigen(1, 10259, 500, null)], yaEnDestino);

        plan.AImportar.Should().BeEmpty();
        plan.YaExistentes.Should().Be(1);
    }

    [Fact]
    public void Dos_filas_de_origen_que_colapsan_en_la_misma_del_destino_se_insertan_una_sola_vez()
    {
        var plan = Planificar([
            new FilaMatrizOrigen(1, 10259, 500, null),
            new FilaMatrizOrigen(2, 10259, 500, null),
        ]);

        plan.AImportar.Should().ContainSingle();
        plan.YaExistentes.Should().Be(1);
    }

    [Fact]
    public void La_fila_sin_partida_se_informa()
    {
        var plan = Planificar([new FilaMatrizOrigen(3, 10259, null, null)]);

        plan.AImportar.Should().BeEmpty();
        plan.NoImportables.Should().ContainSingle();
        plan.NoImportables[0].Motivo.Should().Contain("ninguna partida");
    }

    [Fact]
    public void La_fila_con_las_dos_partidas_se_informa()
    {
        var plan = Planificar([new FilaMatrizOrigen(4, 10259, 500, 700)]);

        plan.AImportar.Should().BeEmpty();
        plan.NoImportables[0].Motivo.Should().Contain("a la vez");
    }

    [Fact]
    public void La_fila_sin_cuenta_contable_se_informa()
    {
        var plan = Planificar([new FilaMatrizOrigen(5, null, 500, null)]);

        plan.AImportar.Should().BeEmpty();
        plan.NoImportables[0].Motivo.Should().Contain("cuenta contable");
    }

    [Fact]
    public void El_resultado_cuenta_lo_que_realmente_va_a_pasar_con_cada_fila()
    {
        // El PL/SQL informaba "Se insertaron N registros" con el total de la vigencia destino, no
        // con lo que habia insertado.
        var yaEnDestino = new HashSet<(int, int, TipoDePartida)> { (10300, 951, TipoDePartida.Recurso) };

        var plan = Planificar(
            [
                new FilaMatrizOrigen(1, 10259, 500, null),
                new FilaMatrizOrigen(2, 10259, 501, null),
                new FilaMatrizOrigen(3, 10300, null, 701),
                new FilaMatrizOrigen(4, 10300, null, null),
            ],
            yaEnDestino);

        plan.TotalOrigen.Should().Be(4);
        plan.AImportar.Should().HaveCount(2);
        plan.YaExistentes.Should().Be(1);
        plan.NoImportables.Should().ContainSingle();
    }

    [Fact]
    public void Un_origen_vacio_da_un_plan_vacio()
    {
        var plan = Planificar([]);

        plan.TotalOrigen.Should().Be(0);
        plan.AImportar.Should().BeEmpty();
        plan.NoImportables.Should().BeEmpty();
    }
}
