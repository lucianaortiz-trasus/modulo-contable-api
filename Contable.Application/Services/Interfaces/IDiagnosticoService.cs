using Contable.Application.ResponseDto.Dtos.Diagnostico;

namespace Contable.Application.Services.Interfaces;

public interface IDiagnosticoService
{
    /// <summary>Verifica de punta a punta que el modulo llega a los datos contables reales.</summary>
    Task<OperationResponse<DiagnosticoResponse>> GetDiagnostico();
}
