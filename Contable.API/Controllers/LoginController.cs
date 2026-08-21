namespace Contable.API.Controllers;

/// <summary>
/// Puente con el portal SIGEMyT. Mismo contrato que el modulo de Tesoreria.
/// </summary>
[ApiController]
public class LoginController(IJwtService jwtService) : BaseController
{
    private readonly IJwtService _jwtService = jwtService;

    /// <summary>Emite el token del modulo para un usuario ya autenticado por el portal.</summary>
    /// <param name="idUsuario">Id del usuario del portal.</param>
    [HttpPost("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OperationResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearTokenAdmin([FromQuery, Required] int idUsuario)
        => Return(await _jwtService.CrearTokenAdmin(idUsuario));

    /// <summary>
    /// Devuelve los datos del usuario del token con el que vino el pedido.
    /// </summary>
    /// <remarks>
    /// El token va en el header <c>Authorization</c>, no por querystring: es una credencial y en la
    /// URL terminaba en los logs del servidor y en el historial del navegador.
    /// </remarks>
    [HttpGet("[action]")]
    [ProducesResponseType(typeof(OperationResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetDataToken() => Return(_jwtService.GetDataToken());
}
