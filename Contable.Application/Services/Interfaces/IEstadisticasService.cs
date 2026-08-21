using Contable.Application.ResponseDto.Dtos.Estadisticas;

namespace Contable.Application.Services.Interfaces;

public interface IEstadisticasService
{
    /// <summary>Indicadores de cabecera del modulo.</summary>
    Task<OperationResponse<TableroResponse>> Tablero(int? ejercicio);

    /// <summary>Asientos por mes y por tipo, con comparacion interanual.</summary>
    Task<OperationResponse<ActividadResponse>> Actividad(int? ejercicio);

    /// <summary>Cuentas mas movidas, cuentas sin usar y concentracion del movimiento.</summary>
    Task<OperationResponse<CuentasResponse>> Cuentas(int? ejercicio);

    /// <summary>Desfase de carga, distribucion por usuario y calidad de las leyendas.</summary>
    Task<OperationResponse<CalidadResponse>> Calidad(int? ejercicio);

    /// <summary>Cuentas propuestas para imputar, cada una con el motivo.</summary>
    Task<OperationResponse<List<SugerenciaResponse>>> SugerirCuentas(string? leyenda, int? idObjetoGasto, int? idRecurso);

    /// <summary>Avisa si el importe se aparta de lo habitual para esa cuenta.</summary>
    Task<OperationResponse<AvisoDeImporteResponse>> RevisarImporte(int idCtaContable, decimal importe);
}
