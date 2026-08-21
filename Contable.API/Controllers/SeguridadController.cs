namespace Contable.API.Controllers;

/// <summary>
/// Federacion de la sesion con el Portal SIGEMyT.
/// </summary>
/// <remarks>
/// La ruta reproduce la que usan los otros modulos de la plataforma —<c>api/Seguridad/Login</c>—
/// para que el contrato sea uno solo en todos.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class SeguridadController(ISeguridadService servicio) : BaseController
{
    private readonly ISeguridadService _servicio = servicio;

    /// <summary>
    /// Canjea el token con el que el Portal mando al usuario por el token propio del modulo.
    /// </summary>
    /// <param name="atoken">
    /// El <c>token</c> del querystring con el que llego el usuario. Es opaco: este modulo no lo
    /// desencripta, lo reenvia al Portal para que diga quien es.
    /// </param>
    /// <param name="idJur">Jurisdiccion con la que el Portal mando al usuario.</param>
    [HttpGet("Login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OperationResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromQuery] string? atoken, [FromQuery] int? idJur)
        => Return(await _servicio.LoginFederado(atoken, idJur));
}
