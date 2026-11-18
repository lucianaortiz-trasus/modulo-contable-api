namespace Contable.Domain.ModelsEjecucion;

public class TTipoDocumento
{
    public int IdTipodoc { get; set; }
    public int? Codigo { get; set; }
    public string? Descripcion { get; set; }

    /// <summary>'S' / 'N', default 'N'.</summary>
    public string? Baja { get; set; }
    public int? IdTipoComprobante { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? IdEtapa { get; set; }
    public string? UsrAlta { get; set; }
    public DateTime? FecAlta { get; set; }
    public string? UsrModif { get; set; }
    public DateTime? FecModif { get; set; }
    public virtual ICollection<TTipoDocumentoLetra> Letras { get; set; } = new List<TTipoDocumentoLetra>();
}
