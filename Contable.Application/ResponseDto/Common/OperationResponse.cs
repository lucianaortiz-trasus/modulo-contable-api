namespace Contable.Application.ResponseDto.Common;

/// <summary>
/// Contrato unico de salida de la API. Todo endpoint devuelve una instancia de esta clase.
/// </summary>
/// <typeparam name="T">Tipo del dato devuelto.</typeparam>
public class OperationResponse<T>
{
    public bool? Success { get; private init; }

    public string? Message { get; private init; }

    public T? Data { get; private init; }

    /// <summary>404, 400 o 200. BaseController.Return lo traduce al IActionResult.</summary>
    public int? Code { get; private init; }

    /// <summary>Total de filas de la consulta sin paginar.</summary>
    public int? TotalRows { get; private init; }

    public string? Exception { get; private init; }

    public object? ExceptionDetails { get; private init; }

    /// <summary>Respuesta correcta con un dato.</summary>
    public static OperationResponse<T> SuccessResponse(T data, int totalRows = default) => new()
    {
        Success = true,
        Code = 200,
        Data = data,
        TotalRows = totalRows,
    };

    /// <summary>Respuesta correcta de una consulta paginada.</summary>
    public static OperationResponse<T> SuccessResponseMassive(T data, int totalRows) => new()
    {
        Success = true,
        Code = 200,
        Data = data,
        TotalRows = totalRows,
    };

    /// <summary>El recurso pedido no existe.</summary>
    public static OperationResponse<T> NotFoundResponse(string message = "No se encontraron resultados.") => new()
    {
        Success = false,
        Code = 404,
        Message = message,
    };

    /// <summary>Error no controlado.</summary>
    public static OperationResponse<T> ErrorResponse(string message, string? exception = null, object? details = null) => new()
    {
        Success = false,
        Code = 500,
        Message = message,
        Exception = exception,
        ExceptionDetails = details,
    };

    /// <summary>Error de negocio o de validacion, con el codigo que corresponda.</summary>
    public static OperationResponse<T> CustomErrorResponse(int code, string message, T? data = default) => new()
    {
        Success = false,
        Code = code,
        Message = message,
        Data = data,
    };
}
