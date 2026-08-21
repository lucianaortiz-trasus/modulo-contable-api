using Contable.Application.RequestDto.Reportes;
using Contable.Application.ResponseDto.Dtos.Reportes;

namespace Contable.API.Controllers;

/// <summary>
/// Reportes contables: libro diario, mayores y sumas y saldos.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ReportesController(IReportesService servicio) : BaseController
{
    private readonly IReportesService _servicio = servicio;

    /// <summary>
    /// Balance de sumas y saldos. Una fila por cuenta, con las sumas del periodo, las anteriores
    /// y el saldo resultante, consolidando cada nivel con el total de su rama.
    /// </summary>
    [HttpPost("SumasYSaldos")]
    [ProducesResponseType(typeof(OperationResponse<SumasYSaldosResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SumasYSaldos([FromBody] FiltroReporteRequest filtro)
        => Return(await _servicio.SumasYSaldos(filtro));

    /// <summary>Libro diario: los asientos del periodo en orden cronologico, con su detalle.</summary>
    [HttpPost("LibroDiario")]
    [ProducesResponseType(typeof(OperationResponse<LibroDiarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LibroDiario([FromBody] FiltroReporteRequest filtro)
        => Return(await _servicio.LibroDiario(filtro));

    /// <summary>Mayor de una cuenta, o de todas las imputables si no se indica ninguna.</summary>
    [HttpPost("Mayores")]
    [ProducesResponseType(typeof(OperationResponse<MayoresResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mayores([FromBody] FiltroMayoresRequest filtro)
        => Return(await _servicio.Mayores(filtro));
}
