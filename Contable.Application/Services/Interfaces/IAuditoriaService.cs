using Contable.Application.ResponseDto.Dtos.Auditoria;

namespace Contable.Application.Services.Interfaces;

public interface IAuditoriaService
{
    /// <summary>Corre todos los controles sobre el ejercicio y devuelve los hallazgos agrupados.</summary>
    Task<OperationResponse<AuditoriaResponse>> Auditar(int? ejercicio);
}
