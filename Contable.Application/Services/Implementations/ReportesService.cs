using Contable.Application.RequestDto.Reportes;
using Contable.Application.ResponseDto.Dtos.Reportes;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Los tres reportes contables: libro diario, mayores y sumas y saldos.
/// </summary>
/// <remarks>
/// Portados de `PKG_REPORTES_CONTABILIDAD`, que en Morteros tiene exactamente estos tres
/// procedimientos y ninguno mas. La estructura de cada reporte se conserva; lo que cambia son
/// tres defectos que daban numeros equivocados, todos verificados contra el stage y anotados en
/// _analisis/REGLAS.md:
///
/// - <b>D13</b>: el original cruzaba `t_cuentas_contables` con `t_unidades_administrativa` por
///   jurisdiccion sin filtrar vigencia. Una jurisdiccion con dos unidades administrativas
///   duplicaba todos sus importes. En el stage pasa: las cuentas de la jurisdiccion 4000 salian
///   al doble exacto.
/// - <b>D14</b>: los renglones anulados sumaban, porque el filtro estaba comentado. Aca se
///   excluyen salvo que la configuracion diga lo contrario.
/// - <b>D15</b>: el parametro de unidad administrativa se recibia y no se usaba, solo servia para
///   el rotulo del encabezado. Aca el alcance se resuelve de verdad, por numero de jurisdiccion,
///   que es lo estable entre ejercicios.
///
/// A eso se suma la consolidacion por prefijo de codigo, que dejaba en cero las cinco cuentas de
/// nivel 1. Ver ReglasSumasYSaldos.
/// </remarks>
public class ReportesService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IReportesService
{
    private const string Anulado = "S";

    public async Task<OperationResponse<SumasYSaldosResponse>> SumasYSaldos(FiltroReporteRequest filtro)
    {
        var contexto = await ResolverContexto(filtro);

        if (contexto.Error is not null) return BadRequest<SumasYSaldosResponse>(contexto.Error);

        var cuentas = await CuentasDelAlcance(contexto.Jurisdicciones);

        var periodo = await MovimientosPorCuenta(contexto.Jurisdicciones, contexto.Desde, contexto.Hasta);

        // El acumulado es todo lo anterior al periodo: no se corta en el 1 de enero, porque un
        // balance a mitad de ejercicio tiene que arrastrar lo que paso antes.
        var acumulado = await MovimientosPorCuenta(contexto.Jurisdicciones, null, contexto.Desde.AddDays(-1));

        // Las cuentas de resultado no arrastran saldo anterior: se cancelan en el cierre y cada
        // ejercicio arranca de cero. Solo las patrimoniales traen saldo de apertura.
        //
        // El PL/SQL original hace lo mismo, pero identificando las cuentas de resultado por el
        // prefijo del codigo (`CODIGO NOT LIKE '04%' AND CODIGO NOT LIKE '05%'`), que es el plan
        // de la instalacion provincial. Aca se usa la naturaleza, que es lo que la configuracion
        // ya define por municipio.
        var naturalezasDeResultado = AppSettings.Contabilidad.NaturalezasResultado;

        var movimientos = cuentas
            .Select(c =>
            {
                var delPeriodo = periodo.GetValueOrDefault(c.IdCtaContable);

                var esDeResultado = c.IdNaturaleza is not null
                                    && naturalezasDeResultado.Contains(c.IdNaturaleza.Value);

                var anterior = esDeResultado
                    ? (Debe: 0m, Haber: 0m)
                    : acumulado.GetValueOrDefault(c.IdCtaContable);

                return new MovimientoDeCuenta(
                    c.IdCtaContable, delPeriodo.Debe, delPeriodo.Haber, anterior.Debe, anterior.Haber);
            })
            .ToList();

        var filas = ReglasSumasYSaldos.Armar(
            cuentas, movimientos, filtro.NivelMaximo, filtro.SoloConMovimiento);

        var (deudor, acreedor, cuadra) = ReglasSumasYSaldos.Totales(filas);

        return Ok(new SumasYSaldosResponse
        {
            Encabezado = contexto.Encabezado,
            NivelMaximo = filtro.NivelMaximo,
            TotalSaldoDeudor = deudor,
            TotalSaldoAcreedor = acreedor,
            Cuadra = cuadra,
            MotivoDescuadre = cuadra
                ? null
                : await ExplicarDescuadre(contexto, deudor - acreedor),
            Filas =
            [
                .. filas.Select(f => new FilaSumasYSaldosResponse
                {
                    IdCtaContable = f.IdCtaContable,
                    Codigo = f.Codigo,
                    Nombre = f.Nombre,
                    Nivel = f.Nivel,
                    Profundidad = f.Profundidad,
                    Imputable = f.Imputable,
                    DebeAcumulado = f.DebeAcumulado,
                    HaberAcumulado = f.HaberAcumulado,
                    DebePeriodo = f.DebePeriodo,
                    HaberPeriodo = f.HaberPeriodo,
                    DebeTotal = f.DebeTotal,
                    HaberTotal = f.HaberTotal,
                    SaldoDeudor = f.SaldoDeudor,
                    SaldoAcreedor = f.SaldoAcreedor,
                })
            ],
        });
    }

