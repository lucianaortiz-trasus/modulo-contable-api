namespace Contable.Application.ResponseDto.Dtos.PlanDeCuentas;

/// <summary>Una cuenta del plan, con todo lo que muestra el ABM.</summary>
public class CuentaContableResponse
{
    public int IdCtaContable { get; set; }

    public int? IdCtaContableRel { get; set; }

    public string? CodigoPadre { get; set; }

    public string? NombrePadre { get; set; }

    public string CuentaSuperior => string.IsNullOrWhiteSpace(CodigoPadre)
        ? "No posee"
        : $"{CodigoPadre} {NombrePadre}".Trim();

    public string Codigo { get; set; } = string.Empty;

    /// <summary>Codigo con los segmentos completados con ceros. Es el que sirve para ordenar.</summary>
    public string CodigoNormalizado { get; set; } = string.Empty;

    /// <summary>Los siete segmentos por separado, como los muestra el ABM.</summary>
    public List<string> Segmentos { get; set; } = [];

    public string Nombre { get; set; } = string.Empty;

    public int? Nivel { get; set; }

    public bool Activa { get; set; }

    public bool Imputable { get; set; }

    /// <summary>'D' deudor o 'A' acreedor.</summary>
    public string TipoSaldo { get; set; } = string.Empty;

    public int? IdNaturaleza { get; set; }

    public string Naturaleza => (IdNaturaleza, Codigo.FirstOrDefault()) switch
    {
        (1, _) or (null, '1') => "ACTIVO",
        (2, _) or (null, '2') => "PASIVO",
        (3, _) or (null, '3') => "PATRIMONIO NETO",
        (4, _) or (null, '4') => "INGRESOS",
        (5, _) or (null, '5') => "EGRESOS",
        _ => "-"
    };

    public int? IdRecursoDebe { get; set; }

    public int? IdRecursoHaber { get; set; }

    public bool Auxiliar { get; set; }

    public bool AsientoOps { get; set; }

    public bool Inventariable { get; set; }

    public int? IdCtaBancaria { get; set; }

    public int? IdProveedor { get; set; }

    public int Amortizacion { get; set; }

    public int IngresoPropio { get; set; }

    public int IdJurisdiccion { get; set; }

    public int IdVigencia { get; set; }

    public int CantidadDeHijas { get; set; }

    public int CantidadDeHijasActivas { get; set; }

    public bool TieneMovimientos { get; set; }

    public bool TieneRelacionMatrizActiva { get; set; }

    public string? UserIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserAct { get; set; }

    public DateTime? FecAct { get; set; }
}

/// <summary>Un nodo del arbol del plan de cuentas.</summary>
public class CuentaArbolResponse
{
    public int IdCtaContable { get; set; }

    public int? IdCtaContableRel { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int? Nivel { get; set; }

    public int Profundidad { get; set; }

    public bool Activa { get; set; }

    public bool Imputable { get; set; }

    public string TipoSaldo { get; set; } = string.Empty;

    /// <summary>Nombres de los problemas estructurales detectados. Vacio si la cuenta esta sana.</summary>
    public List<string> Anomalias { get; set; } = [];

    /// <summary>La cuenta es de otra jurisdiccion y se muestra solo para sostener la estructura.</summary>
    public bool FueraDeAlcance { get; set; }

    public List<CuentaArbolResponse> Hijas { get; set; } = [];
}

/// <summary>El arbol completo mas el resumen de lo que se encontro al armarlo.</summary>
public class ArbolPlanDeCuentasResponse
{
    public List<CuentaArbolResponse> Raices { get; set; } = [];

    public int TotalCuentas { get; set; }

    /// <summary>
    /// Cuentas de otra jurisdiccion que se traen solo para que el arbol no quede partido.
    /// Son de contexto: no se editan ni cuentan como propias.
    /// </summary>
    public int CuentasDeContexto { get; set; }

    public int TotalImputables { get; set; }

    /// <summary>
    /// Cuentas con problemas estructurales. Son datos que ya vienen mal de la base: el modulo los
    /// muestra en lugar de esconderlos.
    /// </summary>
    public List<CuentaConAnomaliaResponse> CuentasConAnomalias { get; set; } = [];
}

public class CuentaConAnomaliaResponse
{
    public int IdCtaContable { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public List<string> Anomalias { get; set; } = [];
}

/// <summary>Proximo codigo libre bajo una cuenta padre.</summary>
public class ProximoCodigoResponse
{
    public string? Codigo { get; set; }

    public int Nivel { get; set; }

    public int? Segmento { get; set; }

    public bool HayDisponible { get; set; }

    /// <summary>Hijas cuyo codigo no cuelga del padre y quedaron fuera del calculo.</summary>
    public int HijasIgnoradas { get; set; }
}
