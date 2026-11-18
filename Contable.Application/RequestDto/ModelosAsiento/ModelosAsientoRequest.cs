using System.ComponentModel.DataAnnotations;
using Contable.Application.RequestDto.Comun;

namespace Contable.Application.RequestDto.ModelosAsiento;

/// <summary>Filtros de la grilla de parametrizacion.</summary>
public class FiltroModelosAsientoRequest
{
    public int? IdTipoOperacion { get; set; }

    public int? IdTipodoc { get; set; }

    public int? IdTipodocLetra { get; set; }

    public bool? DiscriminaOt { get; set; }
}

/// <summary>Combinacion que identifica a un modelo. La usan Generar y Guardar.</summary>
public class DatosCombinacionRequest
{
    [Required]
    public int? IdTipoOperacion { get; set; }

    [Required]
    public int? IdTipodoc { get; set; }

    /// <summary>Nulo cuando el documento no tiene letras activas.</summary>
    public int? IdTipodocLetra { get; set; }

    public bool DiscriminaOt { get; set; }
}

/// <summaryObtener la plantilla predefinida. Con la combinacion alcanza.</summary>
public class ObtenerPlantillaRequest : DatosCombinacionRequest
{
}

/// <summary>Guardar el modelo (alta o clon). Cuentas solo para renglones PLAN_CUENTAS.</summary>
public class GuardarModeloAsientoRequest : DatosCombinacionRequest
{
    public List<CuentaRenglonRequest> Cuentas { get; set; } = [];
}

/// <summary>Edicion: la combinacion no se toca, solo las cuentas.</summary>
public class EditarModeloAsientoRequest
{
    public List<CuentaRenglonRequest> Cuentas { get; set; } = [];
}

public class CuentaRenglonRequest
{
    [Required]
    public int IdPlantillaAsientoDetalle { get; set; }

    [Required]
    public int? IdCtaContable { get; set; }
}

/// <summary>Buscador del plan de cuentas para el selector de cuenta contable.</summary>
public class FiltroCuentasModeloRequest : PaginacionRequest
{
    public string? Codigo { get; set; }

    public string? Nombre { get; set; }
}
