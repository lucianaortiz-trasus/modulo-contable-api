namespace Contable.Domain.ModelsPresupuesto;

/// <summary>Jurisdiccion. Solo lectura desde este modulo.</summary>
public class TJurisdiccion
{
    public int IdJurisdiccion { get; set; }

    /// <summary>Numero estable entre vigencias: es la clave para traducir de un ejercicio a otro.</summary>
    public string? NumeroJurisdiccion { get; set; }

    public string? NombreJurisdiccion { get; set; }

    public int? IdVigencia { get; set; }

    public int? EntId { get; set; }

    public int? IdSecIns { get; set; }

    public string? Sigla { get; set; }

    public string? UserIdIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserIdAct { get; set; }

    public DateTime? FecAct { get; set; }

    public string? Descripcion { get; set; }

    public string? Notas { get; set; }

    public int? IdGrupoSector { get; set; }
}
