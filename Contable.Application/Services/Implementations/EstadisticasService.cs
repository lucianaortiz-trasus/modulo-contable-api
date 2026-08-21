using System.Globalization;
using Contable.Application.RequestDto.Reportes;
using Contable.Application.ResponseDto.Dtos.Estadisticas;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Tablero, estadísticas y asistencia durante la carga.
/// </summary>
/// <remarks>
/// Todo lo que hay acá es determinístico y explicable. En un módulo contable un indicador que no
/// se puede abrir hasta el asiento que lo origina no sirve: quien firma el balance necesita poder
/// justificar cada número, no confiar en una caja negra.
///
/// Por eso las sugerencias de imputación dicen de dónde salieron, y los avisos de importe atípico
/// dicen contra qué se comparó.
/// </remarks>
public class EstadisticasService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IEstadisticasService
{
    private const string Anulado = "S";

    public async Task<OperationResponse<TableroResponse>> Tablero(int? ejercicio)
    {
        var (anio, vigencia, jurisdicciones) = await Alcance(ejercicio);

        if (vigencia is null) return NotFound<TableroResponse>($"No existe el ejercicio {anio}.");

        var asientos = _ejecucionContext.TAsientos
            .AsNoTracking()
            .Where(a => a.IdVigencia == vigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value));

        var mesActual = DateTime.Today.Month;

        var totalAsientos = await asientos.CountAsync();

        var delMes = await asientos.CountAsync(a => a.FecContable.Month == mesActual);

