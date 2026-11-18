namespace Contable.Domain.ModelsEjecucion;

public class TPlantillaAsientoDetalle
{
    public int IdPlantillaAsientoDetalle { get; set; }
    public int? IdPlantillaAsiento { get; set; }
    public int? Orden { get; set; }

    /// <summary>PLAN_CUENTAS | MATRIZ_CONTABLE.</summary>
    public string? OrigenCuenta { get; set; }
    public string? Concepto { get; set; }
    /// <summary>'S' / 'N'.</summary>
    public string? Debita { get; set; } = ReglasModelosAsiento.No;
    /// <summary>'S' / 'N'.</summary>
    public string? Acredita { get; set; } = ReglasModelosAsiento.No;
    public string? BaseCalculo { get; set; }
    public string? UsrIng { get; set; }
    public DateTime? FecIng { get; set; }
    public string? UsrMod { get; set; }
    public DateTime? FecMod { get; set; }
    public string? UsrBaja { get; set; }
    public DateTime? FecBaja { get; set; }
    public virtual TPlantillaAsiento? Plantilla { get; set; }

    [NotMapped]
    public bool EsPlanCuentas => OrigenCuenta == ReglasModelosAsiento.OrigenPlanCuentas;

    [NotMapped]
    public bool EsMatrizContable => OrigenCuenta == ReglasModelosAsiento.OrigenMatrizContable;
}
