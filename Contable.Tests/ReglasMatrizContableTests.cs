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

    [Fact]
    public void MatrizContableResponse_ExponeEstadoYActiva()
    {
        var activa = new Contable.Application.ResponseDto.Dtos.MatrizContable.MatrizContableResponse { Activa = true };
        activa.Estado.Should().Be("Activa");

        var inactiva = new Contable.Application.ResponseDto.Dtos.MatrizContable.MatrizContableResponse { Activa = false };
        inactiva.Estado.Should().Be("Inactiva");
    }

    [Fact]
    public void PendientesMatrizResponse_CalculaHayPendientesCorrectamente()
    {
        var sinPendientes = new Contable.Application.ResponseDto.Dtos.MatrizContable.PendientesMatrizResponse
        {
            GastosSinRelacionar = 0,
            RecursosSinRelacionar = 0
        };
        sinPendientes.HayPendientes.Should().BeFalse();
        sinPendientes.TotalPendientes.Should().Be(0);

        var conPendientes = new Contable.Application.ResponseDto.Dtos.MatrizContable.PendientesMatrizResponse
        {
            GastosSinRelacionar = 5,
            RecursosSinRelacionar = 2
        };
        conPendientes.HayPendientes.Should().BeTrue();
        conPendientes.TotalPendientes.Should().Be(7);
    }

    [Fact]
    public void PartidaArbolResponse_ConstruyeJerarquia()
    {
        var raiz = new Contable.Application.ResponseDto.Dtos.MatrizContable.PartidaArbolResponse
        {
            Id = 1,
            Numero = 2000000,
            Nombre = "Bienes de Consumo",
            EsHoja = false
        };
        var hijo = new Contable.Application.ResponseDto.Dtos.MatrizContable.PartidaArbolResponse
        {
            Id = 2,
            IdPadre = 1,
            Numero = 2070400,
            Nombre = "Repuestos y accesorios",
            EsHoja = true
        };
        raiz.Hijas.Add(hijo);

        raiz.Hijas.Should().ContainSingle();
        raiz.Hijas[0].Codigo.Should().Be("2070400");
        raiz.Hijas[0].EsHoja.Should().BeTrue();
    }

    [Fact]
    public void OrdenamientoMatriz_OrdenaPorCuentaTipoYPartida()
    {
        var lista = new List<Contable.Application.ResponseDto.Dtos.MatrizContable.MatrizContableResponse>
        {
            new() { CodigoCuenta = "05.01", NombreCuenta = "Repuestos", TipoPartida = "Gasto", NumeroObjeto = 2070400 },
            new() { CodigoCuenta = "01.01", NombreCuenta = "Caja", TipoPartida = "Gasto", NumeroObjeto = 3140100 },
            new() { CodigoCuenta = "01.01", NombreCuenta = "Caja", TipoPartida = "Gasto", NumeroObjeto = 1010000 },
            new() { CodigoCuenta = "04.01", NombreCuenta = "Ventas", TipoPartida = "Recurso", NumeroRecurso = 101021000 },
        };

        var ordenadas = lista
            .OrderBy(f => f.CodigoCuenta, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.NombreCuenta, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.TipoPartida, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.NumeroObjeto?.ToString() ?? f.NumeroRecurso?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ordenadas[0].CodigoCuenta.Should().Be("01.01");
        ordenadas[0].NumeroObjeto.Should().Be(1010000);
        ordenadas[1].NumeroObjeto.Should().Be(3140100);
        ordenadas[2].CodigoCuenta.Should().Be("04.01");
        ordenadas[3].CodigoCuenta.Should().Be("05.01");
    }

    [Fact]
    public void PartidaArbolResponse_IndicaSiYaPoseeRelacionActiva()
    {
        var partida = new Contable.Application.ResponseDto.Dtos.MatrizContable.PartidaArbolResponse
        {
            Id = 10,
            Numero = 2070400,
            Nombre = "Repuestos",
            EsHoja = true,
            YaRelacionada = true
        };

        partida.YaRelacionada.Should().BeTrue();
        partida.EsHoja.Should().BeTrue();
    }

    [Fact]
    public void ValidarAlta_RechazaCuentaNoImputableOInactiva()
    {
        var contextoNoImputable = ContextoSano() with { CuentaEsImputable = false };
        var validacionNoImputable = ReglasMatrizContable.ValidarAlta(Gasto(), contextoNoImputable);
        validacionNoImputable.EsValida.Should().BeFalse();
        validacionNoImputable.Errores.Should().Contain(e => e.Contains("no es imputable"));

        var contextoInactiva = ContextoSano() with { CuentaEstaActiva = false };
        var validacionInactiva = ReglasMatrizContable.ValidarAlta(Gasto(), contextoInactiva);
        validacionInactiva.EsValida.Should().BeFalse();
        validacionInactiva.Errores.Should().Contain(e => e.Contains("dada de baja"));
    }

    [Fact]
    public void ValidarAlta_RechazaPartidaYaRelacionadaEnLaMatriz()
    {
        var contextoDuplicada = ContextoSano(partidaYaAsignadaA: "01.01.01.01");
        var validacion = ReglasMatrizContable.ValidarAlta(Gasto(), contextoDuplicada);

        validacion.EsValida.Should().BeFalse();
        validacion.Errores.Should().Contain(e => e.Contains("ya esta relacionada con la cuenta 01.01.01.01"));
    }
}
