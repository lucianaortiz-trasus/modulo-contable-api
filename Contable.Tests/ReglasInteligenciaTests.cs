using Contable.Domain.Reglas;
using FluentAssertions;

namespace Contable.Tests;

/// <summary>
/// Detección de importes fuera de lo habitual para una cuenta.
/// </summary>
public class ReglasImportesAtipicosTests
{
    /// <summary>Historial parejo: importes alrededor de 1000.</summary>
    private static decimal[] Habitual() => [980m, 1000m, 1020m, 995m, 1010m, 1005m, 990m, 1015m, 1000m, 1002m];

    [Fact]
    public void Un_importe_parecido_a_los_anteriores_no_llama_la_atencion()
    {
        ReglasImportesAtipicos.Evaluar(1030m, Habitual()).EsAtipico.Should().BeFalse();
    }

    [Fact]
    public void Un_importe_muy_lejano_se_marca_como_atipico()
    {
        var resultado = ReglasImportesAtipicos.Evaluar(250000m, Habitual());

        resultado.Grado.Should().Be(GradoDeAtipicidad.MuyAtipico);
        resultado.Motivo.Should().Contain("mayor");
    }

    [Fact]
    public void Tambien_se_marca_un_importe_llamativamente_chico()
    {
        var resultado = ReglasImportesAtipicos.Evaluar(2m, Habitual());

        resultado.EsAtipico.Should().BeTrue();
        resultado.Motivo.Should().Contain("menor");
    }

    [Fact]
    public void Con_pocos_antecedentes_no_se_opina()
    {
        // Marcar como raro el tercer movimiento de una cuenta seria ruido, no informacion.
        var resultado = ReglasImportesAtipicos.Evaluar(999999m, [100m, 200m, 300m]);

        resultado.EsAtipico.Should().BeFalse();
        resultado.Motivo.Should().Contain("hacen falta");
    }

    [Fact]
    public void Un_solo_importe_enorme_en_el_historial_no_tapa_los_siguientes()
    {
        // Es la razon de usar mediana y no promedio: con promedio, una compra extraordinaria de
        // dos millones sube la referencia lo suficiente como para que despues nada parezca raro.
        var conExtraordinario = new[] { 980m, 1000m, 1020m, 995m, 1010m, 1005m, 990m, 1015m, 2_000_000m };

        var resultado = ReglasImportesAtipicos.Evaluar(180000m, conExtraordinario);

        resultado.EsAtipico.Should().BeTrue("la mediana sigue rondando los 1000");
        resultado.Mediana.Should().BeLessThan(1100m);
    }

    [Fact]
    public void Si_todos_los_anteriores_son_identicos_cualquier_diferencia_salta()
    {
        var resultado = ReglasImportesAtipicos.Evaluar(1500m, [1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m]);

        resultado.Grado.Should().Be(GradoDeAtipicidad.Inusual);
        resultado.Motivo.Should().Contain("todos");
    }

    [Fact]
    public void Repetir_el_importe_habitual_no_es_atipico()
    {
        ReglasImportesAtipicos.Evaluar(1000m, [1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m])
            .EsAtipico.Should().BeFalse();
    }

    [Fact]
    public void Los_ceros_del_historial_no_cuentan()
    {
        // Un renglon al haber tiene el debe en cero: no es un antecedente de importe.
        var resultado = ReglasImportesAtipicos.Evaluar(1000m, [0m, 0m, 0m, 1000m]);

        resultado.Motivo.Should().Contain("1 antecedente");
    }
}

/// <summary>
/// Sugerencia de cuenta contable al cargar un asiento.
/// </summary>
public class ReglasSugerenciaDeImputacionTests
{
    private static readonly AntecedenteDeImputacion[] Historial =
    [
        new(10296, "Pago de energia electrica de dependencias", 12),
        new(10296, "PAGO ENERGIA ELECTRICA OFICINAS", 5),
        new(10298, "Pago de servicio de internet y telefonia", 8),
        new(10094, "Compra de insumos de libreria", 3),
    ];

    [Fact]
    public void La_matriz_contable_manda_sobre_cualquier_estimacion()
    {
        // No es una recomendacion estadistica: es la relacion que alguien cargo a proposito.
        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            cuentaDeLaMatriz: 10259, Historial, "pago de energia electrica", frecuentes: []);

