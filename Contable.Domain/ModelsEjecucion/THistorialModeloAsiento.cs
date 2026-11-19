namespace Contable.Domain.ModelsEjecucion;
public class THistorialModeloAsiento
{
    public int IdHistorialModeloAsiento { get; set; }
    public int? IdModeloAsiento { get; set; }
    public string? Accion { get; set; }
    public string? Comentarios { get; set; }
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public string? UsrBaja { get; set; }
    public DateTime? FecBaja { get; set; }
    public virtual TModeloAsiento? Modelo { get; set; }
}
