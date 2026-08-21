namespace Contable.Application.RequestDto.Reportes;

/// <summary>Parametros comunes de los tres reportes contables.</summary>
public class FiltroReporteRequest
{
    /// <summary>Si se omite, se toma el año en curso.</summary>
    public int? Ejercicio { get; set; }

    /// <summary>Si se omite, el 1 de enero del ejercicio.</summary>
    public DateTime? FechaInicio { get; set; }

    /// <summary>Si se omite, el 31 de diciembre del ejercicio.</summary>
    public DateTime? FechaCorte { get; set; }

    /// <summary>Si se omite, la del token. El reporte abarca todas las que comparten su numero.</summary>
    public int? IdJurisdiccion { get; set; }

    /// <summary>Ultimo nivel de cuenta a mostrar. Null trae el detalle completo.</summary>
    public int? NivelMaximo { get; set; }

    /// <summary>Descarta las cuentas que quedaron en cero.</summary>
    public bool SoloConMovimiento { get; set; }

    /// <summary>
    /// Si los renglones anulados suman. Por omision se toma de la configuracion, que los excluye.
    /// El reporte viejo los incluia porque el filtro estaba comentado.
    /// </summary>
    public bool? IncluirAnulados { get; set; }
}

/// <summary>Parametros del mayor.</summary>
public class FiltroMayoresRequest : FiltroReporteRequest
{
    /// <summary>Cuenta a listar. Sin cuenta trae todas las imputables.</summary>
    public int? IdCtaContable { get; set; }
}
