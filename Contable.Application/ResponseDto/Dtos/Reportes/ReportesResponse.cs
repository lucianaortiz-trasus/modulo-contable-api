namespace Contable.Application.ResponseDto.Dtos.Reportes;

/// <summary>Encabezado comun de los tres reportes contables.</summary>
public class EncabezadoReporte
{
    public int Ejercicio { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaCorte { get; set; }

    public string? Jurisdiccion { get; set; }

    /// <summary>Ids de jurisdiccion incluidos: los que comparten numero con el del usuario.</summary>
    public List<int> JurisdiccionesIncluidas { get; set; } = [];

    /// <summary>Si los renglones anulados entran en los importes.</summary>
    public bool IncluyeAnulados { get; set; }

    public DateTime GeneradoEl { get; set; } = DateTime.Now;
}

// ---- Sumas y saldos ----

public class SumasYSaldosResponse
{
    public EncabezadoReporte Encabezado { get; set; } = new();

    public int? NivelMaximo { get; set; }

    public List<FilaSumasYSaldosResponse> Filas { get; set; } = [];

    public decimal TotalSaldoDeudor { get; set; }

    public decimal TotalSaldoAcreedor { get; set; }

    /// <summary>En un balance bien armado los dos totales tienen que coincidir.</summary>
    public bool Cuadra { get; set; }

    /// <summary>
    /// Por que no cuadra, cuando no cuadra. Un balance descuadrado casi siempre tiene una causa
    /// concreta y decirla ahorra media hora de buscarla a mano.
    /// </summary>
    public string? MotivoDescuadre { get; set; }
}

public class FilaSumasYSaldosResponse
{
    public int IdCtaContable { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int? Nivel { get; set; }

    public int Profundidad { get; set; }

    public bool Imputable { get; set; }

    public decimal DebeAcumulado { get; set; }

    public decimal HaberAcumulado { get; set; }

    public decimal DebePeriodo { get; set; }

    public decimal HaberPeriodo { get; set; }

    public decimal DebeTotal { get; set; }

    public decimal HaberTotal { get; set; }

    public decimal SaldoDeudor { get; set; }

    public decimal SaldoAcreedor { get; set; }
}

// ---- Libro diario ----

public class LibroDiarioResponse
{
    public EncabezadoReporte Encabezado { get; set; } = new();

    public List<AsientoDiarioResponse> Asientos { get; set; } = [];

    public int TotalAsientos { get; set; }

    public decimal TotalDebe { get; set; }

    public decimal TotalHaber { get; set; }
}

public class AsientoDiarioResponse
{
    public int IdAsiento { get; set; }

    public int Numero { get; set; }

    public DateTime FecContable { get; set; }

    public string? Leyenda { get; set; }

    public string? TipoAsiento { get; set; }

    public bool Anulado { get; set; }

    public decimal TotalDebe { get; set; }

    public decimal TotalHaber { get; set; }

    public List<RenglonDiarioResponse> Renglones { get; set; } = [];
}

public class RenglonDiarioResponse
{
    public string? CodigoCuenta { get; set; }

    public string? NombreCuenta { get; set; }

    public decimal? Debe { get; set; }

    public decimal? Haber { get; set; }

    public bool Anulado { get; set; }
}

// ---- Mayores ----

public class MayoresResponse
{
    public EncabezadoReporte Encabezado { get; set; } = new();

    public List<MayorResponse> Mayores { get; set; } = [];
}

public class MayorResponse
{
    public int IdCtaContable { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal SaldoAnterior { get; set; }

    public string TipoSaldoAnterior { get; set; } = string.Empty;

    public decimal TotalDebe { get; set; }

    public decimal TotalHaber { get; set; }

    public decimal SaldoFinal { get; set; }

    public string TipoSaldoFinal { get; set; } = string.Empty;

    public List<RenglonMayorResponse> Renglones { get; set; } = [];
}

public class RenglonMayorResponse
{
    public int IdAsiento { get; set; }

    public int NumeroAsiento { get; set; }

    public DateTime FecContable { get; set; }

    public string? Leyenda { get; set; }

    public decimal Debe { get; set; }

    public decimal Haber { get; set; }

    public decimal Saldo { get; set; }

    public string TipoSaldo { get; set; } = string.Empty;

    public bool Anulado { get; set; }
}
