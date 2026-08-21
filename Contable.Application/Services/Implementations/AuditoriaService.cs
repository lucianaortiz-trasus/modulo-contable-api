using Contable.Application.ResponseDto.Dtos.Auditoria;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Auditoría contable: busca en el histórico lo que no cierra.
/// </summary>
/// <remarks>
/// Cada control apunta a algo concreto y lleva el id del asiento o de la cuenta para poder ir a
/// mirarlo. Un informe de auditoría que dice "hay 47 problemas" sin decir cuáles no sirve.
///
/// Los controles están ordenados por severidad, no por cantidad: un asiento descuadrado importa
/// más que doscientas partidas sin cuenta asignada.
/// </remarks>
public class AuditoriaService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IAuditoriaService
{
    private const string Anulado = "S";

    /// <summary>Tope de hallazgos por control, para que un problema masivo no tape a los demás.</summary>
    private const int MaximoPorControl = 200;

    public async Task<OperationResponse<AuditoriaResponse>> Auditar(int? ejercicio)
    {
        var anio = ejercicio ?? DateTime.Today.Year;

        var vigencia = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.Ejercicio == anio)
            .Select(v => (int?)v.IdVigencia)
            .FirstOrDefaultAsync();

        if (vigencia is null) return NotFound<AuditoriaResponse>($"No existe el ejercicio {anio}.");

        var jurisdicciones = await JurisdiccionesEquivalentes();

        var asientos = _ejecucionContext.TAsientos
            .AsNoTracking()
            .Where(a => a.IdVigencia == vigencia && jurisdicciones.Contains(a.IdJurisdiccion!.Value));

        var grupos = new List<GrupoDeHallazgos>
        {
            ReglasAuditoria.Grupo(ReglasAuditoria.AsientoDescuadrado, await Descuadrados(asientos)),
            ReglasAuditoria.Grupo(ReglasAuditoria.AsientoSinRenglones, await SinRenglones(asientos)),
            ReglasAuditoria.Grupo(ReglasAuditoria.RenglonHuerfano, await Huerfanos()),
            ReglasAuditoria.Grupo(ReglasAuditoria.FechaFueraDeVigencia, await FechaFueraDeEjercicio(asientos, anio)),
            ReglasAuditoria.Grupo(ReglasAuditoria.CuentaNoImputable, await SobreCuentas(asientos, imputable: false)),
            ReglasAuditoria.Grupo(ReglasAuditoria.CuentaInactiva, await SobreCuentas(asientos, activa: false)),
            ReglasAuditoria.Grupo(ReglasAuditoria.PosibleDuplicado, await Duplicados(asientos)),
            ReglasAuditoria.Grupo(ReglasAuditoria.EstructuraDelPlan, await EstructuraDelPlan(jurisdicciones)),
            ReglasAuditoria.Grupo(ReglasAuditoria.CuentaSinMatriz, await CuentasSinMatriz(vigencia.Value, jurisdicciones)),
            ReglasAuditoria.Grupo(ReglasAuditoria.PartidaSinCuenta, await PartidasSinCuenta(vigencia.Value)),
        };

        var conHallazgos = grupos.Where(g => g.Cantidad > 0).ToList();

