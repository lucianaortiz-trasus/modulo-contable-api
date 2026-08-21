using System.ComponentModel.DataAnnotations;

namespace Contable.Application.RequestDto.PlanDeCuentas;

/// <summary>Alta o edicion de una cuenta del plan.</summary>
public class CuentaContableRequest
{
    /// <summary>Cuenta padre. Null solo para una cuenta de nivel 1.</summary>
    public int? IdCtaContableRel { get; set; }

    /// <summary>
    /// Codigo completo. Si viene vacio en un alta, se calcula el proximo libre bajo el padre.
    /// </summary>
    public string? Codigo { get; set; }

    [Required]
    [MaxLength(500)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Si se omite en un alta, se toma el nivel del padre mas uno.</summary>
    public int? Nivel { get; set; }

    public bool Activa { get; set; } = true;

    public bool Imputable { get; set; }

    /// <summary>'D' deudor o 'A' acreedor.</summary>
    [Required]
    public string TipoSaldo { get; set; } = "D";

    public int? IdNaturaleza { get; set; }

    public int? IdRecursoDebe { get; set; }

    public int? IdRecursoHaber { get; set; }

    public bool Auxiliar { get; set; }

    public bool AsientoOps { get; set; }

    public bool Inventariable { get; set; }

    public int? IdCtaBancaria { get; set; }

    public int? IdProveedor { get; set; }

    public int Amortizacion { get; set; }

    public int IngresoPropio { get; set; }
}

/// <summary>Filtros de la busqueda paginada del plan de cuentas.</summary>
public class FiltroCuentasRequest
{
    public string? Codigo { get; set; }

    public string? Nombre { get; set; }

    public int? Nivel { get; set; }

    public bool? Imputable { get; set; }

    public bool? Activa { get; set; }

    public int? IdNaturaleza { get; set; }

    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 20;
}
