namespace Contable.Application.ResponseDto.Dtos.Estadisticas;

/// <summary>Indicadores de cabecera del módulo.</summary>
public class TableroResponse
{
    public int Ejercicio { get; set; }

    public int AsientosDelEjercicio { get; set; }

    public int AsientosDelMes { get; set; }

    public decimal MontoDelEjercicio { get; set; }

    public int? UltimoNumero { get; set; }

    public DateTime? UltimaFechaContable { get; set; }

    public int CuentasImputablesActivas { get; set; }

    public int CuentasConMovimiento { get; set; }

    /// <summary>Porcentaje de cuentas imputables que efectivamente se usaron.</summary>
    public decimal CoberturaDelPlan { get; set; }

    public int RelacionesEnLaMatriz { get; set; }
}

// ---- Panel de actividad ----

public class ActividadResponse
{
    public int Ejercicio { get; set; }

    public List<ActividadDelMes> PorMes { get; set; } = [];

    public List<ConteoConNombre> PorTipo { get; set; } = [];

    /// <summary>El mismo corte del ejercicio anterior, para comparar.</summary>
    public List<ActividadDelMes> PorMesEjercicioAnterior { get; set; } = [];

    public int TotalAsientos { get; set; }

    public decimal TotalMonto { get; set; }

    public decimal VariacionInteranual { get; set; }
}

public class ActividadDelMes
{
    public int Mes { get; set; }

    public string NombreMes { get; set; } = string.Empty;

    public int Asientos { get; set; }

    public decimal Monto { get; set; }
}

public class ConteoConNombre
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal Monto { get; set; }
}

// ---- Panel de cuentas ----

public class CuentasResponse
{
    public List<CuentaMovidaResponse> MasMovidas { get; set; } = [];

    public List<CuentaMovidaResponse> SinMovimiento { get; set; } = [];

    public int TotalImputables { get; set; }

    public int ConMovimiento { get; set; }

    /// <summary>Qué porcentaje del monto total concentran las diez cuentas más movidas.</summary>
    public decimal ConcentracionEnDiez { get; set; }
}

public class CuentaMovidaResponse
{
    public int IdCtaContable { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int Movimientos { get; set; }

    public decimal Debe { get; set; }

    public decimal Haber { get; set; }

    public decimal Total => Debe + Haber;
}

// ---- Panel de calidad de registración ----

public class CalidadResponse
{
    /// <summary>Días entre la fecha contable y la de carga, por tramos.</summary>
    public List<ConteoConNombre> DesfaseDeCarga { get; set; } = [];

    public decimal DesfasePromedioEnDias { get; set; }

    public List<ConteoConNombre> PorUsuario { get; set; } = [];

    public int AsientosSinLeyendaUtil { get; set; }

    public int AsientosAnulados { get; set; }

    public decimal PorcentajeAnulado { get; set; }
}

// ---- Sugerencia de imputación ----

public class SugerenciaResponse
{
    public int IdCtaContable { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal Puntaje { get; set; }

    public string Origen { get; set; } = string.Empty;

    public string Motivo { get; set; } = string.Empty;
}

/// <summary>Aviso sobre un importe fuera de lo habitual para su cuenta.</summary>
public class AvisoDeImporteResponse
{
    public bool EsAtipico { get; set; }

    public string Grado { get; set; } = string.Empty;

    public decimal ImporteHabitual { get; set; }

    public decimal Veces { get; set; }

    public string? Motivo { get; set; }
}
