namespace Contable.Domain.ModelsPresupuesto;

/// <summary>Unidad administrativa. Solo lectura desde este modulo.</summary>
public class TUnidadAdministrativa
{
    public int IdUnidadAdm { get; set; }

    /// <summary>Numero estable entre vigencias: es la clave de la traduccion entre ejercicios.</summary>
    public string? NumeroUnidadAdm { get; set; }

    public string? NombreUnidadAdm { get; set; }

    public int? IdVigencia { get; set; }

    public int? IdJurisdiccion { get; set; }

    public string? UserIdIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserIdAct { get; set; }

    public DateTime? FecAct { get; set; }

    public string? Estado { get; set; }

    public string? NroServicioAdm { get; set; }

    public int? IdProveedor { get; set; }

    public byte[]? Archivo { get; set; }
}
