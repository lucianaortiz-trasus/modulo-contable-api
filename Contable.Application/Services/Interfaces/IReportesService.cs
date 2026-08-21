using Contable.Application.RequestDto.Reportes;
using Contable.Application.ResponseDto.Dtos.Reportes;

namespace Contable.Application.Services.Interfaces;

public interface IReportesService
{
    /// <summary>Balance de sumas y saldos: sumas del periodo, del acumulado anterior y saldo.</summary>
    Task<OperationResponse<SumasYSaldosResponse>> SumasYSaldos(FiltroReporteRequest filtro);

    /// <summary>Libro diario: los asientos del periodo en orden cronologico, con su detalle.</summary>
    Task<OperationResponse<LibroDiarioResponse>> LibroDiario(FiltroReporteRequest filtro);

    /// <summary>Mayor de una cuenta, o de todas las imputables si no se indica ninguna.</summary>
    Task<OperationResponse<MayoresResponse>> Mayores(FiltroMayoresRequest filtro);
}
