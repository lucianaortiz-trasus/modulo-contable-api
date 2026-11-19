using Contable.Application.RequestDto.ModelosAsiento;
using Contable.Application.ResponseDto.Dtos.ModelosAsiento;

namespace Contable.API.Controllers;

/// <summary>
/// Parametrizacion de asientos contables: catalogos, plantillas predefinidas, modelos e historial
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ParametrizacionAsientosController(IModelosAsientoService servicio) : BaseController
{
    private readonly IModelosAsientoService _servicio = servicio;

    /// <summary>Tipos de operacion activos del formulario.</summary>
    [HttpGet("TipoOperaciones")]
    [ProducesResponseType(typeof(OperationResponse<List<CatalogoItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipoOperaciones() => Return(await _servicio.GetTipoOperaciones());

    /// <summary>Tipos de documento activos, con sus letras (null cuando no tiene).</summary>
    [HttpGet("TipoDocumentos")]
    [ProducesResponseType(typeof(OperationResponse<List<TipoDocumentoCatalogoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipoDocumentos() => Return(await _servicio.GetTipoDocumentos());

    /// <summary>Letras activas del documento.</summary>
    [HttpGet("Letras/{idTipodoc:int}")]
    [ProducesResponseType(typeof(OperationResponse<List<TipoComprobanteResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLetras(int idTipodoc) => Return(await _servicio.GetLetras(idTipodoc));

    /// <summary>Buscador de cuentas activas e imputables del plan de cuentas.</summary>
    [HttpGet("Cuentas")]
    [ProducesResponseType(typeof(OperationResponse<List<CuentaModeloResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarCuentas([FromQuery] FiltroCuentasModeloRequest filtro)
        => Return(await _servicio.BuscarCuentas(filtro));

    /// <summary>Grilla de la consulta general. Devuelve lista vacia si no hay modelos.</summary>
    [HttpGet("Buscar")]
    [ProducesResponseType(typeof(OperationResponse<List<ModeloAsientoResumenResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] FiltroModelosAsientoRequest filtro)
        => Return(await _servicio.Buscar(filtro));

    /// <summary>Devuelve la plantilla predefinida de la combinacion.</summary>
    [HttpGet("ObtenerPlantilla")]
    [ProducesResponseType(typeof(OperationResponse<ModeloAsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerPlantilla([FromQuery] ObtenerPlantillaRequest request)
        => Return(await _servicio.ObtenerPlantilla(request));

    /// <summary>Alta del modelo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OperationResponse<ModeloAsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Guardar([FromBody] GuardarModeloAsientoRequest request)
        => Return(await _servicio.Guardar(request));

    /// <summary>Obtiene el modelo con sus cuentas, para verlo o precargar una clonacion.</summary>
    [HttpGet("{idModeloAsiento:int}")]
    [ProducesResponseType(typeof(OperationResponse<ModeloAsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerModelo(int idModeloAsiento)
        => Return(await _servicio.ObtenerModelo(idModeloAsiento));

    /// <summary>Modifica solo las cuentas.</summary>
    [HttpPut("{idModeloAsiento:int}")]
    [ProducesResponseType(typeof(OperationResponse<ModeloAsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(int idModeloAsiento, [FromBody] EditarModeloAsientoRequest request)
        => Return(await _servicio.Editar(idModeloAsiento, request));

    /// <summary>Historial del modelo.</summary>
    [HttpGet("{idModeloAsiento:int}/Historial")]
    [ProducesResponseType(typeof(OperationResponse<List<HistorialModeloResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Historial(int idModeloAsiento)
        => Return(await _servicio.Historial(idModeloAsiento));
}
