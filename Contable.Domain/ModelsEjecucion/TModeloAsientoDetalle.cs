namespace Contable.Domain.ModelsEjecucion;

public class TModeloAsientoDetalle
{
    public int IdModeloAsientoDetalle { get; set; }
    public int? IdModeloAsiento { get; set; }
    public int? IdPlantillaAsientoDetalle { get; set; }
    public int? IdCtaContable { get; set; }
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public string? UsrBaja { get; set; }
    public DateTime? FecBaja { get; set; }
    public virtual TModeloAsiento? Modelo { get; set; }
    public virtual TPlantillaAsientoDetalle? RenglonPlantilla { get; set; }
    public virtual TCuentaContable? CuentaContable { get; set; }
}
