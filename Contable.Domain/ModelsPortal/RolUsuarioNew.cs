namespace Contable.Domain.ModelsPortal;

/// <summary>Asignacion de rol a usuario. Tabla PORTAL.ROLES_USUARIOS_NEW.</summary>
public class RolUsuarioNew
{
    public int RolUsuarioId { get; set; }

    public int RolId { get; set; }

    public int UsuarioId { get; set; }

    public virtual RolNew Rol { get; set; } = null!;
}
