namespace Contable.API.HealthChecks;

/// <summary>Verifica que el esquema Ejecucion responda.</summary>
public class DatabaseEjecucionHealthCheck(EjecucionContext contexto) : IHealthCheck
{
    private readonly EjecucionContext _contexto = contexto;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _contexto.Database.OpenConnectionAsync(cancellationToken);
            await _contexto.Database.CloseConnectionAsync();

            return HealthCheckResult.Healthy("PONG");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
