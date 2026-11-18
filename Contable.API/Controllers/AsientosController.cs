using Contable.Application.RequestDto.Asientos;
using Contable.Application.ResponseDto.Dtos.Asientos;

namespace Contable.API.Controllers;

/// <summary>
/// Asientos contables: consulta, alta manual, reversa, anulacion, cierre y apertura de ejercicio.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AsientosController(IAsientoService servicio) : BaseController
{
    private readonly IAsientoService _servicio = servicio;

    /// <summary>Grilla paginada vía GET. Sin ejercicio ni jurisdiccion explicitos usa los del token.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(OperationResponse<List<AsientoResumenResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarGet([FromQuery] FiltroAsientosRequest filtro)
        => Return(await _servicio.Buscar(filtro));

    /// <summary>Grilla paginada. Sin ejercicio ni jurisdiccion explicitos usa los del token.</summary>
    [HttpPost("Buscar")]
    [ProducesResponseType(typeof(OperationResponse<List<AsientoResumenResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromBody] FiltroAsientosRequest filtro)
        => Return(await _servicio.Buscar(filtro));

    /// <summary>Catalogo de tipos de asiento.</summary>
    [HttpGet("Tipos")]
    [ProducesResponseType(typeof(OperationResponse<List<TipoAsientoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos() => Return(await _servicio.GetTipos());

    /// <summary>Asiento completo con sus renglones.</summary>
    [HttpGet("{idAsiento:int}")]
    [ProducesResponseType(typeof(OperationResponse<AsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorId(int idAsiento)
        => Return(await _servicio.GetPorId(idAsiento));

    /// <summary>
    /// Alta de un asiento manual. Cabecera y renglones entran en una sola transaccion, y el
    /// asiento tiene que cuadrar antes de grabarse.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OperationResponse<AsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] AsientoRequest request)
        => Return(await _servicio.Crear(request));

    /// <summary>
    /// Reversa: genera el contraasiento con la fecha recalculada segun el ejercicio y marca los
    /// dos como anulados. No borra nada.
    /// </summary>
    [HttpPost("{idAsiento:int}/Reversar")]
    [ProducesResponseType(typeof(OperationResponse<AsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reversar(int idAsiento)
        => Return(await _servicio.Reversar(idAsiento));

    /// <summary>
    /// Anulacion: genera el contraasiento conservando la fecha contable del original, de modo que
    /// el par se cancela dentro del mismo periodo.
    /// </summary>
    [HttpPost("{idAsiento:int}/Anular")]
    [ProducesResponseType(typeof(OperationResponse<AsientoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Anular(int idAsiento)
        => Return(await _servicio.Anular(idAsiento));

    /// <summary>Vista previa del cierre de ejercicio. No escribe nada.</summary>
    [HttpGet("PlanCierre/{ejercicio:int}")]
    [ProducesResponseType(typeof(OperationResponse<PlanAsientosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PlanificarCierre(int ejercicio)
        => Return(await _servicio.PlanificarCierre(ejercicio));

    /// <summary>Genera los dos asientos de cierre del ejercicio.</summary>
    [HttpPost("Cierre/{ejercicio:int}")]
    [ProducesResponseType(typeof(OperationResponse<PlanAsientosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerarCierre(int ejercicio)
        => Return(await _servicio.GenerarCierre(ejercicio));

    /// <summary>Vista previa de la apertura de ejercicio. No escribe nada.</summary>
    [HttpGet("PlanApertura/{ejercicio:int}")]
    [ProducesResponseType(typeof(OperationResponse<PlanAsientosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PlanificarApertura(int ejercicio)
        => Return(await _servicio.PlanificarApertura(ejercicio));

    /// <summary>Genera el asiento de apertura desde el cierre del ejercicio anterior.</summary>
    [HttpPost("Apertura/{ejercicio:int}")]
    [ProducesResponseType(typeof(OperationResponse<PlanAsientosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerarApertura(int ejercicio)
        => Return(await _servicio.GenerarApertura(ejercicio));
}
