namespace Contable.API.HealthChecks;

/// <summary>Verifica que el esquema Presupuesto responda.</summary>
public class DatabasePresupuestoHealthCheck(PresupuestoContext contexto) : IHealthCheck
{
    private readonly PresupuestoContext _contexto = contexto;

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
