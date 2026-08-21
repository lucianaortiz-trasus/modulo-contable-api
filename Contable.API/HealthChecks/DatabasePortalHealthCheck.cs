namespace Contable.API.HealthChecks;

/// <summary>Verifica que el esquema Portal responda.</summary>
public class DatabasePortalHealthCheck(PortalContext contexto) : IHealthCheck
{
    private readonly PortalContext _contexto = contexto;

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
