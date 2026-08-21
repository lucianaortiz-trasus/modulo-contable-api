using Contable.Application.RequestDto.PlanDeCuentas;
using Contable.Application.ResponseDto.Dtos.PlanDeCuentas;

namespace Contable.API.Controllers;

/// <summary>
/// Plan de cuentas: arbol jerarquico, grilla y ABM.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PlanDeCuentasController(IPlanDeCuentasService servicio) : BaseController
{
    private readonly IPlanDeCuentasService _servicio = servicio;

    /// <summary>
    /// Arbol completo del plan, con el detalle de las cuentas que tienen problemas estructurales.
    /// </summary>
    /// <param name="incluirInactivas">Si se muestran tambien las cuentas dadas de baja.</param>
    [HttpGet("Arbol")]
    [ProducesResponseType(typeof(OperationResponse<ArbolPlanDeCuentasResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArbol([FromQuery] bool incluirInactivas = false)
        => Return(await _servicio.GetArbol(incluirInactivas));

    /// <summary>Busqueda paginada para la grilla.</summary>
    [HttpPost("Buscar")]
    [ProducesResponseType(typeof(OperationResponse<List<CuentaContableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromBody] FiltroCuentasRequest filtro)
        => Return(await _servicio.Buscar(filtro));

    /// <summary>Devuelve una cuenta con su cantidad de hijas y si tiene movimientos.</summary>
    [HttpGet("{idCtaContable:int}")]
    [ProducesResponseType(typeof(OperationResponse<CuentaContableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPorId(int idCtaContable)
        => Return(await _servicio.GetPorId(idCtaContable));

    /// <summary>
    /// Proximo codigo libre bajo la cuenta indicada. Sin parametro calcula el de una cuenta raiz.
    /// </summary>
    [HttpGet("ProximoCodigo")]
    [ProducesResponseType(typeof(OperationResponse<ProximoCodigoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProximoCodigo([FromQuery] int? idCtaContableRel)
        => Return(await _servicio.GetProximoCodigo(idCtaContableRel));

    /// <summary>Da de alta una cuenta. Sin codigo explicito toma el proximo libre bajo el padre.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OperationResponse<CuentaContableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] CuentaContableRequest request)
        => Return(await _servicio.Crear(request));

    /// <summary>
    /// Modifica una cuenta. El codigo y la cuenta padre no se editan: mover una cuenta arrastra a
    /// toda su rama y a los asientos ya imputados.
    /// </summary>
    [HttpPut("{idCtaContable:int}")]
    [ProducesResponseType(typeof(OperationResponse<CuentaContableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(int idCtaContable, [FromBody] CuentaContableRequest request)
        => Return(await _servicio.Editar(idCtaContable, request));

    /// <summary>Elimina una cuenta hoja que no tenga asientos imputados.</summary>
    [HttpDelete("{idCtaContable:int}")]
    [ProducesResponseType(typeof(OperationResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int idCtaContable)
        => Return(await _servicio.Eliminar(idCtaContable));

    /// <summary>
    /// Carga masiva del plan desde una planilla.
    /// </summary>
    /// <remarks>
    /// Con <c>confirmar</c> en <c>false</c> no escribe nada: evalua el archivo contra el plan
    /// actual y devuelve que se crearia, que ya existe y que se rechaza, con el motivo de cada
    /// rechazo. Con <c>true</c> crea las cuentas, todas en una sola transaccion.
    /// </remarks>
    [HttpPost("Importar")]
    [ProducesResponseType(typeof(OperationResponse<ImportacionPlanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Importar([FromBody] ImportacionPlanRequest request)
        => Return(await _servicio.Importar(request));
}
