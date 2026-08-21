using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Reversa y anulacion: las dos formas de dar vuelta un asiento ya registrado.
/// </summary>
public class ReglasContrapartidaTests
{
    private static AsientoOriginal Original(
        int numero = 2,
        int anio = 2026,
        int ejercicio = 2026,
        bool yaAnulado = false,
        bool yaTieneContrapartida = false)
        => new(
            IdAsiento: 209999,
            Numero: numero,
            FecContable: new DateTime(anio, 5, 12),
            IdTipoAsiento: 1,
            Leyenda: "LIBRAMIENTO ORDEN DE PAGO 156",
            Observaciones: "obs",
            Ejercicio: ejercicio,
            YaAnulado: yaAnulado,
            YaTieneContrapartida: yaTieneContrapartida);

    private static readonly RenglonPlanificado[] Renglones =
    [
        new(10014, 1000m, null, 3),
        new(10094, null, 1000m, 3),
    ];

    [Fact]
    public void La_contrapartida_invierte_el_debe_y_el_haber()
    {
        var plan = ReglasContrapartida.Planificar(
            TipoDeContrapartida.Reversa, Original(), Renglones, new DateTime(2026, 8, 20));

        plan.Renglones.Should().HaveCount(2);
        plan.Renglones[0].Should().BeEquivalentTo(new RenglonPlanificado(10014, null, 1000m, 3));
        plan.Renglones[1].Should().BeEquivalentTo(new RenglonPlanificado(10094, 1000m, null, 3));
    }

    [Fact]
    public void La_contrapartida_queda_cuadrada()
    {
        var plan = ReglasContrapartida.Planificar(
            TipoDeContrapartida.Reversa, Original(), Renglones, new DateTime(2026, 8, 20));

        var comoRenglones = plan.Renglones.Select(r => new RenglonAsiento(r.IdCtaContable, r.Debe, r.Haber));

        ReglasBalanceAsiento.Cuadra(comoRenglones).Should().BeTrue();
    }

    [Fact]
    public void La_reversa_de_un_asiento_del_año_en_curso_va_a_la_fecha_de_hoy()
    {
        var hoy = new DateTime(2026, 8, 20);

        ReglasContrapartida.FechaContable(TipoDeContrapartida.Reversa, Original(anio: 2026), hoy)
            .Should().Be(hoy);
    }

    [Fact]
    public void La_reversa_de_un_asiento_de_un_año_anterior_va_al_cierre_de_su_ejercicio()
    {
        // Asi la reversa de un asiento viejo no ensucia el ejercicio corriente.
        var hoy = new DateTime(2026, 8, 20);

        ReglasContrapartida.FechaContable(TipoDeContrapartida.Reversa, Original(anio: 2025, ejercicio: 2025), hoy)
            .Should().Be(new DateTime(2025, 12, 31));
    }

    [Fact]
    public void La_anulacion_conserva_la_fecha_contable_del_original()
    {
        // Es la unica diferencia real entre anular y reversar.
        var original = Original(anio: 2025, ejercicio: 2025);

        ReglasContrapartida.FechaContable(TipoDeContrapartida.Anulacion, original, new DateTime(2026, 8, 20))
            .Should().Be(original.FecContable);
    }

    [Fact]
    public void La_reversa_lleva_el_numero_del_asiento_de_origen_en_la_leyenda()
    {
        var plan = ReglasContrapartida.Planificar(
            TipoDeContrapartida.Reversa, Original(numero: 7), Renglones, new DateTime(2026, 8, 20));

        plan.Leyenda.Should().Be("REVERSA ASIENTO 7");
    }

    [Fact]
    public void La_anulacion_antepone_CONTRAASIENTO_a_la_leyenda_original()
    {
        var plan = ReglasContrapartida.Planificar(
            TipoDeContrapartida.Anulacion, Original(), Renglones, new DateTime(2026, 8, 20));

        plan.Leyenda.Should().Be("CONTRAASIENTOLIBRAMIENTO ORDEN DE PAGO 156");
        plan.Observaciones.Should().Be("CONTRAASIENTOobs");
    }

    [Fact]
    public void La_contrapartida_queda_vinculada_al_asiento_de_origen()
    {
        var plan = ReglasContrapartida.Planificar(
            TipoDeContrapartida.Reversa, Original(), Renglones, new DateTime(2026, 8, 20));

        plan.IdAsientoRel.Should().Be(209999);
        plan.IdTipoAsiento.Should().Be(1);
    }

    [Fact]
    public void No_se_reversa_un_asiento_ya_anulado()
    {
        ReglasContrapartida.Validar(Original(yaAnulado: true))
            .Errores.Should().Contain(e => e.Contains("ya esta anulado"));
    }

    [Fact]
    public void No_se_reversa_dos_veces_el_mismo_asiento()
    {
        ReglasContrapartida.Validar(Original(yaTieneContrapartida: true))
            .Errores.Should().Contain(e => e.Contains("ya tiene un contraasiento"));
    }

    [Fact]
    public void Un_asiento_sano_se_puede_reversar()
    {
        ReglasContrapartida.Validar(Original()).EsValida.Should().BeTrue();
    }
}
