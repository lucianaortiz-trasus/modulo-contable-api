namespace Contable.Application.ResponseDto.Dtos.Asientos;

/// <summary>Un asiento en la grilla.</summary>
public class AsientoResumenResponse
{
    public int IdAsiento { get; set; }

    public int Numero { get; set; }

    public int Anio { get; set; }

    public DateTime FecContable { get; set; }

    public string? Leyenda { get; set; }

    public int IdTipoAsiento { get; set; }

    public string? CodigoTipoAsiento { get; set; }

    public string? TipoAsiento { get; set; }

    public bool EsManual { get; set; }

    public string TipoClasificacion => EsManual ? "Manual" : "Automático";

    public decimal TotalDebe { get; set; }

    public decimal TotalHaber { get; set; }

    public int CantidadDeRenglones { get; set; }

    /// <summary>Todos los renglones estan marcados como anulados.</summary>
    public bool Anulado { get; set; }

    /// <summary>Indica si el asiento fue reversado.</summary>
    public bool Reversado { get; set; }

    public string Estado => !Anulado ? "Vigente" : (Reversado ? "Reversado" : "Anulado");

    /// <summary>Asiento de origen, cuando este es una reversa o un contraasiento.</summary>
    public int? IdAsientoRel { get; set; }

    /// <summary>Otro asiento apunta a este como origen.</summary>
    public bool TieneContrapartida { get; set; }

    public int? IdJurisdiccion { get; set; }

    public string? UserAlta { get; set; }

    public DateTime? FecAlta { get; set; }
}

/// <summary>Un asiento con sus renglones.</summary>
public class AsientoResponse : AsientoResumenResponse
{
    public string? Observaciones { get; set; }

    public int IdVigencia { get; set; }

    public bool Cuadra { get; set; }

    public List<RenglonResponse> Renglones { get; set; } = [];
}

public class RenglonResponse
{
    public int IdAsientoDet { get; set; }

    public int IdCtaContable { get; set; }

    public string? CodigoCuenta { get; set; }

    public string? NombreCuenta { get; set; }

    public decimal? Debe { get; set; }

    public decimal? Haber { get; set; }

    public int? IdRecursoDh { get; set; }

    public int? IdObjetoGasto { get; set; }

    public int? IdCuentaBancaria { get; set; }

    public bool Anulado { get; set; }
}

/// <summary>Vista previa de un cierre o una apertura de ejercicio.</summary>
public class PlanAsientosResponse
{
    public int Ejercicio { get; set; }

    public bool SePuedeGenerar { get; set; }

    public string? Motivo { get; set; }

    /// <summary>True cuando los asientos ya se generaron; false si es la vista previa.</summary>
    public bool Confirmado { get; set; }

    public List<AsientoPlanificadoResponse> Asientos { get; set; } = [];
}

public class AsientoPlanificadoResponse
{
    public string Leyenda { get; set; } = string.Empty;

    public DateTime FecContable { get; set; }

    public int IdTipoAsiento { get; set; }

    public int CantidadDeRenglones { get; set; }

    public decimal TotalDebe { get; set; }

    public decimal TotalHaber { get; set; }

    public bool Cuadra { get; set; }

    /// <summary>Id del asiento generado. Null en la vista previa.</summary>
    public int? IdAsiento { get; set; }

    public int? Numero { get; set; }

    public List<RenglonResponse> Renglones { get; set; } = [];
}

/// <summary>Tipo de asiento del catalogo.</summary>
/// <remarks>
/// El codigo 'AP' esta duplicado en dos filas del catalogo (AUTOMATICO PURO y DE APERTURA), asi
/// que los selectores tienen que trabajar por Id y nunca por codigo.
/// </remarks>
public class TipoAsientoResponse
{
    public int IdTipoAsiento { get; set; }

    public string? Codigo { get; set; }

    public string? Descripcion { get; set; }
}
