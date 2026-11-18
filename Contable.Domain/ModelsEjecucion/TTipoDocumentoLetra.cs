namespace Contable.Domain.ModelsEjecucion;

public class TTipoDocumentoLetra
{
    public int IdTipodocLetra { get; set; }
    public int IdTipoDoc { get; set; }
    public string? Codigo { get; set; }
    public int? Activo { get; set; }
    public DateTime? FecBaja { get; set; }
    public string? UsrBaja { get; set; }
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public virtual TTipoDocumento? Documento { get; set; }
}