        sugerencias[0].IdCtaContable.Should().Be(10259);
        sugerencias[0].Origen.Should().Be(OrigenDeSugerencia.MatrizContable);
        sugerencias[0].Puntaje.Should().Be(1m);
    }

    [Fact]
    public void Sin_matriz_propone_por_leyenda_parecida()
    {
        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            null, Historial, "Pago de energia electrica de dependencias", frecuentes: []);

        sugerencias.Should().NotBeEmpty();
        sugerencias[0].IdCtaContable.Should().Be(10296);
        sugerencias[0].Origen.Should().Be(OrigenDeSugerencia.LeyendaParecida);
    }

    [Fact]
    public void Cada_sugerencia_dice_de_donde_salio()
    {
        // En contabilidad una recomendacion que no se puede justificar no sirve.
        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            10259, Historial, "pago de energia electrica", frecuentes: [10001]);

        sugerencias.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.Motivo));
    }

    [Fact]
    public void Los_acentos_y_las_mayusculas_no_cambian_el_parecido()
    {
        var conAcento = ReglasSugerenciaDeImputacion.Palabras("Pago de ENERGÍA eléctrica");
        var sinAcento = ReglasSugerenciaDeImputacion.Palabras("pago de energia electrica");

        conAcento.Should().BeEquivalentTo(sinAcento);
    }

    [Fact]
    public void Las_palabras_de_relleno_no_aportan_al_parecido()
    {
        var palabras = ReglasSugerenciaDeImputacion.Palabras("Pago de la energia de las oficinas");

        palabras.Should().NotContain("de");
        palabras.Should().NotContain("las");
        palabras.Should().Contain("energia");
    }

    [Fact]
    public void Los_numeros_de_expediente_no_aportan_al_parecido()
    {
        // Cambian en cada asiento y no dicen nada sobre a que cuenta va.
        ReglasSugerenciaDeImputacion.Palabras("LIBRAMIENTO ORDEN DE PAGO 00003-00000267")
            .Should().NotContain("00000267");
    }

    [Fact]
    public void Dos_leyendas_sin_nada_en_comun_no_se_parecen()
    {
        var unas = ReglasSugerenciaDeImputacion.Palabras("pago de energia electrica");
        var otras = ReglasSugerenciaDeImputacion.Palabras("compra de repuestos para vehiculos");

        ReglasSugerenciaDeImputacion.Parecido(unas, otras).Should().Be(0m);
    }

    [Fact]
    public void Cuando_no_hay_nada_mejor_propone_las_cuentas_mas_usadas()
    {
        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            null, [], "una leyenda sin antecedentes", frecuentes: [10005, 10006]);

        sugerencias.Should().HaveCount(2);
        sugerencias.Should().OnlyContain(s => s.Origen == OrigenDeSugerencia.UsoFrecuente);
    }

    [Fact]
    public void Una_cuenta_no_se_propone_dos_veces()
    {
        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            10296, Historial, "pago de energia electrica", frecuentes: [10296, 10005]);

        sugerencias.Select(s => s.IdCtaContable).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void No_devuelve_mas_sugerencias_que_las_pedidas()
    {
        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            10259, Historial, "pago de energia", frecuentes: [1, 2, 3, 4, 5, 6, 7, 8], maximo: 3);

        sugerencias.Should().HaveCount(3);
    }
}

/// <summary>
/// Detección de asientos posiblemente duplicados.
/// </summary>
public class ReglasDuplicadosTests
{
    private static ReglasDuplicados.AsientoParaComparar Asiento(
        int id, int numero, string fecha, decimal total, string leyenda)
        => new(id, numero, DateTime.Parse(fecha), total, leyenda);

    [Fact]
    public void Dos_asientos_con_misma_fecha_importe_y_leyenda_se_agrupan()
    {
        var grupos = ReglasDuplicados.Detectar(
        [
            Asiento(1, 10, "2026-03-15", 5000m, "Pago a proveedor Gonzalez"),
            Asiento(2, 11, "2026-03-15", 5000m, "Pago a proveedor Gonzalez"),
        ]);

        grupos.Should().ContainSingle();
        grupos[0].Asientos.Should().HaveCount(2);
    }

