namespace Contable.Domain.ModelsPortal;

/// <summary>Usuario del portal SIGEMyT. Tabla PORTAL.T_USUARIOS_NEW.</summary>
public class TUsuarioNew
{
    public int UsuarioId { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public byte Estado { get; set; }

    public int? ProveedorId { get; set; }

    public int JurisdiccionId { get; set; }

    public string? IdPersona { get; set; }

    public virtual TPersona? Persona { get; set; }
}
