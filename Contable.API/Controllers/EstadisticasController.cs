using Contable.Application.ResponseDto.Dtos.Estadisticas;

namespace Contable.API.Controllers;

/// <summary>
/// Estadisticas del modulo contable y las ayudas de carga que se apoyan en el historial.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class EstadisticasController(IEstadisticasService servicio) : BaseController
{
    private readonly IEstadisticasService _servicio = servicio;

    /// <summary>Indicadores de cabecera: volumen, cobertura del plan y hallazgos abiertos.</summary>
    [HttpGet("Tablero")]
    [ProducesResponseType(typeof(OperationResponse<TableroResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Tablero([FromQuery] int? ejercicio)
        => Return(await _servicio.Tablero(ejercicio));

    /// <summary>Asientos por mes y por tipo, contra el mismo corte del ejercicio anterior.</summary>
    [HttpGet("Actividad")]
    [ProducesResponseType(typeof(OperationResponse<ActividadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actividad([FromQuery] int? ejercicio)
        => Return(await _servicio.Actividad(ejercicio));

    /// <summary>Cuentas mas movidas, cuentas imputables sin uso y concentracion del movimiento.</summary>
    [HttpGet("Cuentas")]
    [ProducesResponseType(typeof(OperationResponse<CuentasResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cuentas([FromQuery] int? ejercicio)
        => Return(await _servicio.Cuentas(ejercicio));

    /// <summary>Desfase entre la fecha contable y la de carga, por usuario y por tramo.</summary>
    [HttpGet("Calidad")]
    [ProducesResponseType(typeof(OperationResponse<CalidadResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Calidad([FromQuery] int? ejercicio)
        => Return(await _servicio.Calidad(ejercicio));

    /// <summary>
    /// Cuentas propuestas para imputar un renglon. Primero la que indica la matriz contable,
    /// despues las que se usaron en asientos con leyenda parecida, y por ultimo las mas frecuentes.
    /// </summary>
    [HttpGet("SugerirCuentas")]
    [ProducesResponseType(typeof(OperationResponse<List<SugerenciaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SugerirCuentas(
        [FromQuery] string? leyenda,
        [FromQuery] int? idObjetoGasto,
        [FromQuery] int? idRecurso)
        => Return(await _servicio.SugerirCuentas(leyenda, idObjetoGasto, idRecurso));

    /// <summary>Avisa si el importe se aparta de lo habitual para esa cuenta.</summary>
    [HttpGet("RevisarImporte")]
    [ProducesResponseType(typeof(OperationResponse<AvisoDeImporteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevisarImporte([FromQuery] int idCtaContable, [FromQuery] decimal importe)
        => Return(await _servicio.RevisarImporte(idCtaContable, importe));
}
