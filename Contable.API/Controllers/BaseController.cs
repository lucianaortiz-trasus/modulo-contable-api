namespace Contable.API.Controllers;

/// <summary>
/// Base de todos los controllers: traduce el Code de OperationResponse al IActionResult.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult Return<T>(OperationResponse<T> response) => response.Code switch
    {
        404 => NotFound(response),
        400 => BadRequest(response),
        401 => Unauthorized(response),
        500 => StatusCode(StatusCodes.Status500InternalServerError, response),
        _ => Ok(response),
    };
}
