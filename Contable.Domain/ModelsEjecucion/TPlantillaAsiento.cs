namespace Contable.Domain.ModelsEjecucion;

public class TPlantillaAsiento
{
    public int IdPlantillaAsiento { get; set; }
    public int? IdTipoOperacion { get; set; }
    public int? IdTipodoc { get; set; }
    /// <summary>Nulo cuando el documento no tiene letras activas.</summary>
    public int? IdTipodocLetra { get; set; }
    /// <summary>'S' / 'N'.</summary>
    public string? DiscriminaOt { get; set; } = ReglasModelosAsiento.No;
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public string? UsrBaja { get; set; }
    public DateTime? FecBaja { get; set; }
    public virtual TTipoOperacionAsiento? TipoOperacion { get; set; }
    public virtual TTipoDocumento? TipoDocumento { get; set; }
    public virtual TTipoDocumentoLetra? TipoDocumentoLetra { get; set; }
    public virtual ICollection<TPlantillaAsientoDetalle> Renglones { get; set; } = new List<TPlantillaAsientoDetalle>();
    [NotMapped]
    public bool DiscriminaOtrosTributos => DiscriminaOt == ReglasModelosAsiento.Si;
}
