namespace Contable.Domain.ModelsEjecucion;

/// <summary>
/// Cuenta del plan de cuentas. Tabla EJECUCION.T_CUENTAS_CONTABLES.
/// </summary>
/// <remarks>
/// La jerarquia se resuelve por IdCtaContableRel, nunca por prefijo de Codigo: el nivel 1 usa
/// codigos de un caracter ("1") mientras el nivel 2 usa "01.01", asi que el padre no es prefijo
/// del hijo. Ver ReglasJerarquiaCuentas.
/// </remarks>
public class TCuentaContable : INodoPlanDeCuentas, ICuentaConNombre
{
    public int IdCtaContable { get; set; }

    /// <summary>Cuenta padre. Null en el nivel 1.</summary>
    public int? IdCtaContableRel { get; set; }

    /// <summary>Codigo posicional NN.NN.NN.NN.NN.NN.NNNN. Ver ReglasCodigoCuenta.</summary>
    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    /// <summary>'S' / 'N'.</summary>
    public string Activa { get; set; } = null!;

    /// <summary>'D' deudor / 'A' acreedor. Es la naturaleza esperada, no el saldo calculado.</summary>
    public string TipoSaldo { get; set; } = null!;

    public int? Nivel { get; set; }

    public string? UserIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserAct { get; set; }

    public DateTime? FecAct { get; set; }

    /// <summary>1 y 2 patrimoniales, 3 patrimonio neto, 4 y 5 de resultado. Sin catalogo cargado.</summary>
    public int? IdNaturaleza { get; set; }

    public int? IdRecursoDebe { get; set; }

    public int? IdRecursoHaber { get; set; }

    /// <summary>'S' solo en las hojas: unicamente las imputables reciben movimientos.</summary>
    public string Imputable { get; set; } = null!;

    public string? Auxiliar { get; set; }

    public int IdJurisdiccion { get; set; }

    public int IdVigencia { get; set; }

    public string? AsientoOps { get; set; }

    public int? IdCtaBancaria { get; set; }

    public string? Inventariable { get; set; }

    public string? ClasificadorRelacionado { get; set; }

    public int? IdProveedor { get; set; }

    public int Amortizacion { get; set; }

    public int IngresoPropio { get; set; }

    public virtual TCuentaContable? Padre { get; set; }

    public virtual ICollection<TCuentaContable> Hijas { get; set; } = new List<TCuentaContable>();

    // Adaptacion a INodoPlanDeCuentas, que es lo que consume ArbolDeCuentas. Son proyecciones de
    // columnas que ya existen, no campos nuevos: van sin mapear.

    [NotMapped]
    public int Id => IdCtaContable;

    [NotMapped]
    public int? IdPadre => IdCtaContableRel;

    [NotMapped]
    public bool EsImputable => Imputable == ReglasJerarquiaCuentas.MarcaSi;

    /// <summary>Ojo: ACTIVA usa 'A' / 'I', no 'S' / 'N' como el resto de las marcas.</summary>
    [NotMapped]
    public bool EstaActiva => Activa == ReglasJerarquiaCuentas.CuentaActiva;
}
