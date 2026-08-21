namespace Contable.Domain.ModelsEjecucion;

/// <summary>
/// Renglon de asiento contable. Tabla EJECUCION.T_ASIENTOS_DET.
/// </summary>
/// <remarks>
/// DEBE y HABER son NUMBER(18,2). El ABM viejo capturaba cinco decimales y la base truncaba
/// a dos sin avisar; aca el redondeo es explicito. Ver ReglasDetalleAsiento.
/// </remarks>
public class TAsientoDetalle
{
    public int IdAsientoDet { get; set; }

    public int IdAsiento { get; set; }

    public int IdCtaContable { get; set; }

    public DateTime? FecContable { get; set; }

    public decimal? Debe { get; set; }

    public decimal? Haber { get; set; }

    public int? IdIngresosDet { get; set; }

    public int? IdDevengadoDet { get; set; }

    public int? IdDietdacDet { get; set; }

    public int? IdMovTesoDet { get; set; }

    public int? IdElementoPago { get; set; }

    public string? UserIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserAct { get; set; }

    public DateTime? FecAct { get; set; }

    /// <summary>'S' cuando el renglon fue anulado por una reversa o un contraasiento.</summary>
    public string? Anulado { get; set; }

    public int? IdRecursoDh { get; set; }

    public int? IdComprobanteDeduccion { get; set; }

    public int? IdRetencionLiquidacion { get; set; }

    public int? IdAporteDet { get; set; }

    public int? IdCobro { get; set; }

    public int? IdCatProg { get; set; }

    public int? IdObjetoGasto { get; set; }

    public int? IdCentroCosto { get; set; }

    public int? IdOficina { get; set; }

    public int? IdSelloDetalle { get; set; }

    public int? IdObras { get; set; }

    public int? IdRurDet { get; set; }

    public int? IdAuxRec { get; set; }

    public int? IdAuxCtaBan { get; set; }

    public int? IdAuxOg { get; set; }

    public int? IdCuentaBancaria { get; set; }

    public int? IdLibroBanco { get; set; }

    public virtual TAsiento? Asiento { get; set; }

    public virtual TCuentaContable? CuentaContable { get; set; }
}
