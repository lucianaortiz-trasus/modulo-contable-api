using Contable.Application.RequestDto.Asientos;
using Contable.Application.ResponseDto.Dtos.Asientos;

namespace Contable.Application.Services.Interfaces;

public interface IAsientoService
{
    Task<OperationResponse<List<AsientoResumenResponse>>> Buscar(FiltroAsientosRequest filtro);

    /// <summary>Catalogo de tipos de asiento, para los selectores.</summary>
    Task<OperationResponse<List<TipoAsientoResponse>>> GetTipos();

    Task<OperationResponse<AsientoResponse>> GetPorId(int idAsiento);

    /// <summary>Alta de un asiento manual: cabecera y renglones en una sola transaccion.</summary>
    Task<OperationResponse<AsientoResponse>> Crear(AsientoRequest request);

    /// <summary>Contraasiento con la fecha recalculada segun el ejercicio.</summary>
    Task<OperationResponse<AsientoResponse>> Reversar(int idAsiento);

    /// <summary>Contraasiento con la fecha contable del original.</summary>
    Task<OperationResponse<AsientoResponse>> Anular(int idAsiento);

    Task<OperationResponse<PlanAsientosResponse>> PlanificarCierre(int ejercicio);

    Task<OperationResponse<PlanAsientosResponse>> GenerarCierre(int ejercicio);

    Task<OperationResponse<PlanAsientosResponse>> PlanificarApertura(int ejercicio);

    Task<OperationResponse<PlanAsientosResponse>> GenerarApertura(int ejercicio);
}
