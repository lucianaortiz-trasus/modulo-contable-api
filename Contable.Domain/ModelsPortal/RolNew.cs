namespace Contable.Domain.ModelsPortal;

/// <summary>Rol del portal. Tabla PORTAL.ROLES_NEW.</summary>
public class RolNew
{
    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public int AccessoId { get; set; }

    public virtual ICollection<RolUsuarioNew> RolesUsuarios { get; set; } = new List<RolUsuarioNew>();
}
