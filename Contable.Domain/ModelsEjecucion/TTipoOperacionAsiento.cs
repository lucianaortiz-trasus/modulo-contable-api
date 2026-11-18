namespace Contable.Domain.ModelsEjecucion;

public class TTipoOperacionAsiento
{
    public int IdTipoOperacion { get; set; }
    public string? Codigo { get; set; }
    public string? NTipoOperacion { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; } = 1;
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public string? UsrBaja { get; set; }
    public DateTime? FecBaja { get; set; }
    public virtual ICollection<TPlantillaAsiento> Plantillas { get; set; } = new List<TPlantillaAsiento>();
}