        var monto = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in asientos on d.IdAsiento equals a.IdAsiento
            where d.Anulado == null || d.Anulado != Anulado
            select d.Debe ?? 0).SumAsync();

        var ultimo = await asientos
            .OrderByDescending(a => a.FecContable)
            .ThenByDescending(a => a.Numero)
            .Select(a => new { a.Numero, a.FecContable })
            .FirstOrDefaultAsync();

        var imputables = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .CountAsync(c => c.Imputable == "S" && c.Activa == "A" && jurisdicciones.Contains(c.IdJurisdiccion));

        var conMovimiento = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in asientos on d.IdAsiento equals a.IdAsiento
            select d.IdCtaContable).Distinct().CountAsync();

        var matriz = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .CountAsync(m => m.IdVigencia == vigencia);

        return Ok(new TableroResponse
        {
            Ejercicio = anio,
            AsientosDelEjercicio = totalAsientos,
            AsientosDelMes = delMes,
            MontoDelEjercicio = ReglasBalanceAsiento.Redondear(monto),
            UltimoNumero = ultimo?.Numero,
            UltimaFechaContable = ultimo?.FecContable,
            CuentasImputablesActivas = imputables,
            CuentasConMovimiento = conMovimiento,
            CoberturaDelPlan = imputables == 0
                ? 0
                : decimal.Round((decimal)conMovimiento * 100 / imputables, 1),
            RelacionesEnLaMatriz = matriz,
        });
    }

    public async Task<OperationResponse<ActividadResponse>> Actividad(int? ejercicio)
    {
        var (anio, vigencia, jurisdicciones) = await Alcance(ejercicio);

        if (vigencia is null) return NotFound<ActividadResponse>($"No existe el ejercicio {anio}.");

        var porMes = await ActividadPorMes(vigencia.Value, jurisdicciones);

        var vigenciaAnterior = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.Ejercicio == anio - 1)
            .Select(v => (int?)v.IdVigencia)
            .FirstOrDefaultAsync();

        // El ejercicio anterior puede estar en otra jurisdiccion: el id cambia de un año a otro.
        var anterior = vigenciaAnterior is null
            ? []
            : await ActividadPorMes(vigenciaAnterior.Value, jurisdicciones);

        var porTipo = await (
            from a in _ejecucionContext.TAsientos.AsNoTracking()
            join t in _ejecucionContext.TTiposAsiento.AsNoTracking() on a.IdTipoAsiento equals t.IdTipoAsiento
            where a.IdVigencia == vigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value)
            group a by new { t.IdTipoAsiento, t.Descripcion } into g
            select new ConteoConNombre
            {
                Id = g.Key.IdTipoAsiento,
                Nombre = g.Key.Descripcion ?? string.Empty,
                Cantidad = g.Count(),
            }).ToListAsync();

        var total = porMes.Sum(m => m.Monto);
        var totalAnterior = anterior.Sum(m => m.Monto);

        return Ok(new ActividadResponse
        {
            Ejercicio = anio,
            PorMes = porMes,
            PorMesEjercicioAnterior = anterior,
            PorTipo = [.. porTipo.OrderByDescending(t => t.Cantidad)],
            TotalAsientos = porMes.Sum(m => m.Asientos),
            TotalMonto = total,
            VariacionInteranual = totalAnterior == 0
                ? 0
                : decimal.Round((total - totalAnterior) * 100 / totalAnterior, 1),
        });
    }

    public async Task<OperationResponse<CuentasResponse>> Cuentas(int? ejercicio)
    {
        var (anio, vigencia, jurisdicciones) = await Alcance(ejercicio);

        if (vigencia is null) return NotFound<CuentasResponse>($"No existe el ejercicio {anio}.");

        var movidas = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            join c in _ejecucionContext.TCuentasContables.AsNoTracking() on d.IdCtaContable equals c.IdCtaContable
            where a.IdVigencia == vigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value)
                  && (d.Anulado == null || d.Anulado != Anulado)
            group new { d.Debe, d.Haber } by new { c.IdCtaContable, c.Codigo, c.Nombre } into g
            select new CuentaMovidaResponse
            {
                IdCtaContable = g.Key.IdCtaContable,
                Codigo = g.Key.Codigo,
                Nombre = g.Key.Nombre,
                Movimientos = g.Count(),
                Debe = g.Sum(x => x.Debe ?? 0),
                Haber = g.Sum(x => x.Haber ?? 0),
            }).ToListAsync();

        var ordenadas = movidas.OrderByDescending(c => c.Total).ToList();
        var totalMonto = ordenadas.Sum(c => c.Total);
        var conMovimiento = ordenadas.Select(c => c.IdCtaContable).ToHashSet();

        var sinMovimiento = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => c.Imputable == "S" && c.Activa == "A" && jurisdicciones.Contains(c.IdJurisdiccion))
            .Select(c => new CuentaMovidaResponse
            {
                IdCtaContable = c.IdCtaContable,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
            })
            .ToListAsync();

        sinMovimiento = [.. sinMovimiento
            .Where(c => !conMovimiento.Contains(c.IdCtaContable))
            .OrderBy(c => CodigoCuenta.Normalizar(c.Codigo), StringComparer.Ordinal)];

        var diezPrimeras = ordenadas.Take(10).Sum(c => c.Total);

        return Ok(new CuentasResponse
        {
            MasMovidas = [.. ordenadas.Take(25)],
            SinMovimiento = sinMovimiento,
            TotalImputables = sinMovimiento.Count + conMovimiento.Count,
            ConMovimiento = conMovimiento.Count,
            ConcentracionEnDiez = totalMonto == 0 ? 0 : decimal.Round(diezPrimeras * 100 / totalMonto, 1),
        });
    }

    public async Task<OperationResponse<CalidadResponse>> Calidad(int? ejercicio)
    {
        var (anio, vigencia, jurisdicciones) = await Alcance(ejercicio);

        if (vigencia is null) return NotFound<CalidadResponse>($"No existe el ejercicio {anio}.");

        var asientos = await _ejecucionContext.TAsientos
            .AsNoTracking()
            .Where(a => a.IdVigencia == vigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value))
            .Select(a => new { a.IdAsiento, a.FecContable, a.FecAlta, a.UserAlta, a.Leyenda })
            .ToListAsync();

        // Cuanto tarda un asiento en cargarse desde su fecha contable. Un desfase grande no es un
        // error, pero dice algo sobre como se trabaja.
        var conFecha = asientos.Where(a => a.FecAlta is not null).ToList();

        var tramos = new (string Nombre, int Desde, int Hasta)[]
        {
            ("Mismo dia", 0, 0),
            ("1 a 7 dias", 1, 7),
            ("8 a 30 dias", 8, 30),
            ("31 a 90 dias", 31, 90),
            ("Mas de 90 dias", 91, int.MaxValue),
        };

        var desfases = conFecha
            .Select(a => (int)(a.FecAlta!.Value.Date - a.FecContable.Date).TotalDays)
            .Where(d => d >= 0)
            .ToList();

        var porTramo = tramos
            .Select((t, i) => new ConteoConNombre
            {
                Id = i,
                Nombre = t.Nombre,
                Cantidad = desfases.Count(d => d >= t.Desde && d <= t.Hasta),
            })
            .ToList();

        var porUsuario = asientos
            .GroupBy(a => string.IsNullOrWhiteSpace(a.UserAlta) ? "(sin usuario)" : a.UserAlta!)
            .Select((g, i) => new ConteoConNombre { Id = i, Nombre = g.Key, Cantidad = g.Count() })
            .OrderByDescending(u => u.Cantidad)
            .ToList();

        var ids = asientos.Select(a => a.IdAsiento).ToList();

        var anulados = await (
            from a in _ejecucionContext.TAsientos.AsNoTracking()
            where ids.Contains(a.IdAsiento)
                  && _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento) > 0
                  && _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento && d.Anulado == Anulado) ==
                     _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento)
            select a.IdAsiento).CountAsync();

        return Ok(new CalidadResponse
        {
            DesfaseDeCarga = porTramo,
            DesfasePromedioEnDias = desfases.Count == 0
                ? 0
                : decimal.Round((decimal)desfases.Average(), 1),
            PorUsuario = porUsuario,
            // Una leyenda de menos de tres palabras utiles no dice de que se trata el asiento.
            AsientosSinLeyendaUtil = asientos.Count(a => ReglasSugerenciaDeImputacion.Palabras(a.Leyenda).Count < 2),
            AsientosAnulados = anulados,
            PorcentajeAnulado = asientos.Count == 0
                ? 0
                : decimal.Round((decimal)anulados * 100 / asientos.Count, 1),
        });
    }

    // ---- Asistencia durante la carga -------------------------------------

    public async Task<OperationResponse<List<SugerenciaResponse>>> SugerirCuentas(
        string? leyenda, int? idObjetoGasto, int? idRecurso)
    {
        var (_, vigencia, jurisdicciones) = await Alcance(null);

        if (vigencia is null) return Ok(new List<SugerenciaResponse>());

        int? cuentaDeLaMatriz = null;

        if (idObjetoGasto is > 0 || idRecurso is > 0)
        {
            cuentaDeLaMatriz = await _ejecucionContext.TMatrizContable
                .AsNoTracking()
                .Where(m => m.IdVigencia == vigencia)
                .Where(m => idObjetoGasto > 0 ? m.IdObjetoGasto == idObjetoGasto : m.IdRecurso == idRecurso)
                .Select(m => m.IdCtaContable)
                .FirstOrDefaultAsync();
        }

        // Antecedentes: que cuenta se uso con que leyenda. Se acota a lo reciente porque el
        // criterio de imputacion cambia con los años.
        var antecedentes = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where jurisdicciones.Contains(a.IdJurisdiccion!.Value) && a.Leyenda != null
            group a by new { d.IdCtaContable, a.Leyenda } into g
            select new AntecedenteDeImputacion(g.Key.IdCtaContable, g.Key.Leyenda, g.Count()))
            .Take(3000)
            .ToListAsync();

        var frecuentes = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where a.IdVigencia == vigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value)
            group d by d.IdCtaContable into g
            orderby g.Count() descending
            select g.Key).Take(10).ToListAsync();

        var sugerencias = ReglasSugerenciaDeImputacion.Sugerir(
            cuentaDeLaMatriz, antecedentes, leyenda, frecuentes);

        var ids = sugerencias.Select(s => s.IdCtaContable).ToList();

        var cuentas = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => ids.Contains(c.IdCtaContable))
            .ToDictionaryAsync(c => c.IdCtaContable);

        return Ok(sugerencias
            .Where(s => cuentas.ContainsKey(s.IdCtaContable))
            .Select(s => new SugerenciaResponse
            {
                IdCtaContable = s.IdCtaContable,
                Codigo = cuentas[s.IdCtaContable].Codigo,
                Nombre = cuentas[s.IdCtaContable].Nombre,
                Puntaje = s.Puntaje,
                Origen = s.Origen.ToString(),
                Motivo = s.Motivo,
            })
            .ToList());
    }

    public async Task<OperationResponse<AvisoDeImporteResponse>> RevisarImporte(int idCtaContable, decimal importe)
    {
        var (_, _, jurisdicciones) = await Alcance(null);

        var historial = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where d.IdCtaContable == idCtaContable
                  && jurisdicciones.Contains(a.IdJurisdiccion!.Value)
                  && (d.Anulado == null || d.Anulado != Anulado)
            select (d.Debe ?? 0) + (d.Haber ?? 0)).ToListAsync();

        var atipicidad = ReglasImportesAtipicos.Evaluar(importe, historial);

        return Ok(new AvisoDeImporteResponse
        {
            EsAtipico = atipicidad.EsAtipico,
            Grado = atipicidad.Grado.ToString(),
            ImporteHabitual = ReglasBalanceAsiento.Redondear(atipicidad.Mediana),
            Veces = atipicidad.Veces,
            Motivo = atipicidad.Motivo,
        });
    }

    // ---- Auxiliares ------------------------------------------------------

    private async Task<List<ActividadDelMes>> ActividadPorMes(int idVigencia, List<int> jurisdicciones)
    {
        var datos = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where a.IdVigencia == idVigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value)
                  && (d.Anulado == null || d.Anulado != Anulado)
            group new { d.Debe, a.IdAsiento } by a.FecContable.Month into g
            select new
            {
                Mes = g.Key,
                Monto = g.Sum(x => x.Debe ?? 0),
                Asientos = g.Select(x => x.IdAsiento).Distinct().Count(),
            }).ToListAsync();

        var cultura = new CultureInfo("es-AR");

        return
        [
            .. Enumerable.Range(1, 12).Select(mes =>
            {
                var fila = datos.FirstOrDefault(d => d.Mes == mes);

                return new ActividadDelMes
                {
                    Mes = mes,
                    NombreMes = cultura.DateTimeFormat.GetAbbreviatedMonthName(mes),
                    Asientos = fila?.Asientos ?? 0,
                    Monto = ReglasBalanceAsiento.Redondear(fila?.Monto ?? 0),
                };
            })
        ];
    }

    /// <summary>
    /// Ejercicio, vigencia y jurisdicciones equivalentes. Mismo criterio que los reportes: el
    /// alcance son todas las jurisdicciones que comparten numero con la del usuario.
    /// </summary>
    private async Task<(int Anio, int? Vigencia, List<int> Jurisdicciones)> Alcance(int? ejercicio)
    {
        var anio = ejercicio ?? DateTime.Today.Year;

        var vigencia = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.Ejercicio == anio)
            .Select(v => (int?)v.IdVigencia)
            .FirstOrDefaultAsync();

        var idJurisdiccion = GetJurisdiccionCache();

        var numero = await _presupuestoContext.TJurisdicciones
            .AsNoTracking()
            .Where(j => j.IdJurisdiccion == idJurisdiccion)
            .Select(j => j.NumeroJurisdiccion)
            .FirstOrDefaultAsync();

        var jurisdicciones = numero is null
            ? await _presupuestoContext.TJurisdicciones.AsNoTracking().Select(j => j.IdJurisdiccion).ToListAsync()
            : await _presupuestoContext.TJurisdicciones.AsNoTracking()
                .Where(j => j.NumeroJurisdiccion == numero).Select(j => j.IdJurisdiccion).ToListAsync();

        return (anio, vigencia, jurisdicciones);
    }
}
