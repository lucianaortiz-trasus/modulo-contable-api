namespace Contable.Domain.ModelsEjecucion;

public class TModeloAsiento
{
    public int IdModeloAsiento { get; set; }
    public int? IdPlantillaAsiento { get; set; }
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public string? UsrBaja { get; set; }
    public DateTime? FecBaja { get; set; }
    public virtual TPlantillaAsiento? Plantilla { get; set; }
    public virtual ICollection<TModeloAsientoDetalle> Detalles { get; set; } = new List<TModeloAsientoDetalle>();
    public virtual ICollection<THistorialModeloAsiento> Historial { get; set; } = new List<THistorialModeloAsiento>();
}
