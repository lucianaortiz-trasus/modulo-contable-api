namespace Contable.Application.ResponseDto.Dtos.ModelosAsiento;

/// <summary>Item generico para los desplegables del formulario.</summary>
public class CatalogoItemResponse
{
    public int Id { get; set; }

    public string? Descripcion { get; set; }
}

/// <summary>Tipo de documento del desplegable, con sus letras y el aviso de si lleva letra.</summary>
public class TipoDocumentoCatalogoResponse : CatalogoItemResponse
{
    /// <summary>False bloquea el desplegable de tipo de comprobante.</summary>
    public bool TieneLetras { get; set; }

    /// <summary>Letras activas del documento; null cuando no tiene.</summary>
    public List<TipoComprobanteResponse>? Letras { get; set; }
}

/// <summary>Letra activa de un tipo de documento.</summary>
public class TipoComprobanteResponse
{
    public int IdTipodocLetra { get; set; }

    public string? Codigo { get; set; }
}

/// <summary>Un modelo en la grilla de la consulta general.</summary>
public class ModeloAsientoResumenResponse
{
    public int IdModeloAsiento { get; set; }

    public int IdPlantillaAsiento { get; set; }

    public int IdTipoOperacion { get; set; }

    public string? Operacion { get; set; }

    public int IdTipodoc { get; set; }

    public string? TipoDocumento { get; set; }

    public int? IdTipodocLetra { get; set; }

    public string? TipoComprobante { get; set; }

    public bool DiscriminaOt { get; set; }

    public string DiscriminaOtTexto => DiscriminaOt ? "Sí" : "No";

    public int CantidadDeRenglones { get; set; }

    public string? UsrIng { get; set; }

    public DateTime FecIng { get; set; }
}

/// <summary>
/// Modelo completo, tambien usado como respuesta de ObtenerPlantilla (sin id de modelo)
/// y de ObtenerModelo (con las cuentas del modelo para precargar el formulario).
/// </summary>
public class ModeloAsientoResponse : ModeloAsientoResumenResponse
{
    public string Formula { get; set; } = string.Empty;

    public List<RenglonPlantillaResponse> Renglones { get; set; } = [];
}

/// <summary>Un renglon de la plantilla del asiento.</summary>
public class RenglonPlantillaResponse
{
    public int IdPlantillaAsientoDetalle { get; set; }

    public int Orden { get; set; }

    /// <summary>PLAN_CUENTAS | MATRIZ_CONTABLE.</summary>
    public string OrigenCuenta { get; set; } = string.Empty;

    public string OrigenCuentaTexto =>
        OrigenCuenta == "PLAN_CUENTAS" ? "Plan de cuentas" : "Matriz Contable";

    public string Concepto { get; set; } = string.Empty;

    /// <summary>"Definida en la matriz" o el nombre/codigo de la cuenta elegida.</summary>
    public string CuentaContable { get; set; } = string.Empty;

    public bool Debita { get; set; }

    public bool Acredita { get; set; }

    public string? BaseCalculo { get; set; }

    /// <summary>Cuenta parametrizada; nulo en MATRIZ_CONTABLE y en una plantilla sin guardar.</summary>
    public int? IdCtaContable { get; set; }

    public string? CodigoCuenta { get; set; }

    public string? NombreCuenta { get; set; }

    /// <summary>Texto del boton de ayuda, solo en renglones MATRIZ_CONTABLE.</summary>
    public string? Tooltip { get; set; }
}

/// <summary>Una fila del historial del modelo.</summary>
public class HistorialModeloResponse
{
    public int IdHistorialModeloAsiento { get; set; }

    public DateTime FechaHs { get; set; }

    public string Usuario { get; set; } = string.Empty;

    /// <summary>Generar | Modificar.</summary>
    public string Accion { get; set; } = string.Empty;

    public string Comentarios { get; set; } = string.Empty;
}

/// <summary>Cuenta del plan para el buscador del modelo.</summary>
public class CuentaModeloResponse
{
    public int IdCtaContable { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string? TipoSaldo { get; set; }
}
