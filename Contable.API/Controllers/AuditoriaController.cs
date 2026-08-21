using Contable.Application.ResponseDto.Dtos.Auditoria;

namespace Contable.API.Controllers;

/// <summary>
/// Controles de consistencia sobre los datos contables del ejercicio.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuditoriaController(IAuditoriaService servicio) : BaseController
{
    private readonly IAuditoriaService _servicio = servicio;

    /// <summary>
    /// Corre los diez controles y devuelve los hallazgos agrupados por tipo, cada grupo con su
    /// severidad y la explicacion de que conviene hacer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OperationResponse<AuditoriaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Auditar([FromQuery] int? ejercicio)
        => Return(await _servicio.Auditar(ejercicio));
}
