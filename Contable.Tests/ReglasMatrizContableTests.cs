using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Validaciones de la matriz contable: que cuenta recibe cada partida presupuestaria.
/// </summary>
public class ReglasMatrizContableTests
{
    private static ContextoMatriz ContextoSano(bool relacionYaCargada = false, string? partidaYaAsignadaA = null)
        => new(
            CuentaExiste: true,
            CuentaEsImputable: true,
            CuentaEstaActiva: true,
            PartidaExiste: true,
            PartidaEsDeLaVigencia: true,
            PartidaEsImputable: true,
            RelacionYaCargada: relacionYaCargada,
            PartidaYaAsignadaA: partidaYaAsignadaA);

    private static DatosMatriz Gasto(int cuenta = 10259, int objeto = 102978, int vigencia = 201)
        => new(cuenta, objeto, null, vigencia);

    private static DatosMatriz Recurso(int cuenta = 10259, int recurso = 555, int vigencia = 201)
        => new(cuenta, null, recurso, vigencia);

    [Fact]
    public void Un_alta_de_gasto_bien_formada_pasa()
    {
        ReglasMatrizContable.ValidarAlta(Gasto(), ContextoSano()).EsValida.Should().BeTrue();
    }

    [Fact]
    public void Un_alta_de_recurso_bien_formada_pasa()
    {
        ReglasMatrizContable.ValidarAlta(Recurso(), ContextoSano()).EsValida.Should().BeTrue();
    }

    [Fact]
    public void TipoDe_distingue_gasto_de_recurso()
    {
        ReglasMatrizContable.TipoDe(Gasto()).Should().Be(TipoDePartida.Gasto);
        ReglasMatrizContable.TipoDe(Recurso()).Should().Be(TipoDePartida.Recurso);
    }

    [Fact]
    public void Una_fila_no_puede_tener_las_dos_partidas()
    {
        var datos = new DatosMatriz(10259, 102978, 555, 201);

        ReglasMatrizContable.TipoDe(datos).Should().BeNull();

        var validacion = ReglasMatrizContable.ValidarAlta(datos, ContextoSano());
        validacion.Errores.Should().Contain(e => e.Contains("una sola partida"));
    }

    [Fact]
    public void Una_fila_tiene_que_tener_alguna_partida()
    {
        var validacion = ReglasMatrizContable.ValidarAlta(new DatosMatriz(10259, null, null, 201), ContextoSano());

        validacion.Errores.Should().Contain(e => e.Contains("indicar una partida"));
    }

    [Fact]
    public void La_cuenta_tiene_que_ser_imputable()
    {
        // Una cuenta no imputable no puede recibir el asiento que genera la partida.
        var contexto = ContextoSano() with { CuentaEsImputable = false };

        ReglasMatrizContable.ValidarAlta(Gasto(), contexto)
            .Errores.Should().Contain(e => e.Contains("no es imputable"));
    }

    [Fact]
    public void La_cuenta_dada_de_baja_no_admite_relaciones_nuevas()
    {
        var contexto = ContextoSano() with { CuentaEstaActiva = false };

        ReglasMatrizContable.ValidarAlta(Gasto(), contexto)
            .Errores.Should().Contain(e => e.Contains("dada de baja"));
    }

    [Fact]
    public void La_partida_tiene_que_ser_de_ultimo_nivel()
    {
        var contexto = ContextoSano() with { PartidaEsImputable = false };

        ReglasMatrizContable.ValidarAlta(Gasto(), contexto)
            .Errores.Should().Contain(e => e.Contains("ultimo nivel"));
    }

    [Fact]
    public void La_partida_tiene_que_ser_del_mismo_ejercicio_que_la_relacion()
    {
        var contexto = ContextoSano() with { PartidaEsDeLaVigencia = false };

        ReglasMatrizContable.ValidarAlta(Gasto(), contexto)
            .Errores.Should().Contain(e => e.Contains("otro ejercicio"));
    }

    [Fact]
    public void No_se_repite_la_misma_relacion_en_el_mismo_ejercicio()
    {
        ReglasMatrizContable.ValidarAlta(Gasto(), ContextoSano(relacionYaCargada: true))
            .Errores.Should().Contain(e => e.Contains("ya esta relacionada"));
    }

    [Fact]
    public void Avisa_cuando_la_partida_ya_apunta_a_otra_cuenta()
    {
        // Si la misma partida apunta a dos cuentas, el asiento automatico no sabe cual usar.
        ReglasMatrizContable.ValidarAlta(Gasto(), ContextoSano(partidaYaAsignadaA: "05.01.02.01.02"))
            .Errores.Should().Contain(e => e.Contains("05.01.02.01.02"));
    }

    [Fact]
    public void El_mismo_recurso_se_puede_mapear_en_otro_ejercicio()
    {
        // Defecto D9: el PL/SQL controlaba solo el recurso, sin cuenta ni vigencia, con lo que un
        // recurso mapeado una vez quedaba bloqueado para siempre. Aca el control es por la terna.
        var enEjercicioNuevo = ContextoSano(relacionYaCargada: false);

        ReglasMatrizContable.ValidarAlta(Recurso(vigencia: 202), enEjercicioNuevo)
            .EsValida.Should().BeTrue();
    }

    [Fact]
    public void El_mismo_recurso_se_puede_mapear_a_otra_cuenta_del_mismo_ejercicio_con_aviso()
    {
        // No es un error de datos, pero el usuario tiene que enterarse.
        var validacion = ReglasMatrizContable.ValidarAlta(
            Recurso(cuenta: 10300), ContextoSano(partidaYaAsignadaA: "05.01.02.01.02"));

        validacion.EsValida.Should().BeFalse();
        validacion.Errores.Should().ContainSingle();
    }

    [Fact]
    public void Hacen_falta_cuenta_y_ejercicio()
    {
        var validacion = ReglasMatrizContable.ValidarAlta(new DatosMatriz(null, 102978, null, null), ContextoSano());

        validacion.Errores.Should().Contain(e => e.Contains("cuenta contable"));
        validacion.Errores.Should().Contain(e => e.Contains("ejercicio"));
    }
}
