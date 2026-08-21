namespace Contable.Domain.Auditoria;

/// <summary>
/// Campos de auditoria comunes. Las entidades que lo implementan reciben un filtro global
/// que descarta las filas dadas de baja.
/// </summary>
public interface IEntityAuditable
{
    int Id { get; set; }

    string? UsrIng { get; set; }

    DateTime? FecIng { get; set; }

    string? UsrMod { get; set; }

    DateTime? FecMod { get; set; }

    string? UserBaja { get; set; }

    DateTime? FecBaja { get; set; }
}
