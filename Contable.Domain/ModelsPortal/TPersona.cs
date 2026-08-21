namespace Contable.Domain.ModelsPortal;

/// <summary>Persona del portal. Tabla PORTAL.T_PERSONAS.</summary>
public class TPersona : IEntityAuditable
{
    public string IdPersona { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string DocumentoNro { get; set; } = null!;

    public string? Cuil { get; set; }

    public string? Email { get; set; }

    public int? IdJurisdiccion { get; set; }

    [NotMapped]
    public int Id { get; set; }

    public string? UsrIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UsrMod { get; set; }

    public DateTime? FecMod { get; set; }

    public string? UserBaja { get; set; }

    public DateTime? FecBaja { get; set; }
}
