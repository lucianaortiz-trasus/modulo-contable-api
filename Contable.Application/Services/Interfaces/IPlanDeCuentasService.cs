using Contable.Application.RequestDto.PlanDeCuentas;
using Contable.Application.ResponseDto.Dtos.PlanDeCuentas;

namespace Contable.Application.Services.Interfaces;

public interface IPlanDeCuentasService
{
    /// <summary>Arbol completo del plan, con las anomalias estructurales detectadas y soporte de filtros.</summary>
    Task<OperationResponse<ArbolPlanDeCuentasResponse>> GetArbol(
        bool incluirInactivas,
        string? codigo = null,
        string? nombre = null,
        string? tipoSaldo = null,
        string? estado = null);

    /// <summary>Busqueda paginada para la grilla.</summary>
    Task<OperationResponse<List<CuentaContableResponse>>> Buscar(FiltroCuentasRequest filtro);

    Task<OperationResponse<CuentaContableResponse>> GetPorId(int idCtaContable);

    /// <summary>Proximo codigo libre bajo el padre indicado. Sin padre, calcula una raiz.</summary>
    Task<OperationResponse<ProximoCodigoResponse>> GetProximoCodigo(int? idCtaContableRel);

    Task<OperationResponse<CuentaContableResponse>> Crear(CuentaContableRequest request);

    Task<OperationResponse<CuentaContableResponse>> Editar(int idCtaContable, CuentaContableRequest request);

    Task<OperationResponse<bool>> Eliminar(int idCtaContable);

    /// <summary>
    /// Carga masiva desde una planilla. Con `Confirmar` en false devuelve que pasaria; en true
    /// crea las cuentas, todas en una sola transaccion.
    /// </summary>
    Task<OperationResponse<ImportacionPlanResponse>> Importar(ImportacionPlanRequest request);
}
