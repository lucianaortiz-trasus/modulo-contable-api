namespace Contable.Domain.ModelsPresupuesto;

/// <summary>Ejercicio presupuestario. Solo lectura desde este modulo.</summary>
public class TVigencia
{
    public int IdVigencia { get; set; }

    public int? Ejercicio { get; set; }

    public string? ActivoFormulacion { get; set; }

    /// <summary>'S' en la vigencia abierta para ejecucion.</summary>
    public string? ActivoEjecucion { get; set; }

    public int? MaximoNivelCp { get; set; }

    public string? UserIdIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserIdAct { get; set; }

    public DateTime? FecAct { get; set; }

    public string? ProcesoCierre { get; set; }
}
