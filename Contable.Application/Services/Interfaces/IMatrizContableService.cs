using Contable.Application.RequestDto.MatrizContable;
using Contable.Application.ResponseDto.Dtos.MatrizContable;

namespace Contable.Application.Services.Interfaces;

public interface IMatrizContableService
{
    Task<OperationResponse<List<MatrizContableResponse>>> Buscar(FiltroMatrizRequest filtro);

    Task<OperationResponse<MatrizContableResponse>> GetPorId(int idMatrizCble);

    Task<OperationResponse<MatrizContableResponse>> Crear(MatrizContableRequest request);

    Task<OperationResponse<MatrizContableResponse>> Editar(int idMatrizCble, MatrizContableRequest request);

    Task<OperationResponse<bool>> Eliminar(int idMatrizCble);

    Task<OperationResponse<List<PartidaResponse>>> BuscarObjetosGasto(FiltroPartidaRequest filtro);

    Task<OperationResponse<List<PartidaResponse>>> BuscarRecursos(FiltroPartidaRequest filtro);

    /// <summary>Vista previa de la importacion: no escribe nada.</summary>
    Task<OperationResponse<PlanImportacionResponse>> PlanificarImportacion(int ejercicioDestino);

    /// <summary>Ejecuta la importacion desde el ejercicio anterior.</summary>
    Task<OperationResponse<PlanImportacionResponse>> Importar(int ejercicioDestino);
}