    public async Task<OperationResponse<LibroDiarioResponse>> LibroDiario(FiltroReporteRequest filtro)
    {
        var contexto = await ResolverContexto(filtro);

        if (contexto.Error is not null) return BadRequest<LibroDiarioResponse>(contexto.Error);

        var asientos = await _ejecucionContext.TAsientos
            .AsNoTracking()
            .Where(a => contexto.Jurisdicciones.Contains(a.IdJurisdiccion!.Value))
            .Where(a => a.FecContable >= contexto.Desde && a.FecContable <= contexto.Hasta)
            .OrderBy(a => a.FecContable)
            .ThenBy(a => a.Numero)
            .Select(a => new
            {
                a.IdAsiento,
                a.Numero,
                a.FecContable,
                a.Leyenda,
                a.IdTipoAsiento,
            })
            .ToListAsync();

        if (asientos.Count == 0)
        {
            return Ok(new LibroDiarioResponse { Encabezado = contexto.Encabezado });
        }

        var ids = asientos.Select(a => a.IdAsiento).ToList();

        var renglones = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join c in _ejecucionContext.TCuentasContables.AsNoTracking() on d.IdCtaContable equals c.IdCtaContable
            where ids.Contains(d.IdAsiento)
            orderby d.IdAsiento, d.IdAsientoDet
            select new
            {
                d.IdAsiento,
                d.IdAsientoDet,
                c.Codigo,
                c.Nombre,
                d.Debe,
                d.Haber,
                d.Anulado,
            }).ToListAsync();

        var tipos = await _ejecucionContext.TTiposAsiento
            .AsNoTracking()
            .ToDictionaryAsync(t => t.IdTipoAsiento, t => t.Descripcion);

        var porAsiento = renglones.GroupBy(r => r.IdAsiento).ToDictionary(g => g.Key, g => g.ToList());

        var lista = new List<AsientoDiarioResponse>(asientos.Count);

        foreach (var a in asientos)
        {
            var propios = porAsiento.GetValueOrDefault(a.IdAsiento) ?? [];
            var computables = contexto.IncluirAnulados ? propios : [.. propios.Where(r => r.Anulado != Anulado)];

            lista.Add(new AsientoDiarioResponse
            {
                IdAsiento = a.IdAsiento,
                Numero = a.Numero,
                FecContable = a.FecContable,
                Leyenda = a.Leyenda,
                TipoAsiento = tipos.GetValueOrDefault(a.IdTipoAsiento),
                Anulado = propios.Count > 0 && propios.All(r => r.Anulado == Anulado),
                TotalDebe = ReglasBalanceAsiento.Redondear(computables.Sum(r => r.Debe ?? 0)),
                TotalHaber = ReglasBalanceAsiento.Redondear(computables.Sum(r => r.Haber ?? 0)),
                Renglones =
                [
                    .. propios.Select(r => new RenglonDiarioResponse
                    {
                        CodigoCuenta = r.Codigo,
                        NombreCuenta = r.Nombre,
                        Debe = r.Debe,
                        Haber = r.Haber,
                        Anulado = r.Anulado == Anulado,
                    })
                ],
            });
        }

