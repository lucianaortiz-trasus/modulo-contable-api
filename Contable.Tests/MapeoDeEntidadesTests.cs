using Contable.Domain.ModelsEjecucion;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Contable.Tests;

/// <summary>
/// Verifica que el modelo EF Core apunte a las tablas y columnas que la base tiene de verdad.
/// </summary>
/// <remarks>
/// El modelo se construye con el proveedor Oracle pero sin abrir conexion: alcanza para leer los
/// metadatos. Un nombre de columna mal escrito se cae aca, en un test de medio segundo, en vez de
/// aparecer como un ORA-00904 recien en tiempo de ejecucion.
/// </remarks>
public class MapeoDeEntidadesTests
{
    private static EjecucionContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<EjecucionContext>()
            .UseOracle("user id=x;password=y;data source=z")
            .Options;

        return new EjecucionContext(options);
    }

    private static IEntityType Entidad<T>(EjecucionContext contexto)
        => contexto.Model.FindEntityType(typeof(T))!;

    [Theory]
    [InlineData(typeof(TAsiento), "T_ASIENTOS")]
    [InlineData(typeof(TAsientoDetalle), "T_ASIENTOS_DET")]
    [InlineData(typeof(TCuentaContable), "T_CUENTAS_CONTABLES")]
    [InlineData(typeof(TMatrizContable), "T_MATRIZ_CONTABLE")]
    [InlineData(typeof(TTipoAsiento), "T_TIPO_ASIENTO")]
    public void Cada_entidad_apunta_a_su_tabla_del_esquema_ejecucion(Type tipo, string tabla)
    {
        using var contexto = CrearContexto();

        var entidad = contexto.Model.FindEntityType(tipo)!;

        entidad.GetTableName().Should().Be(tabla);
        entidad.GetSchema().Should().Be("EJECUCION");
    }

    [Fact]
    public void El_asiento_mapea_la_jurisdiccion_y_no_la_unidad_administrativa()
    {
        // La tabla guarda ID_JURISDICCION. El PL/SQL recibia la unidad administrativa y traducia
        // internamente; si alguien agrega una propiedad IdUnidadAdm al asiento, esto lo frena.
        using var contexto = CrearContexto();
        var asiento = Entidad<TAsiento>(contexto);

        asiento.GetProperty(nameof(TAsiento.IdJurisdiccion))
            .GetColumnName().Should().Be("ID_JURISDICCION");

        asiento.FindProperty("IdUnidadAdm").Should().BeNull(
            "la tabla T_ASIENTOS no tiene columna de unidad administrativa");
    }

    [Fact]
    public void Debe_y_haber_conservan_los_dos_decimales_de_la_base()
    {
        // El ABM viejo capturaba cinco decimales contra un NUMBER(18,2) y la base truncaba sin
        // avisar. Fijamos la precision para que el redondeo sea una decision, no un accidente.
        using var contexto = CrearContexto();
        var detalle = Entidad<TAsientoDetalle>(contexto);

        foreach (var nombre in new[] { nameof(TAsientoDetalle.Debe), nameof(TAsientoDetalle.Haber) })
        {
            var propiedad = detalle.GetProperty(nombre);
            propiedad.GetPrecision().Should().Be(18);
            propiedad.GetScale().Should().Be(2);
        }
    }

    [Fact]
    public void La_jerarquia_del_plan_de_cuentas_se_resuelve_por_la_cuenta_padre()
    {
        // El nivel 1 usa codigos de un caracter ("1") y el nivel 2 usa "01.01": el codigo del
        // padre no es prefijo del hijo, asi que el arbol solo se puede armar por la relacion.
        using var contexto = CrearContexto();
        var cuenta = Entidad<TCuentaContable>(contexto);

        var relacion = cuenta.GetForeignKeys()
            .SingleOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(TCuentaContable));

        relacion.Should().NotBeNull("el plan de cuentas es un arbol autorreferenciado");
        relacion!.Properties.Single().GetColumnName().Should().Be("ID_CTA_CONTABLE_REL");
    }

    [Fact]
    public void Los_identificadores_no_se_generan_solos_porque_Oracle_usa_secuencias()
    {
        using var contexto = CrearContexto();

        Entidad<TAsiento>(contexto).GetProperty(nameof(TAsiento.IdAsiento))
            .ValueGenerated.Should().Be(ValueGenerated.Never);

        Entidad<TAsientoDetalle>(contexto).GetProperty(nameof(TAsientoDetalle.IdAsientoDet))
            .ValueGenerated.Should().Be(ValueGenerated.Never);
    }
}
