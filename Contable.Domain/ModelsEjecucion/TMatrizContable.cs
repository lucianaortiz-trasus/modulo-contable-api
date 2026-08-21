namespace Contable.Domain.ModelsEjecucion;

/// <summary>
/// Relacion cuenta contable con objeto de gasto o recurso. Tabla EJECUCION.T_MATRIZ_CONTABLE.
/// </summary>
/// <remarks>
/// En la base todas las columnas son nullable, incluida la clave primaria.
/// </remarks>
public class TMatrizContable
{
    public int IdMatrizCble { get; set; }

    public int? IdCtaContable { get; set; }

    public int? IdObjetoGasto { get; set; }

    public int? IdRecurso { get; set; }

    public int? IdVigencia { get; set; }

    public string? UserIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserAct { get; set; }

    public DateTime? FecAct { get; set; }

    public virtual TCuentaContable? CuentaContable { get; set; }
}
