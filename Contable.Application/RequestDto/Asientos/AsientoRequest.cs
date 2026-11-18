using System.ComponentModel.DataAnnotations;

namespace Contable.Application.RequestDto.Asientos;

/// <summary>Alta de un asiento manual, cabecera y renglones en una sola operacion.</summary>
public class AsientoRequest
{
    [Required]
    public DateTime? FecContable { get; set; }

    /// <summary>Si se omite, se toma la del token.</summary>
    public int? IdUnidadAdm { get; set; }

    [Required]
    public int? IdTipoAsiento { get; set; }

    [Required]
    [MaxLength(4000)]
    public string? Leyenda { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public List<RenglonRequest> Renglones { get; set; } = [];
}

public class RenglonRequest
{
    [Required]
    public int IdCtaContable { get; set; }

    public decimal? Debe { get; set; }

    public decimal? Haber { get; set; }

    public int? IdRecursoDh { get; set; }

    public int? IdObjetoGasto { get; set; }

    public int? IdCuentaBancaria { get; set; }
}

/// <summary>Filtros de la grilla de asientos.</summary>
public class FiltroAsientosRequest
{
    public int? IdVigencia { get; set; }

    public int? IdJurisdiccion { get; set; }

    public DateTime? FechaDesde { get; set; }

    public DateTime? FechaHasta { get; set; }

    public int? IdTipoAsiento { get; set; }

    /// <summary>
    /// Clasificación del tipo: Todos, Manual, Automático, Cierre o Apertura.
    /// </summary>
    public string? Tipo { get; set; }

    public int? Numero { get; set; }

    /// <summary>Busca dentro de la leyenda.</summary>
    public string? Leyenda { get; set; }

    /// <summary>null trae todos, true solo anulados, false solo vigentes.</summary>
    public bool? Anulados { get; set; }

    /// <summary>"Todos", "Vigentes", "Reversados" o "Anulados".</summary>
    public string? Estado { get; set; }

    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 20;
}
