namespace Contable.Domain.ModelsEjecucion;

/// <summary>
/// Cabecera de asiento contable. Tabla EJECUCION.T_ASIENTOS.
/// </summary>
/// <remarks>
/// La tabla guarda ID_JURISDICCION, no ID_UNIDAD_ADM: el PL/SQL original recibia la unidad
/// administrativa y la traducia internamente. Ver ReglasJurisdiccionPorUnidadAdm.
/// </remarks>
public class TAsiento
{
    public int IdAsiento { get; set; }

    /// <summary>Correlativo por jurisdiccion. No se reinicia por ejercicio.</summary>
    public int Numero { get; set; }

    public int Anio { get; set; }

    public int IdVigencia { get; set; }

    public int? IdJurisdiccion { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FecAlta { get; set; }

    public string? UserAlta { get; set; }

    public DateTime? FecAct { get; set; }

    public string? UserAct { get; set; }

    /// <summary>Asiento de origen cuando este es una reversa o un contraasiento.</summary>
    public int? IdAsientoRel { get; set; }

    public int? IdDevengado { get; set; }

    public int? IdDietdac { get; set; }

    public int? IdAnticipo { get; set; }

    public int IdTipoAsiento { get; set; }

    public DateTime FecContable { get; set; }

    public string? Leyenda { get; set; }

    public int? IdTransferencia { get; set; }

    public int? IdMovTeso { get; set; }

    public int? IdLibroBanco { get; set; }

    public int? IdBien { get; set; }

    public int? IdAporte { get; set; }

    public int? IdSello { get; set; }

    public int? IdProveedor { get; set; }

    public int? IdRur { get; set; }

    public virtual TTipoAsiento? TipoAsiento { get; set; }

    public virtual ICollection<TAsientoDetalle> Detalles { get; set; } = new List<TAsientoDetalle>();
}