        return Ok(new LibroDiarioResponse
        {
            Encabezado = contexto.Encabezado,
            Asientos = lista,
            TotalAsientos = lista.Count,
            TotalDebe = ReglasBalanceAsiento.Redondear(lista.Sum(a => a.TotalDebe)),
            TotalHaber = ReglasBalanceAsiento.Redondear(lista.Sum(a => a.TotalHaber)),
        });
    }

    public async Task<OperationResponse<MayoresResponse>> Mayores(FiltroMayoresRequest filtro)
    {
        var contexto = await ResolverContexto(filtro);

        if (contexto.Error is not null) return BadRequest<MayoresResponse>(contexto.Error);

        var cuentas = await CuentasDelAlcance(contexto.Jurisdicciones);

        // Sin cuenta indicada se traen todas las imputables que tengan algo que mostrar. El
        // reporte de una sola cuenta es el uso habitual; el de todas sirve para revisar el
        // ejercicio completo.
        var seleccionadas = filtro.IdCtaContable is > 0
            ? cuentas.Where(c => c.IdCtaContable == filtro.IdCtaContable).ToList()
            : [.. cuentas.Where(c => c.EsImputable)];

        if (seleccionadas.Count == 0)
        {
            return NotFound<MayoresResponse>("No hay cuentas para el mayor con ese filtro.");
        }

        var ids = seleccionadas.Select(c => c.IdCtaContable).ToList();

        var anteriores = await MovimientosPorCuenta(
            contexto.Jurisdicciones, null, contexto.Desde.AddDays(-1), ids);

        var movimientos = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where ids.Contains(d.IdCtaContable)
                  && contexto.Jurisdicciones.Contains(a.IdJurisdiccion!.Value)
                  && a.FecContable >= contexto.Desde && a.FecContable <= contexto.Hasta
                  && (contexto.IncluirAnulados || d.Anulado == null || d.Anulado != Anulado)
            select new
            {
                d.IdCtaContable,
                a.IdAsiento,
                a.Numero,
                a.FecContable,
                a.Leyenda,
                d.Debe,
                d.Haber,
                d.Anulado,
            }).ToListAsync();

        var porCuenta = movimientos.GroupBy(m => m.IdCtaContable).ToDictionary(g => g.Key, g => g.ToList());

        var mayores = new List<MayorResponse>();

        foreach (var cuenta in seleccionadas.OrderBy(c => CodigoCuenta.Normalizar(c.Codigo), StringComparer.Ordinal))
        {
            var previos = anteriores.GetValueOrDefault(cuenta.IdCtaContable);
            var diferenciaAnterior = previos.Debe - previos.Haber;
            var propios = porCuenta.GetValueOrDefault(cuenta.IdCtaContable) ?? [];

            if (filtro.SoloConMovimiento && propios.Count == 0 && diferenciaAnterior == 0) continue;

            var mayor = ReglasMayor.Armar(
                cuenta.IdCtaContable,
                cuenta.Codigo,
                cuenta.Nombre,
                diferenciaAnterior,
                propios.Select(m => new MovimientoMayor(
                    m.IdAsiento, m.Numero, m.FecContable, m.Leyenda, m.Debe, m.Haber, m.Anulado == Anulado)));

            mayores.Add(new MayorResponse
            {
                IdCtaContable = mayor.IdCtaContable,
                Codigo = mayor.Codigo,
                Nombre = mayor.Nombre,
                SaldoAnterior = mayor.SaldoAnterior,
                TipoSaldoAnterior = mayor.TipoSaldoAnterior,
                TotalDebe = mayor.TotalDebe,
                TotalHaber = mayor.TotalHaber,
                SaldoFinal = mayor.SaldoFinal,
                TipoSaldoFinal = mayor.TipoSaldoFinal,
                Renglones =
                [
                    .. mayor.Renglones.Select(r => new RenglonMayorResponse
                    {
                        IdAsiento = r.IdAsiento,
                        NumeroAsiento = r.NumeroAsiento,
                        FecContable = r.FecContable,
                        Leyenda = r.Leyenda,
                        Debe = r.Debe,
                        Haber = r.Haber,
                        Saldo = r.Saldo,
                        TipoSaldo = r.TipoSaldo,
                        Anulado = r.Anulado,
                    })
                ],
            });
        }

        return Ok(new MayoresResponse { Encabezado = contexto.Encabezado, Mayores = mayores });
    }

    // ---- Auxiliares ------------------------------------------------------

    private record ContextoReporte(
        List<int> Jurisdicciones,
        DateTime Desde,
        DateTime Hasta,
        bool IncluirAnulados,
        EncabezadoReporte Encabezado,
        string? Error = null);

    /// <summary>
    /// Resuelve el alcance del reporte: periodo y jurisdicciones.
    /// </summary>
    /// <remarks>
    /// El alcance no es una sola jurisdiccion sino todas las que comparten NUMERO_JURISDICCION con
    /// la del usuario. El id cambia de un ejercicio a otro —en el stage, la jurisdiccion numero 1
    /// es 4000 en 2024, 10 en 2025 y 2437 en 2026— asi que sin esta traduccion un reporte con
    /// acumulado del ejercicio anterior se quedaria sin nada que acumular.
    /// </remarks>
    private async Task<ContextoReporte> ResolverContexto(FiltroReporteRequest filtro)
    {
        var vacio = new ContextoReporte([], DateTime.Today, DateTime.Today, false, new EncabezadoReporte());

        var ejercicio = filtro.Ejercicio ?? DateTime.Today.Year;

        var vigencia = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Ejercicio == ejercicio);

        if (vigencia is null) return vacio with { Error = $"No existe el ejercicio {ejercicio}." };

        var desde = filtro.FechaInicio ?? new DateTime(ejercicio, 1, 1);
        var hasta = filtro.FechaCorte ?? new DateTime(ejercicio, 12, 31);

        if (hasta < desde) return vacio with { Error = "La fecha de corte es anterior a la de inicio." };

        var idJurisdiccion = filtro.IdJurisdiccion ?? GetJurisdiccionCache();

        var numero = await _presupuestoContext.TJurisdicciones
            .AsNoTracking()
            .Where(j => j.IdJurisdiccion == idJurisdiccion)
            .Select(j => j.NumeroJurisdiccion)
            .FirstOrDefaultAsync();

        List<int> jurisdicciones;
        string? nombre = null;

        if (numero is null)
        {
            // Sin jurisdiccion en el token (solo en desarrollo) el reporte abarca todo.
            jurisdicciones = await _presupuestoContext.TJurisdicciones
                .AsNoTracking().Select(j => j.IdJurisdiccion).ToListAsync();
        }
        else
        {
            var equivalentes = await _presupuestoContext.TJurisdicciones
                .AsNoTracking()
                .Where(j => j.NumeroJurisdiccion == numero)
                .Select(j => new { j.IdJurisdiccion, j.NombreJurisdiccion })
                .ToListAsync();

            jurisdicciones = [.. equivalentes.Select(j => j.IdJurisdiccion)];
            nombre = equivalentes.FirstOrDefault()?.NombreJurisdiccion;
        }

        var incluirAnulados = filtro.IncluirAnulados ?? AppSettings.Contabilidad.IncluirAnuladosEnSaldos;

        return new ContextoReporte(
            jurisdicciones,
            desde,
            hasta,
            incluirAnulados,
            new EncabezadoReporte
            {
                Ejercicio = ejercicio,
                FechaInicio = desde,
                FechaCorte = hasta,
                Jurisdiccion = numero is null ? "Todas" : $"{numero} - {nombre}",
                JurisdiccionesIncluidas = jurisdicciones,
                IncluyeAnulados = incluirAnulados,
            });
    }

    /// <summary>
    /// Explica el descuadre del balance cuando lo hay.
    /// </summary>
    /// <remarks>
    /// La causa mas frecuente no es un error de carga sino un ejercicio anterior sin cerrar: las
    /// cuentas de resultado no arrastran saldo, asi que si su resultado nunca se traslado al
    /// patrimonio neto, la diferencia aparece aca. Decirlo evita que alguien salga a buscar un
    /// asiento descuadrado que no existe.
    /// </remarks>
    private async Task<string> ExplicarDescuadre(ContextoReporte contexto, decimal diferencia)
    {
        var ejercicioAnterior = contexto.Encabezado.Ejercicio - 1;

        var vigenciaAnterior = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.Ejercicio == ejercicioAnterior)
            .Select(v => (int?)v.IdVigencia)
            .FirstOrDefaultAsync();

        var descuadrados = await _ejecucionContext.TAsientos
            .AsNoTracking()
            .Where(a => contexto.Jurisdicciones.Contains(a.IdJurisdiccion!.Value))
            .Where(a => a.FecContable >= contexto.Desde && a.FecContable <= contexto.Hasta)
            .Where(a => _ejecucionContext.TAsientosDetalle
                            .Where(d => d.IdAsiento == a.IdAsiento).Sum(d => d.Debe ?? 0) !=
                        _ejecucionContext.TAsientosDetalle
                            .Where(d => d.IdAsiento == a.IdAsiento).Sum(d => d.Haber ?? 0))
            .CountAsync();

        if (descuadrados > 0)
        {
            return $"Hay {descuadrados} asiento(s) descuadrado(s) en el periodo. " +
                   $"La diferencia del balance es de {Math.Abs(diferencia):N2}.";
        }

        if (vigenciaAnterior is not null)
        {
            var cerrado = await TieneAsientoDeCierre(vigenciaAnterior.Value);

            if (!cerrado)
            {
                return $"El ejercicio {ejercicioAnterior} no tiene asiento de cierre. Las cuentas de " +
                       "resultado no arrastran saldo, asi que su resultado del ejercicio anterior no " +
                       $"llego al patrimonio neto: de ahi la diferencia de {Math.Abs(diferencia):N2}. " +
                       "No hay ningun asiento mal cargado.";
            }
        }

        return $"Los saldos deudores y acreedores difieren en {Math.Abs(diferencia):N2} y todos los " +
               "asientos del periodo cuadran. Conviene revisar el arrastre del ejercicio anterior.";
    }

    private async Task<bool> TieneAsientoDeCierre(int idVigencia)
    {
        var tipoCierre = AppSettings.Contabilidad.TipoAsientoCierre;

        if (tipoCierre <= 0) return false;

        return await _ejecucionContext.TAsientos
            .Where(a => a.IdVigencia == idVigencia && a.IdTipoAsiento == tipoCierre)
            .Take(1)
            .CountAsync() > 0;
    }

    private async Task<List<TCuentaContable>> CuentasDelAlcance(List<int> jurisdicciones)
        => await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => jurisdicciones.Count == 0 || jurisdicciones.Contains(c.IdJurisdiccion))
            .ToListAsync();

    /// <summary>
    /// Suma de debe y haber por cuenta dentro de un periodo. Sin fecha de inicio toma todo lo
    /// anterior a la fecha de corte, que es lo que hace falta para el acumulado.
    /// </summary>
    private async Task<Dictionary<int, (decimal Debe, decimal Haber)>> MovimientosPorCuenta(
        List<int> jurisdicciones,
        DateTime? desde,
        DateTime hasta,
        List<int>? soloEstasCuentas = null)
    {
        var incluirAnulados = AppSettings.Contabilidad.IncluirAnuladosEnSaldos;

        var consulta =
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where a.FecContable <= hasta
                  && (desde == null || a.FecContable >= desde)
                  && (jurisdicciones.Count == 0 || jurisdicciones.Contains(a.IdJurisdiccion!.Value))
                  && (incluirAnulados || d.Anulado == null || d.Anulado != Anulado)
                  && (soloEstasCuentas == null || soloEstasCuentas.Contains(d.IdCtaContable))
            group new { d.Debe, d.Haber } by d.IdCtaContable into g
            select new
            {
                IdCtaContable = g.Key,
                Debe = g.Sum(x => x.Debe ?? 0),
                Haber = g.Sum(x => x.Haber ?? 0),
            };

        var filas = await consulta.ToListAsync();

        return filas.ToDictionary(f => f.IdCtaContable, f => (f.Debe, f.Haber));
    }
}
