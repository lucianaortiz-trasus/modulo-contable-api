namespace Contable.API.Controllers;

/// <summary>
/// Prueba de humo del modulo. Es el endpoint que se usa para confirmar que el armado llega a los
/// datos contables reales sin depender del front.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DiagnosticoController(IDiagnosticoService diagnosticoService) : BaseController
{
    private readonly IDiagnosticoService _diagnosticoService = diagnosticoService;

    /// <summary>Estado de los datos contables: plan de cuentas, asientos, matriz y vigencia activa.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OperationResponse<DiagnosticoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get() => Return(await _diagnosticoService.GetDiagnostico());
}