        return Ok(new AuditoriaResponse
        {
            Ejercicio = anio,
            Errores = conHallazgos.Where(g => g.Severidad == Severidad.Error).Sum(g => g.Cantidad),
            Advertencias = conHallazgos.Where(g => g.Severidad == Severidad.Advertencia).Sum(g => g.Cantidad),
            Sugerencias = conHallazgos.Where(g => g.Severidad == Severidad.Sugerencia).Sum(g => g.Cantidad),
            ControlesCorridos = grupos.Count,
            ControlesLimpios = grupos.Count - conHallazgos.Count,
            Grupos =
            [
                .. conHallazgos
                    .OrderBy(g => g.Severidad)
                    .ThenByDescending(g => g.Cantidad)
                    .Select(g => new GrupoResponse
                    {
                        Tipo = g.Tipo,
                        Titulo = g.Titulo,
                        Severidad = g.Severidad.ToString(),
                        Explicacion = g.Explicacion,
                        Cantidad = g.Cantidad,
                        Hallazgos =
                        [
                            .. g.Hallazgos.Take(MaximoPorControl).Select(h => new HallazgoResponse
                            {
                                Descripcion = h.Descripcion,
                                IdAsiento = h.IdAsiento,
                                Numero = h.Numero,
                                IdCtaContable = h.IdCtaContable,
                                Codigo = h.Codigo,
                                Importe = h.Importe,
                            })
                        ],
                        Recortado = g.Cantidad > MaximoPorControl,
                    })
            ],
        });
    }

    // ---- Controles -------------------------------------------------------

    private async Task<List<Hallazgo>> Descuadrados(IQueryable<TAsiento> asientos)
    {
        var filas = await asientos
            .Select(a => new
            {
                a.IdAsiento,
                a.Numero,
                a.Leyenda,
                Debe = _ejecucionContext.TAsientosDetalle.Where(d => d.IdAsiento == a.IdAsiento).Sum(d => d.Debe ?? 0),
                Haber = _ejecucionContext.TAsientosDetalle.Where(d => d.IdAsiento == a.IdAsiento).Sum(d => d.Haber ?? 0),
                Renglones = _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento),
            })
            .Where(x => x.Renglones > 0 && x.Debe != x.Haber)
            .ToListAsync();

        return
        [
            .. filas.Select(x => new Hallazgo(
                ReglasAuditoria.AsientoDescuadrado, Severidad.Error,
                $"El debe suma {x.Debe:N2} y el haber {x.Haber:N2}: difieren en {Math.Abs(x.Debe - x.Haber):N2}.",
                x.IdAsiento, x.Numero, Importe: Math.Abs(x.Debe - x.Haber)))
        ];
    }

    private async Task<List<Hallazgo>> SinRenglones(IQueryable<TAsiento> asientos)
    {
        var filas = await asientos
            .Where(a => _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento) == 0)
            .Select(a => new { a.IdAsiento, a.Numero, a.Leyenda })
            .ToListAsync();

        return
        [
            .. filas.Select(x => new Hallazgo(
                ReglasAuditoria.AsientoSinRenglones, Severidad.Error,
                $"La cabecera existe pero no tiene detalle: \"{x.Leyenda}\".",
                x.IdAsiento, x.Numero))
        ];
    }

    private async Task<List<Hallazgo>> Huerfanos()
    {
        var cantidad = await _ejecucionContext.TAsientosDetalle
            .AsNoTracking()
            .Where(d => _ejecucionContext.TAsientos.Count(a => a.IdAsiento == d.IdAsiento) == 0)
            .CountAsync();

        return cantidad == 0
            ? []
            : [new Hallazgo(ReglasAuditoria.RenglonHuerfano, Severidad.Error,
                $"Hay {cantidad} renglon(es) que apuntan a un asiento inexistente.")];
    }

    private async Task<List<Hallazgo>> FechaFueraDeEjercicio(IQueryable<TAsiento> asientos, int anio)
    {
        var filas = await asientos
            .Where(a => a.FecContable.Year != anio)
            .Select(a => new { a.IdAsiento, a.Numero, a.FecContable })
            .ToListAsync();

        return
        [
            .. filas.Select(x => new Hallazgo(
                ReglasAuditoria.FechaFueraDeVigencia, Severidad.Error,
                $"Esta asignado al ejercicio {anio} pero su fecha contable es {x.FecContable:dd/MM/yyyy}.",
                x.IdAsiento, x.Numero))
        ];
    }

    private async Task<List<Hallazgo>> SobreCuentas(IQueryable<TAsiento> asientos, bool imputable = true, bool activa = true)
    {
        var buscaNoImputables = !imputable;

        var filas = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in asientos on d.IdAsiento equals a.IdAsiento
            join c in _ejecucionContext.TCuentasContables.AsNoTracking() on d.IdCtaContable equals c.IdCtaContable
            where buscaNoImputables ? c.Imputable != "S" : c.Activa != "A"
            select new { a.IdAsiento, a.Numero, c.IdCtaContable, c.Codigo, c.Nombre, d.Debe, d.Haber })
            .ToListAsync();

        var tipo = buscaNoImputables ? ReglasAuditoria.CuentaNoImputable : ReglasAuditoria.CuentaInactiva;
        var texto = buscaNoImputables ? "no es imputable" : "esta dada de baja";

        return
        [
            .. filas.Select(x => new Hallazgo(
                tipo, Severidad.Advertencia,
                $"El asiento imputa a {x.Codigo} {x.Nombre}, que {texto}.",
                x.IdAsiento, x.Numero, x.IdCtaContable, x.Codigo,
                (x.Debe ?? 0) + (x.Haber ?? 0)))
        ];
    }

    private async Task<List<Hallazgo>> Duplicados(IQueryable<TAsiento> asientos)
    {
        var candidatos = await asientos
            .Select(a => new
            {
                a.IdAsiento,
                a.Numero,
                a.FecContable,
                a.Leyenda,
                Total = _ejecucionContext.TAsientosDetalle
                    .Where(d => d.IdAsiento == a.IdAsiento && (d.Anulado == null || d.Anulado != Anulado))
                    .Sum(d => d.Debe ?? 0),
            })
            .ToListAsync();

        var grupos = ReglasDuplicados.Detectar(candidatos.Select(c =>
            new ReglasDuplicados.AsientoParaComparar(c.IdAsiento, c.Numero, c.FecContable, c.Total, c.Leyenda)));

        var hallazgos = new List<Hallazgo>();

        foreach (var grupo in grupos)
        {
            var numeros = string.Join(", ", grupo.Asientos.Select(a => $"N° {a.Numero}"));

            foreach (var asiento in grupo.Asientos)
            {
                hallazgos.Add(new Hallazgo(
                    ReglasAuditoria.PosibleDuplicado, Severidad.Advertencia,
                    $"Comparte fecha ({grupo.FecContable:dd/MM/yyyy}), importe ({grupo.Total:N2}) y leyenda con " +
                    $"otro(s) asiento(s): {numeros}.",
                    asiento.IdAsiento, asiento.Numero, Importe: grupo.Total));
            }
        }

        return hallazgos;
    }

    private async Task<List<Hallazgo>> EstructuraDelPlan(List<int> jurisdicciones)
    {
        var cuentas = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => jurisdicciones.Contains(c.IdJurisdiccion))
            .ToListAsync();

        var arbol = ArbolDeCuentas.Armar(cuentas);

        return
        [
            .. arbol.ConAnomalias.Select(n => new Hallazgo(
                ReglasAuditoria.EstructuraDelPlan, Severidad.Advertencia,
                string.Join(" ", DescribirAnomalias(n.Anomalias)),
                IdCtaContable: n.Cuenta.IdCtaContable,
                Codigo: n.Cuenta.Codigo))
        ];
    }

    private async Task<List<Hallazgo>> CuentasSinMatriz(int idVigencia, List<int> jurisdicciones)
    {
        var enLaMatriz = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .Where(m => m.IdVigencia == idVigencia && m.IdCtaContable != null)
            .Select(m => m.IdCtaContable!.Value)
            .Distinct()
            .ToListAsync();

        var conMovimiento = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            join c in _ejecucionContext.TCuentasContables.AsNoTracking() on d.IdCtaContable equals c.IdCtaContable
            where a.IdVigencia == idVigencia && c.Imputable == "S"
            group c by new { c.IdCtaContable, c.Codigo, c.Nombre } into g
            select new { g.Key.IdCtaContable, g.Key.Codigo, g.Key.Nombre, Movimientos = g.Count() })
            .ToListAsync();

        return
        [
            .. conMovimiento
                .Where(c => !enLaMatriz.Contains(c.IdCtaContable))
                .OrderByDescending(c => c.Movimientos)
                .Select(c => new Hallazgo(
                    ReglasAuditoria.CuentaSinMatriz, Severidad.Sugerencia,
                    $"Tiene {c.Movimientos} movimiento(s) en el ejercicio pero ninguna partida asignada en la matriz.",
                    IdCtaContable: c.IdCtaContable, Codigo: c.Codigo))
        ];
    }

    private async Task<List<Hallazgo>> PartidasSinCuenta(int idVigencia)
    {
        var conCuenta = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .Where(m => m.IdVigencia == idVigencia && m.IdObjetoGasto != null)
            .Select(m => m.IdObjetoGasto!.Value)
            .ToListAsync();

        var sinCuenta = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => o.IdVigencia == idVigencia && o.UltimoNivel == "S")
            .Select(o => new { o.IdObjetoGasto, o.NumeroObjeto, o.NombreObjeto })
            .ToListAsync();

        return
        [
            .. sinCuenta
                .Where(o => !conCuenta.Contains(o.IdObjetoGasto))
                .OrderBy(o => o.NumeroObjeto)
                .Select(o => new Hallazgo(
                    ReglasAuditoria.PartidaSinCuenta, Severidad.Sugerencia,
                    $"El objeto de gasto {o.NumeroObjeto} \"{o.NombreObjeto}\" no tiene cuenta contable asignada.",
                    Codigo: o.NumeroObjeto?.ToString()))
        ];
    }

    // ---- Auxiliares ------------------------------------------------------

    private static List<string> DescribirAnomalias(AnomaliaJerarquia anomalias) =>
    [
        .. Enum.GetValues<AnomaliaJerarquia>()
            .Where(a => a != AnomaliaJerarquia.Ninguna && anomalias.HasFlag(a))
            .Select(a => a switch
            {
                AnomaliaJerarquia.SeApuntaASiMisma => "Figura como padre de si misma.",
                AnomaliaJerarquia.PadreInexistente => "Su cuenta padre no existe.",
                AnomaliaJerarquia.FormaUnCiclo => "La cadena de cuentas padre vuelve sobre si misma.",
                AnomaliaJerarquia.NivelInconsistente => "Su nivel no es el del padre mas uno.",
                AnomaliaJerarquia.CodigoNoCuelgaDelPadre => "Su codigo no cuelga del codigo del padre.",
                AnomaliaJerarquia.ImputableConHijas => "Esta marcada como imputable pero tiene hijas.",
                _ => a.ToString(),
            })
    ];

    private async Task<List<int>> JurisdiccionesEquivalentes()
    {
        var numero = await _presupuestoContext.TJurisdicciones
            .AsNoTracking()
            .Where(j => j.IdJurisdiccion == GetJurisdiccionCache())
            .Select(j => j.NumeroJurisdiccion)
            .FirstOrDefaultAsync();

        return numero is null
            ? await _presupuestoContext.TJurisdicciones.AsNoTracking().Select(j => j.IdJurisdiccion).ToListAsync()
            : await _presupuestoContext.TJurisdicciones.AsNoTracking()
                .Where(j => j.NumeroJurisdiccion == numero).Select(j => j.IdJurisdiccion).ToListAsync();
    }
}