    [Fact]
    public void La_leyenda_se_compara_normalizada_y_no_caracter_por_caracter()
    {
        // Dos cargas del mismo asiento rara vez tienen la leyenda identica, pero si las mismas
        // palabras.
        var grupos = ReglasDuplicados.Detectar(
        [
            Asiento(1, 10, "2026-03-15", 5000m, "Pago a proveedor GONZALEZ"),
            Asiento(2, 11, "2026-03-15", 5000m, "pago  proveedor gonzalez"),
        ]);

        grupos.Should().ContainSingle();
    }

    [Fact]
    public void Distinto_importe_no_es_duplicado()
    {
        ReglasDuplicados.Detectar(
        [
            Asiento(1, 10, "2026-03-15", 5000m, "Pago a proveedor"),
            Asiento(2, 11, "2026-03-15", 5001m, "Pago a proveedor"),
        ]).Should().BeEmpty();
    }

    [Fact]
    public void Distinta_fecha_no_es_duplicado()
    {
        ReglasDuplicados.Detectar(
        [
            Asiento(1, 10, "2026-03-15", 5000m, "Pago a proveedor"),
            Asiento(2, 11, "2026-03-16", 5000m, "Pago a proveedor"),
        ]).Should().BeEmpty();
    }

    [Fact]
    public void Los_asientos_en_cero_no_se_comparan()
    {
        ReglasDuplicados.Detectar(
        [
            Asiento(1, 10, "2026-03-15", 0m, "Sin importe"),
            Asiento(2, 11, "2026-03-15", 0m, "Sin importe"),
        ]).Should().BeEmpty();
    }

    [Fact]
    public void Los_grupos_mas_numerosos_salen_primero()
    {
        var grupos = ReglasDuplicados.Detectar(
        [
            Asiento(1, 10, "2026-03-15", 100m, "Uno"),
            Asiento(2, 11, "2026-03-15", 100m, "Uno"),
            Asiento(3, 12, "2026-04-20", 200m, "Dos"),
            Asiento(4, 13, "2026-04-20", 200m, "Dos"),
            Asiento(5, 14, "2026-04-20", 200m, "Dos"),
        ]);

        grupos[0].Asientos.Should().HaveCount(3);
    }
}

/// <summary>
/// Catálogo de controles de auditoría.
/// </summary>
public class ReglasAuditoriaTests
{
    [Fact]
    public void Un_asiento_descuadrado_es_un_error_y_no_una_sugerencia()
    {
        var grupo = ReglasAuditoria.Grupo(ReglasAuditoria.AsientoDescuadrado, []);

        grupo.Severidad.Should().Be(Severidad.Error);
        grupo.Titulo.Should().NotBeEmpty();
        grupo.Explicacion.Should().NotBeEmpty();
    }

    [Fact]
    public void Una_partida_sin_cuenta_es_apenas_una_sugerencia()
    {
        // Mezclar esto con los errores hace que nadie mire ninguno.
        ReglasAuditoria.Grupo(ReglasAuditoria.PartidaSinCuenta, []).Severidad.Should().Be(Severidad.Sugerencia);
    }

    [Fact]
    public void Todo_control_explica_que_significa_y_que_hacer()
    {
        var tipos = new[]
        {
            ReglasAuditoria.AsientoDescuadrado, ReglasAuditoria.AsientoSinRenglones,
            ReglasAuditoria.RenglonHuerfano, ReglasAuditoria.CuentaNoImputable,
            ReglasAuditoria.CuentaInactiva, ReglasAuditoria.FechaFueraDeVigencia,
            ReglasAuditoria.PosibleDuplicado, ReglasAuditoria.CuentaSinMatriz,
            ReglasAuditoria.PartidaSinCuenta, ReglasAuditoria.EstructuraDelPlan,
        };

        foreach (var tipo in tipos)
        {
            var grupo = ReglasAuditoria.Grupo(tipo, []);

            grupo.Explicacion.Should().NotBeNullOrWhiteSpace($"el control {tipo} tiene que explicarse");
            grupo.Titulo.Should().NotBe(tipo, $"el control {tipo} necesita un titulo legible");
        }
    }
}
