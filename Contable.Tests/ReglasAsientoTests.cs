using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Cabecera del asiento y numeracion.
/// </summary>
public class ReglasAsientoTests
{
    private static DatosAsiento Datos(DateTime? fecha = null, int? unidad = 1619, int? tipo = 1, string? leyenda = "Asiento de prueba")
        => new(fecha ?? new DateTime(2026, 5, 12), unidad, tipo, leyenda);

    private static ContextoAsiento Contexto(
        int? vigencia = 201,
        int? ejercicio = 2026,
        int? jurisdiccion = 2437,
        bool tipoExiste = true,
        bool cerrado = false)
        => new(vigencia, ejercicio, jurisdiccion, tipoExiste, cerrado);

    [Fact]
    public void Una_cabecera_bien_formada_pasa()
    {
        ReglasAsiento.ValidarAlta(Datos(), Contexto()).EsValida.Should().BeTrue();
    }

    [Fact]
    public void El_año_contable_sale_de_la_fecha_y_no_de_un_parametro()
    {
        // PKG_T_ASIENTOS.INSERTAR recibe p_ANIO y lo descarta: lo recalcula de la fecha contable.
        ReglasAsiento.AnioDe(new DateTime(2025, 12, 31)).Should().Be(2025);
        ReglasAsiento.AnioDe(new DateTime(2026, 1, 1)).Should().Be(2026);
        ReglasAsiento.AnioDe(null).Should().BeNull();
    }

    [Fact]
    public void Sin_ejercicio_para_el_año_de_la_fecha_no_se_puede_registrar()
    {
        var validacion = ReglasAsiento.ValidarAlta(
            Datos(fecha: new DateTime(2019, 3, 1)),
            Contexto(vigencia: null, ejercicio: null));

        validacion.Errores.Should().Contain(e => e.Contains("2019"));
    }

    [Fact]
    public void No_se_admite_una_fecha_contable_futura()
    {
        var validacion = ReglasAsiento.ValidarAlta(Datos(fecha: DateTime.Today.AddDays(1)), Contexto());

        validacion.Errores.Should().Contain(e => e.Contains("posterior al dia de hoy"));
    }

    [Fact]
    public void Un_ejercicio_ya_cerrado_no_admite_asientos_nuevos()
    {
        var validacion = ReglasAsiento.ValidarAlta(Datos(), Contexto(cerrado: true));

        validacion.Errores.Should().Contain(e => e.Contains("ya tiene asiento de cierre"));
    }

    [Fact]
    public void La_leyenda_es_obligatoria()
    {
        ReglasAsiento.ValidarAlta(Datos(leyenda: "  "), Contexto())
            .Errores.Should().Contain(e => e.Contains("leyenda"));
    }

    [Fact]
    public void Hacen_falta_unidad_administrativa_y_tipo_de_asiento()
    {
        var validacion = ReglasAsiento.ValidarAlta(Datos(unidad: null, tipo: null), Contexto());

        validacion.Errores.Should().Contain(e => e.Contains("unidad administrativa"));
        validacion.Errores.Should().Contain(e => e.Contains("tipo de asiento"));
    }

    [Fact]
    public void El_tipo_de_asiento_tiene_que_existir_en_el_catalogo()
    {
        ReglasAsiento.ValidarAlta(Datos(tipo: 99), Contexto(tipoExiste: false))
            .Errores.Should().Contain(e => e.Contains("no existe"));
    }

    [Fact]
    public void Sin_jurisdiccion_resuelta_no_se_puede_registrar()
    {
        // La tabla guarda ID_JURISDICCION, no la unidad administrativa: si la traduccion falla,
        // el asiento no tiene donde imputarse.
        var validacion = ReglasAsiento.ValidarAlta(Datos(), Contexto(jurisdiccion: null));

        validacion.Errores.Should().Contain(e => e.Contains("jurisdiccion"));
    }

    [Fact]
    public void El_primer_asiento_de_una_jurisdiccion_lleva_el_numero_uno()
    {
        ReglasNumeracionAsiento.ProximoNumero(null).Should().Be(1);
        ReglasNumeracionAsiento.ProximoNumero(0).Should().Be(1);
    }

    [Fact]
    public void La_numeracion_sigue_el_maximo_de_la_jurisdiccion()
    {
        // En el stage la jurisdiccion 2437 tiene los numeros 1, 2 y 3.
        ReglasNumeracionAsiento.ProximoNumero(3).Should().Be(4);
    }

    [Fact]
    public void La_numeracion_no_se_reinicia_al_cambiar_de_ejercicio()
    {
        // Es un correlativo perpetuo por jurisdiccion: F_NUMERO_ASIENTO no filtra por vigencia.
        // El maximo de la jurisdiccion es lo unico que importa, venga del ejercicio que venga.
        ReglasNumeracionAsiento.ProximoNumero(1200).Should().Be(1201);
    }
}
