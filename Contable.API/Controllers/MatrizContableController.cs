using Contable.Application.RequestDto.MatrizContable;
using Contable.Application.ResponseDto.Dtos.MatrizContable;

namespace Contable.API.Controllers;

/// <summary>
/// Matriz contable: relaciona cada partida presupuestaria con la cuenta que recibe su movimiento.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class MatrizContableController(IMatrizContableService servicio) : BaseController
{
    private readonly IMatrizContableService _servicio = servicio;

    /// <summary>Grilla paginada. Sin ejercicio explicito usa el del token.</summary>
    [HttpPost("Buscar")]
    [ProducesResponseType(typeof(OperationResponse<List<MatrizContableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromBody] FiltroMatrizRequest filtro)
        => Return(await _servicio.Buscar(filtro));

    [HttpGet("{idMatrizCble:int}")]
    [ProducesResponseType(typeof(OperationResponse<MatrizContableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorId(int idMatrizCble)
        => Return(await _servicio.GetPorId(idMatrizCble));

    /// <summary>Relaciona una cuenta con un objeto de gasto o con un recurso, nunca con los dos.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OperationResponse<MatrizContableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] MatrizContableRequest request)
        => Return(await _servicio.Crear(request));

    [HttpPut("{idMatrizCble:int}")]
    [ProducesResponseType(typeof(OperationResponse<MatrizContableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(int idMatrizCble, [FromBody] MatrizContableRequest request)
        => Return(await _servicio.Editar(idMatrizCble, request));

    [HttpDelete("{idMatrizCble:int}")]
    [ProducesResponseType(typeof(OperationResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int idMatrizCble)
        => Return(await _servicio.Eliminar(idMatrizCble));

    /// <summary>Objetos de gasto para el selector de partidas.</summary>
    [HttpPost("ObjetosGasto")]
    [ProducesResponseType(typeof(OperationResponse<List<PartidaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarObjetosGasto([FromBody] FiltroPartidaRequest filtro)
        => Return(await _servicio.BuscarObjetosGasto(filtro));

    /// <summary>Recursos para el selector de partidas.</summary>
    [HttpPost("Recursos")]
    [ProducesResponseType(typeof(OperationResponse<List<PartidaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarRecursos([FromBody] FiltroPartidaRequest filtro)
        => Return(await _servicio.BuscarRecursos(filtro));

    /// <summary>
    /// Vista previa del traslado de la matriz desde el ejercicio anterior. No escribe nada:
    /// permite ver que se va a importar y que no se puede trasladar antes de confirmar.
    /// </summary>
    [HttpGet("PlanImportacion/{ejercicioDestino:int}")]
    [ProducesResponseType(typeof(OperationResponse<PlanImportacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PlanificarImportacion(int ejercicioDestino)
        => Return(await _servicio.PlanificarImportacion(ejercicioDestino));

    /// <summary>Ejecuta el traslado de la matriz desde el ejercicio anterior.</summary>
    [HttpPost("Importar/{ejercicioDestino:int}")]
    [ProducesResponseType(typeof(OperationResponse<PlanImportacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Importar(int ejercicioDestino)
        => Return(await _servicio.Importar(ejercicioDestino));
}
