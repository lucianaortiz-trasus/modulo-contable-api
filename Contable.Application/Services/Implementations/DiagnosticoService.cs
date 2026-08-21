using Contable.Application.ResponseDto.Dtos.Diagnostico;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Prueba de humo del esqueleto: recorre los tres contextos y devuelve el estado real de los
/// datos contables. Si responde, el armado esta completo de punta a punta.
/// </summary>
public class DiagnosticoService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IDiagnosticoService
{
    public async Task<OperationResponse<DiagnosticoResponse>> GetDiagnostico()
    {
        var vigencia = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ActivoEjecucion == "S");

        var porNivel = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .GroupBy(x => x.Nivel)
            .Select(g => new CuentasPorNivel { Nivel = g.Key, Cantidad = g.Count() })
            .OrderBy(x => x.Nivel)
            .ToListAsync();

        var tipos = await _ejecucionContext.TTiposAsiento
            .AsNoTracking()
            .OrderBy(x => x.IdTipoAsiento)
            .Select(x => new TipoAsientoResumen
            {
                Id = x.IdTipoAsiento,
                Codigo = x.Codigo,
                Descripcion = x.Descripcion,
            })
            .ToListAsync();

        var diagnostico = new DiagnosticoResponse
        {
            Entorno = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            EjercicioActivo = vigencia?.Ejercicio,
            IdVigenciaActiva = vigencia?.IdVigencia,
            CuentasContables = await _ejecucionContext.TCuentasContables.CountAsync(),
            CuentasImputables = await _ejecucionContext.TCuentasContables.CountAsync(x => x.Imputable == "S"),
            PlanDeCuentasPorNivel = porNivel,
            Asientos = await _ejecucionContext.TAsientos.CountAsync(),
            AsientosDetalle = await _ejecucionContext.TAsientosDetalle.CountAsync(),
            FilasMatrizContable = await _ejecucionContext.TMatrizContable.CountAsync(),
            TiposDeAsiento = tipos,
        };

        return Ok(diagnostico);
    }
}
