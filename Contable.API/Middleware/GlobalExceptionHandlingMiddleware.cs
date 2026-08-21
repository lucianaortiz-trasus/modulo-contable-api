using System.Text.Json;

namespace Contable.API.Middleware;

/// <summary>
/// Captura lo que se escape de los servicios y lo devuelve con el mismo contrato que el resto
/// de la API, en vez de una pagina de error.
/// </summary>
public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;

    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // El detalle de la excepcion solo viaja fuera de produccion.
            var esDesarrollo = context.RequestServices
                .GetRequiredService<IHostEnvironment>()
                .IsDevelopment();

            var respuesta = OperationResponse<object>.ErrorResponse(
                "Ocurrio un error al procesar la solicitud.",
                esDesarrollo ? ex.Message : null,
                esDesarrollo ? ex.StackTrace : null);

            await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta));
        }
    }
}
