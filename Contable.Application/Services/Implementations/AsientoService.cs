using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Contable.Application.RequestDto.Asientos;
using Contable.Application.ResponseDto.Dtos.Asientos;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Asientos contables: consulta, alta manual, reversa, anulacion, cierre y apertura de ejercicio.
/// </summary>
/// <remarks>
/// Reemplaza a PKG_T_ASIENTOS y PKG_T_ASIENTOS_DET. Las decisiones estan en _analisis/REGLAS.md.
/// Lo que conviene tener presente al leer este archivo:
///
/// - El alta es <b>una sola operacion transaccional</b>: cabecera y renglones entran o no entran
///   juntos. En el sistema viejo eran dos procedimientos separados y nada garantizaba que el
///   asiento quedara completo ni cuadrado.
/// - La numeracion es un correlativo perpetuo por jurisdiccion. `F_NUMERO_ASIENTO` hacia
///   `MAX(numero) + 1` sin ningun bloqueo, asi que dos altas simultaneas se llevaban el mismo
///   numero (defecto D4). Aca la lectura del maximo va precedida de un `FOR UPDATE`.
/// - Ni la reversa ni la anulacion borran nada: generan un contraasiento invertido y marcan
///   `ANULADO = 'S'` en los dos.
/// </remarks>
public class AsientoService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IAsientoService
{
    private const string SecuenciaAsiento = "EJECUCION.SQ_T_ASIENTOS";

    private const string SecuenciaDetalle = "EJECUCION.SQ_T_ASIENTOS_DETALLES";

    private const string Anulado = "S";

    private const string NoAnulado = "N";

    // ---- Consulta --------------------------------------------------------

    public async Task<OperationResponse<List<AsientoResumenResponse>>> Buscar(FiltroAsientosRequest filtro)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamano = filtro.TamanoPagina is < 1 or > 200 ? 20 : filtro.TamanoPagina;

        var consulta = _ejecucionContext.TAsientos.AsNoTracking();

        var vigencia = filtro.IdVigencia ?? GetIdVigencia();
        if (vigencia > 0) consulta = consulta.Where(a => a.IdVigencia == vigencia);

        var jurisdiccion = filtro.IdJurisdiccion ?? GetJurisdiccionCache();
        if (jurisdiccion > 0) consulta = consulta.Where(a => a.IdJurisdiccion == jurisdiccion);

        if (filtro.FechaDesde is not null) consulta = consulta.Where(a => a.FecContable >= filtro.FechaDesde);
        if (filtro.FechaHasta is not null) consulta = consulta.Where(a => a.FecContable <= filtro.FechaHasta);
        if (filtro.IdTipoAsiento is > 0) consulta = consulta.Where(a => a.IdTipoAsiento == filtro.IdTipoAsiento);
        if (filtro.Numero is > 0) consulta = consulta.Where(a => a.Numero == filtro.Numero);

        if (!string.IsNullOrWhiteSpace(filtro.Leyenda))
        {
            var texto = filtro.Leyenda.Trim().ToUpper();
            consulta = consulta.Where(a => a.Leyenda != null && a.Leyenda.ToUpper().Contains(texto));
        }

        // La marca de anulado vive en el detalle, no en la cabecera: un asiento esta anulado
        // cuando todos sus renglones lo estan. El filtro va en la consulta y no en memoria,
        // porque aplicarlo despues de paginar recorta la pagina pero deja el total sin tocar.
        if (filtro.Anulados is not null)
        {
            consulta = filtro.Anulados.Value
                ? consulta.Where(a =>
                    _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento) > 0 &&
                    _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento && d.Anulado == Anulado) ==
                    _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento))
                : consulta.Where(a =>
                    _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento) == 0 ||
                    _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento && d.Anulado == Anulado) <
                    _ejecucionContext.TAsientosDetalle.Count(d => d.IdAsiento == a.IdAsiento));
        }

        var total = await consulta.CountAsync();

        // Los totales del asiento se resuelven en la misma consulta que la pagina, con
        // subconsultas escalares. Traer todos los renglones para sumarlos en memoria costaba
        // tres viajes de ida y vuelta mas, y contra una base en otro continente cada viaje son
        // casi doscientos milisegundos.
        var pagina0 = await consulta
            .OrderByDescending(a => a.FecContable)
            .ThenByDescending(a => a.Numero)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(a => new
            {
                Asiento = a,
                TipoAsiento = _ejecucionContext.TTiposAsiento
                    .Where(t => t.IdTipoAsiento == a.IdTipoAsiento)
                    .Select(t => t.Descripcion)
                    .FirstOrDefault(),
                TotalDebe = _ejecucionContext.TAsientosDetalle
                    .Where(d => d.IdAsiento == a.IdAsiento).Sum(d => d.Debe ?? 0),
                TotalHaber = _ejecucionContext.TAsientosDetalle
                    .Where(d => d.IdAsiento == a.IdAsiento).Sum(d => d.Haber ?? 0),
                Renglones = _ejecucionContext.TAsientosDetalle
                    .Count(d => d.IdAsiento == a.IdAsiento),
                RenglonesAnulados = _ejecucionContext.TAsientosDetalle
                    .Count(d => d.IdAsiento == a.IdAsiento && d.Anulado == Anulado),
                Contrapartidas = _ejecucionContext.TAsientos
                    .Count(x => x.IdAsientoRel == a.IdAsiento),
            })
            .ToListAsync();

        var resumen = pagina0
            .Select(x => new AsientoResumenResponse
            {
                IdAsiento = x.Asiento.IdAsiento,
                Numero = x.Asiento.Numero,
                Anio = x.Asiento.Anio,
                FecContable = x.Asiento.FecContable,
                Leyenda = x.Asiento.Leyenda,
                IdTipoAsiento = x.Asiento.IdTipoAsiento,
                TipoAsiento = x.TipoAsiento,
                IdJurisdiccion = x.Asiento.IdJurisdiccion,
                IdAsientoRel = x.Asiento.IdAsientoRel,
                TieneContrapartida = x.Contrapartidas > 0,
                UserAlta = x.Asiento.UserAlta,
                FecAlta = x.Asiento.FecAlta,
                CantidadDeRenglones = x.Renglones,
                TotalDebe = ReglasBalanceAsiento.Redondear(x.TotalDebe),
                TotalHaber = ReglasBalanceAsiento.Redondear(x.TotalHaber),
                Anulado = x.Renglones > 0 && x.RenglonesAnulados == x.Renglones,
            })
            .ToList();

        return Paginado(resumen, total);
    }

    public async Task<OperationResponse<List<TipoAsientoResponse>>> GetTipos()
    {
        var tipos = await _ejecucionContext.TTiposAsiento
            .AsNoTracking()
            .OrderBy(t => t.IdTipoAsiento)
            .Select(t => new TipoAsientoResponse
            {
                IdTipoAsiento = t.IdTipoAsiento,
                Codigo = t.Codigo,
                Descripcion = t.Descripcion,
            })
            .ToListAsync();

        return Ok(tipos);
    }

    public async Task<OperationResponse<AsientoResponse>> GetPorId(int idAsiento)
    {
        var asiento = await _ejecucionContext.TAsientos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAsiento == idAsiento);

        if (asiento is null) return NotFound<AsientoResponse>("El asiento no existe.");

        var renglones = await RenglonesDe(idAsiento);
        var cuentas = await CuentasPorId(renglones.Select(r => r.IdCtaContable));

        var comoRenglones = renglones.Select(r => new RenglonAsiento(r.IdCtaContable, r.Debe, r.Haber)).ToList();

        var tipo = await _ejecucionContext.TTiposAsiento
            .AsNoTracking()
            .Where(t => t.IdTipoAsiento == asiento.IdTipoAsiento)
            .Select(t => t.Descripcion)
            .FirstOrDefaultAsync();

        var respuesta = new AsientoResponse
        {
            IdAsiento = asiento.IdAsiento,
            Numero = asiento.Numero,
            Anio = asiento.Anio,
            IdVigencia = asiento.IdVigencia,
            IdJurisdiccion = asiento.IdJurisdiccion,
            FecContable = asiento.FecContable,
            Leyenda = asiento.Leyenda,
            Observaciones = asiento.Observaciones,
            IdTipoAsiento = asiento.IdTipoAsiento,
            TipoAsiento = tipo,
            IdAsientoRel = asiento.IdAsientoRel,
            TieneContrapartida = await Existe(_ejecucionContext.TAsientos.Where(a => a.IdAsientoRel == idAsiento)),
            UserAlta = asiento.UserAlta,
            FecAlta = asiento.FecAlta,
            TotalDebe = ReglasBalanceAsiento.TotalDebe(comoRenglones),
            TotalHaber = ReglasBalanceAsiento.TotalHaber(comoRenglones),
            Cuadra = ReglasBalanceAsiento.Cuadra(comoRenglones),
            CantidadDeRenglones = renglones.Count,
            Anulado = renglones.Count > 0 && renglones.All(r => r.Anulado == Anulado),
            Renglones =
            [
                .. renglones.Select(r => new RenglonResponse
                {
                    IdAsientoDet = r.IdAsientoDet,
                    IdCtaContable = r.IdCtaContable,
                    CodigoCuenta = cuentas.GetValueOrDefault(r.IdCtaContable)?.Codigo,
                    NombreCuenta = cuentas.GetValueOrDefault(r.IdCtaContable)?.Nombre,
                    Debe = r.Debe,
                    Haber = r.Haber,
                    IdRecursoDh = r.IdRecursoDh,
                    IdObjetoGasto = r.IdObjetoGasto,
                    IdCuentaBancaria = r.IdCuentaBancaria,
                    Anulado = r.Anulado == Anulado,
                })
            ],
        };

        return Ok(respuesta);
    }

    // ---- Alta ------------------------------------------------------------

    public async Task<OperationResponse<AsientoResponse>> Crear(AsientoRequest request)
    {
        var renglones = request.Renglones ?? [];

        var balance = ReglasBalanceAsiento.Validar(
            renglones.Select(r => new RenglonAsiento(r.IdCtaContable, r.Debe, r.Haber)));

        if (!balance.EsValida) return BadRequest<AsientoResponse>(balance.Mensaje);

        var idUnidadAdm = request.IdUnidadAdm ?? GetUnidadAdministrativa();
        var contexto = await ArmarContextoDeCabecera(request.FecContable, idUnidadAdm, request.IdTipoAsiento);

        var datos = new DatosAsiento(request.FecContable, idUnidadAdm, request.IdTipoAsiento, request.Leyenda);
        var cabecera = ReglasAsiento.ValidarAlta(datos, contexto);

        if (!cabecera.EsValida) return BadRequest<AsientoResponse>(cabecera.Mensaje);

        var cuentas = await CuentasPorId(renglones.Select(r => r.IdCtaContable));
        var imputacion = ValidarImputacion(renglones, cuentas);

        if (!imputacion.EsValida) return BadRequest<AsientoResponse>(imputacion.Mensaje);

        var planificado = new AsientoPlanificado(
            request.Leyenda!,
            request.Observaciones,
            request.FecContable!.Value,
            request.IdTipoAsiento!.Value,
            [
                .. renglones.Select(r => new RenglonPlanificado(
                    r.IdCtaContable, r.Debe, r.Haber, r.IdRecursoDh, r.IdCuentaBancaria))
            ]);

        var idAsiento = await Registrar(planificado, contexto.VigenciaDelAnio!.Value, contexto.IdJurisdiccion!.Value);

        return await GetPorId(idAsiento);
    }

    // ---- Reversa y anulacion --------------------------------------------

    public Task<OperationResponse<AsientoResponse>> Reversar(int idAsiento)
        => Contrapartir(idAsiento, TipoDeContrapartida.Reversa);

    public Task<OperationResponse<AsientoResponse>> Anular(int idAsiento)
        => Contrapartir(idAsiento, TipoDeContrapartida.Anulacion);

    private async Task<OperationResponse<AsientoResponse>> Contrapartir(int idAsiento, TipoDeContrapartida tipo)
    {
        var asiento = await _ejecucionContext.TAsientos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAsiento == idAsiento);

        if (asiento is null) return NotFound<AsientoResponse>("El asiento no existe.");

        var renglones = await RenglonesDe(idAsiento);

        if (renglones.Count == 0) return BadRequest<AsientoResponse>("El asiento no tiene renglones.");

        var ejercicio = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.IdVigencia == asiento.IdVigencia)
            .Select(v => v.Ejercicio)
            .FirstOrDefaultAsync() ?? asiento.Anio;

        var original = new AsientoOriginal(
            asiento.IdAsiento,
            asiento.Numero,
            asiento.FecContable,
            asiento.IdTipoAsiento,
            asiento.Leyenda,
            asiento.Observaciones,
            ejercicio,
            YaAnulado: renglones.All(r => r.Anulado == Anulado),
            YaTieneContrapartida: await Existe(_ejecucionContext.TAsientos.Where(a => a.IdAsientoRel == idAsiento)));

        var validacion = ReglasContrapartida.Validar(original);

        if (!validacion.EsValida) return BadRequest<AsientoResponse>(validacion.Mensaje);

        var plan = ReglasContrapartida.Planificar(
            tipo,
            original,
            renglones.Select(r => new RenglonPlanificado(
                r.IdCtaContable, r.Debe, r.Haber, r.IdRecursoDh, r.IdCuentaBancaria)),
            DateTime.Today);

        var vigencia = await VigenciaDelAnio(plan.FecContable.Year) ?? asiento.IdVigencia;

        var idNuevo = await Registrar(
            plan,
            vigencia,
            asiento.IdJurisdiccion ?? 0,
            marcarAnulado: true,
            idAsientoAAnular: idAsiento);

        return await GetPorId(idNuevo);
    }

    // ---- Cierre y apertura ----------------------------------------------

    public Task<OperationResponse<PlanAsientosResponse>> PlanificarCierre(int ejercicio)
        => Cierre(ejercicio, confirmar: false);

    public Task<OperationResponse<PlanAsientosResponse>> GenerarCierre(int ejercicio)
        => Cierre(ejercicio, confirmar: true);

    public Task<OperationResponse<PlanAsientosResponse>> PlanificarApertura(int ejercicio)
        => Apertura(ejercicio, confirmar: false);

    public Task<OperationResponse<PlanAsientosResponse>> GenerarApertura(int ejercicio)
        => Apertura(ejercicio, confirmar: true);

    private async Task<OperationResponse<PlanAsientosResponse>> Cierre(int ejercicio, bool confirmar)
    {
        var vigencia = await VigenciaDelAnio(ejercicio);

        if (vigencia is null) return NotFound<PlanAsientosResponse>($"No existe el ejercicio {ejercicio}.");

        var config = ConfiguracionDelMunicipio();
        var saldos = await SaldosDelEjercicio(vigencia.Value);
        var yaCerrado = await TieneAsientoDeTipo(vigencia.Value, config.TipoAsientoCierre);

        var plan = ReglasCierreEjercicio.PlanificarCierre(saldos, ejercicio, config, yaCerrado);

        if (!plan.SePuedeGenerar)
        {
            return Ok(new PlanAsientosResponse
            {
                Ejercicio = ejercicio,
                SePuedeGenerar = false,
                Motivo = plan.Validacion.Mensaje,
            });
        }

        return Ok(await MaterializarPlan(plan.Asientos, ejercicio, vigencia.Value, config, confirmar));
    }

    private async Task<OperationResponse<PlanAsientosResponse>> Apertura(int ejercicio, bool confirmar)
    {
        var vigencia = await VigenciaDelAnio(ejercicio);

        if (vigencia is null) return NotFound<PlanAsientosResponse>($"No existe el ejercicio {ejercicio}.");

        var vigenciaAnterior = await VigenciaDelAnio(ejercicio - 1);

        if (vigenciaAnterior is null)
        {
            return NotFound<PlanAsientosResponse>($"No existe el ejercicio {ejercicio - 1} para tomar el cierre.");
        }

        var config = ConfiguracionDelMunicipio();
        var yaAbierto = await TieneAsientoDeTipo(vigencia.Value, config.TipoAsientoApertura);

        var renglonesDelCierre = await RenglonesDelCierrePatrimonial(vigenciaAnterior.Value, config);

        var (validacion, asiento) = ReglasCierreEjercicio.PlanificarApertura(
            renglonesDelCierre, ejercicio, config, yaAbierto);

        if (asiento is null)
        {
            return Ok(new PlanAsientosResponse
            {
                Ejercicio = ejercicio,
                SePuedeGenerar = false,
                Motivo = validacion.Mensaje,
            });
        }

        return Ok(await MaterializarPlan([asiento], ejercicio, vigencia.Value, config, confirmar));
    }

    private async Task<PlanAsientosResponse> MaterializarPlan(
        List<AsientoPlanificado> asientos,
        int ejercicio,
        int idVigencia,
        ConfiguracionCierre config,
        bool confirmar)
    {
        var jurisdiccion = await JurisdiccionDeUnidad(config.UnidadAdministrativa, idVigencia)
                           ?? GetJurisdiccionCache();

        var cuentas = await CuentasPorId(asientos.SelectMany(a => a.Renglones).Select(r => r.IdCtaContable));

        var respuesta = new PlanAsientosResponse
        {
            Ejercicio = ejercicio,
            SePuedeGenerar = true,
            Confirmado = confirmar,
        };

        foreach (var asiento in asientos)
        {
            var comoRenglones = asiento.Renglones
                .Select(r => new RenglonAsiento(r.IdCtaContable, r.Debe, r.Haber))
                .ToList();

            int? idGenerado = null;
            int? numero = null;

            if (confirmar)
            {
                idGenerado = await Registrar(asiento, idVigencia, jurisdiccion);

                numero = await _ejecucionContext.TAsientos
                    .AsNoTracking()
                    .Where(a => a.IdAsiento == idGenerado)
                    .Select(a => (int?)a.Numero)
                    .FirstOrDefaultAsync();
            }

            respuesta.Asientos.Add(new AsientoPlanificadoResponse
            {
                Leyenda = asiento.Leyenda,
                FecContable = asiento.FecContable,
                IdTipoAsiento = asiento.IdTipoAsiento,
                CantidadDeRenglones = asiento.Renglones.Count,
                TotalDebe = ReglasBalanceAsiento.TotalDebe(comoRenglones),
                TotalHaber = ReglasBalanceAsiento.TotalHaber(comoRenglones),
                Cuadra = ReglasBalanceAsiento.Cuadra(comoRenglones),
                IdAsiento = idGenerado,
                Numero = numero,
                Renglones =
                [
                    .. asiento.Renglones.Select(r => new RenglonResponse
                    {
                        IdCtaContable = r.IdCtaContable,
                        CodigoCuenta = cuentas.GetValueOrDefault(r.IdCtaContable)?.Codigo,
                        NombreCuenta = cuentas.GetValueOrDefault(r.IdCtaContable)?.Nombre,
                        Debe = r.Debe,
                        Haber = r.Haber,
                        IdRecursoDh = r.IdRecursoDh,
                    })
                ],
            });
        }

        return respuesta;
    }

    // ---- Escritura -------------------------------------------------------

    /// <summary>
    /// Inserta cabecera y renglones en una sola transaccion, y opcionalmente marca como anulado
    /// el asiento de origen.
    /// </summary>
    private async Task<int> Registrar(
        AsientoPlanificado plan,
        int idVigencia,
        int idJurisdiccion,
        bool marcarAnulado = false,
        int? idAsientoAAnular = null)
    {
        var estrategia = _ejecucionContext.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _ejecucionContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var numero = await ProximoNumeroConBloqueo(idJurisdiccion);

            var asiento = new TAsiento
            {
                IdAsiento = await GetSecuencia(_ejecucionContext, SecuenciaAsiento),
                Numero = numero,
                Anio = plan.FecContable.Year,
                IdVigencia = idVigencia,
                IdJurisdiccion = idJurisdiccion,
                IdTipoAsiento = plan.IdTipoAsiento,
                FecContable = plan.FecContable,
                Leyenda = plan.Leyenda,
                Observaciones = plan.Observaciones,
                IdAsientoRel = plan.IdAsientoRel,
                UserAlta = GetUserCache(),
                FecAlta = DateTime.Now,
            };

            _ejecucionContext.TAsientos.Add(asiento);

            foreach (var renglon in plan.Renglones)
            {
                _ejecucionContext.TAsientosDetalle.Add(new TAsientoDetalle
                {
                    IdAsientoDet = await GetSecuencia(_ejecucionContext, SecuenciaDetalle),
                    IdAsiento = asiento.IdAsiento,
                    IdCtaContable = renglon.IdCtaContable,
                    // El detalle hereda la fecha de la cabecera, igual que INSERTAR_DET.
                    FecContable = plan.FecContable,
                    Debe = renglon.Debe is null ? null : ReglasBalanceAsiento.Redondear(renglon.Debe.Value),
                    Haber = renglon.Haber is null ? null : ReglasBalanceAsiento.Redondear(renglon.Haber.Value),
                    IdRecursoDh = renglon.IdRecursoDh,
                    IdCuentaBancaria = renglon.IdCuentaBancaria,
                    Anulado = marcarAnulado ? Anulado : NoAnulado,
                    UserIng = GetUserCache(),
                    FecIng = DateTime.Now,
                });
            }

            await _ejecucionContext.SaveChangesAsync();

            if (marcarAnulado && idAsientoAAnular is not null)
            {
                // El original queda anulado junto con su contrapartida: los dos se cancelan.
                await _ejecucionContext.TAsientosDetalle
                    .Where(d => d.IdAsiento == idAsientoAAnular)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(d => d.Anulado, Anulado)
                        .SetProperty(d => d.UserAct, GetUserCache())
                        .SetProperty(d => d.FecAct, DateTime.Now));
            }

            await transaccion.CommitAsync();

            return asiento.IdAsiento;
        });
    }

    /// <summary>
    /// Proximo numero de la jurisdiccion, tomando antes un bloqueo sobre el asiento que hoy tiene
    /// el maximo.
    /// </summary>
    /// <remarks>
    /// `F_NUMERO_ASIENTO` hacia `MAX(numero) + 1` sin bloquear nada: dos altas simultaneas en la
    /// misma jurisdiccion leian el mismo maximo y grababan el mismo numero (defecto D4). El
    /// `FOR UPDATE` hace que la segunda transaccion espere a que la primera confirme.
    /// </remarks>
    private async Task<int> ProximoNumeroConBloqueo(int idJurisdiccion)
    {
        var conexion = _ejecucionContext.Database.GetDbConnection();

        if (conexion.State != ConnectionState.Open) await conexion.OpenAsync();

        await using (var bloqueo = conexion.CreateCommand())
        {
            bloqueo.Transaction = _ejecucionContext.Database.CurrentTransaction?.GetDbTransaction();
            bloqueo.CommandText = """
                SELECT ID_ASIENTO FROM EJECUCION.T_ASIENTOS
                 WHERE ID_JURISDICCION = :jurisdiccion
                   AND NUMERO = (SELECT MAX(NUMERO) FROM EJECUCION.T_ASIENTOS WHERE ID_JURISDICCION = :jurisdiccion)
                   FOR UPDATE
                """;

            var parametro = bloqueo.CreateParameter();
            parametro.ParameterName = "jurisdiccion";
            parametro.Value = idJurisdiccion;
            bloqueo.Parameters.Add(parametro);

            // Si la jurisdiccion todavia no tiene asientos no hay fila que bloquear, y el primer
            // numero sera 1.
            await using var lector = await bloqueo.ExecuteReaderAsync();
            while (await lector.ReadAsync()) { }
        }

        var maximo = await _ejecucionContext.TAsientos
            .Where(a => a.IdJurisdiccion == idJurisdiccion)
            .MaxAsync(a => (int?)a.Numero);

        return ReglasNumeracionAsiento.ProximoNumero(maximo);
    }

    // ---- Auxiliares ------------------------------------------------------

    private ConfiguracionCierre ConfiguracionDelMunicipio() => new()
    {
        CuentaBalanceo = AppSettings.Contabilidad.CuentaBalanceoCierre,
        UnidadAdministrativa = AppSettings.Contabilidad.UnidadAdministrativaCierre,
        RecursoBalanceoHaber = AppSettings.Contabilidad.RecursoBalanceoHaber,
        RecursoBalanceoDebe = AppSettings.Contabilidad.RecursoBalanceoDebe,
        NaturalezasResultado = AppSettings.Contabilidad.NaturalezasResultado,
        NaturalezasPatrimoniales = AppSettings.Contabilidad.NaturalezasPatrimoniales,
        TipoAsientoCierre = AppSettings.Contabilidad.TipoAsientoCierre,
        TipoAsientoApertura = AppSettings.Contabilidad.TipoAsientoApertura,
    };

    private async Task<ContextoAsiento> ArmarContextoDeCabecera(DateTime? fecContable, int idUnidadAdm, int? idTipoAsiento)
    {
        if (fecContable is null) return new ContextoAsiento();

        var vigencia = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Ejercicio == fecContable.Value.Year);

        var jurisdiccion = vigencia is null
            ? null
            : await JurisdiccionDeUnidad(idUnidadAdm, vigencia.IdVigencia);

        var tipoExiste = idTipoAsiento is > 0
                         && await Existe(_ejecucionContext.TTiposAsiento.Where(t => t.IdTipoAsiento == idTipoAsiento));

        var cerrado = vigencia is not null
                      && await TieneAsientoDeTipo(vigencia.IdVigencia, AppSettings.Contabilidad.TipoAsientoCierre);

        return new ContextoAsiento(vigencia?.IdVigencia, vigencia?.Ejercicio, jurisdiccion, tipoExiste, cerrado);
    }

    /// <summary>
    /// Resuelve la jurisdiccion de una unidad administrativa dentro de una vigencia, traduciendo
    /// por NUMERO_UNIDAD_ADM, que es lo estable entre ejercicios.
    /// </summary>
    private async Task<int?> JurisdiccionDeUnidad(int idUnidadAdm, int idVigencia)
    {
        if (idUnidadAdm <= 0) return null;

        var numero = await _presupuestoContext.TUnidadesAdministrativa
            .AsNoTracking()
            .Where(u => u.IdUnidadAdm == idUnidadAdm)
            .Select(u => u.NumeroUnidadAdm)
            .FirstOrDefaultAsync();

        if (numero is null) return null;

        return await _presupuestoContext.TUnidadesAdministrativa
            .AsNoTracking()
            .Where(u => u.NumeroUnidadAdm == numero && u.IdVigencia == idVigencia)
            .Select(u => u.IdJurisdiccion)
            .FirstOrDefaultAsync();
    }

    private async Task<int?> VigenciaDelAnio(int anio)
        => await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.Ejercicio == anio)
            .Select(v => (int?)v.IdVigencia)
            .FirstOrDefaultAsync();

    private async Task<bool> TieneAsientoDeTipo(int idVigencia, int idTipoAsiento)
        => idTipoAsiento > 0
           && await Existe(_ejecucionContext.TAsientos
               .Where(a => a.IdVigencia == idVigencia && a.IdTipoAsiento == idTipoAsiento));

    /// <summary>
    /// Saldos por cuenta del ejercicio, con la misma formula que la vista VT_SALDO_CTACONTABLE:
    /// saldo absoluto de la diferencia y tipo segun el signo.
    /// </summary>
    /// <remarks>
    /// Los renglones anulados se excluyen o no segun la configuracion. La vista original los
    /// incluye (defecto D14); el modulo los excluye por defecto y la desviacion queda documentada.
    /// </remarks>
    private async Task<List<SaldoDeCuenta>> SaldosDelEjercicio(int idVigencia)
    {
        var incluirAnulados = AppSettings.Contabilidad.IncluirAnuladosEnSaldos;

        var movimientos = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            where a.IdVigencia == idVigencia
                  && (incluirAnulados || d.Anulado == null || d.Anulado != Anulado)
            group new { d.Debe, d.Haber } by d.IdCtaContable into g
            select new
            {
                IdCtaContable = g.Key,
                Debe = g.Sum(x => x.Debe ?? 0),
                Haber = g.Sum(x => x.Haber ?? 0),
            }).ToListAsync();

        var cuentas = await CuentasPorId(movimientos.Select(m => m.IdCtaContable));

        return
        [
            .. movimientos
                .Select(m =>
                {
                    var diferencia = ReglasBalanceAsiento.Redondear(m.Debe - m.Haber);
                    var cuenta = cuentas.GetValueOrDefault(m.IdCtaContable);

                    return new SaldoDeCuenta(
                        m.IdCtaContable,
                        Math.Abs(diferencia),
                        diferencia >= 0 ? "D" : "H",
                        cuenta?.IdNaturaleza,
                        cuenta?.IdRecursoDebe,
                        cuenta?.IdRecursoHaber);
                })
                .Where(s => s.Saldo != 0)
        ];
    }

    private async Task<List<RenglonDeCierre>> RenglonesDelCierrePatrimonial(int idVigencia, ConfiguracionCierre config)
    {
        var renglones = await (
            from d in _ejecucionContext.TAsientosDetalle.AsNoTracking()
            join a in _ejecucionContext.TAsientos.AsNoTracking() on d.IdAsiento equals a.IdAsiento
            join c in _ejecucionContext.TCuentasContables.AsNoTracking() on d.IdCtaContable equals c.IdCtaContable
            where a.IdVigencia == idVigencia
                  && a.IdTipoAsiento == config.TipoAsientoCierre
                  && c.IdNaturaleza != null
                  && config.NaturalezasPatrimoniales.Contains(c.IdNaturaleza.Value)
            select new RenglonDeCierre(d.IdCtaContable, d.Debe, d.Haber, c.IdRecursoDebe, c.IdRecursoHaber))
            .ToListAsync();

        return renglones;
    }

    private async Task<List<TAsientoDetalle>> RenglonesDe(int idAsiento)
        => await _ejecucionContext.TAsientosDetalle
            .AsNoTracking()
            .Where(d => d.IdAsiento == idAsiento)
            .OrderBy(d => d.IdAsientoDet)
            .ToListAsync();

    private async Task<Dictionary<int, TCuentaContable>> CuentasPorId(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();

        return await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => lista.Contains(c.IdCtaContable))
            .ToDictionaryAsync(c => c.IdCtaContable);
    }

    /// <summary>
    /// Controla que cada renglon apunte a una cuenta que pueda recibir movimientos.
    /// </summary>
    private static Validacion ValidarImputacion(
        List<RenglonRequest> renglones,
        Dictionary<int, TCuentaContable> cuentas)
    {
        var validacion = new Validacion();

        for (var i = 0; i < renglones.Count; i++)
        {
            var numero = i + 1;
            var id = renglones[i].IdCtaContable;

            if (!cuentas.TryGetValue(id, out var cuenta))
            {
                validacion.Agregar($"El renglon {numero} apunta a una cuenta contable que no existe.");
                continue;
            }

            validacion.AgregarSi(
                !cuenta.EsImputable,
                $"El renglon {numero} imputa a {cuenta.Codigo}, que no es una cuenta imputable.");

            validacion.AgregarSi(
                !cuenta.EstaActiva,
                $"El renglon {numero} imputa a {cuenta.Codigo}, que esta dada de baja.");
        }

        return validacion;
    }


    /// <summary>Ver la nota de PlanDeCuentasService: Any() no se puede usar contra Oracle 12.2.</summary>
    private static async Task<bool> Existe<TEntidad>(IQueryable<TEntidad> consulta)
        => await consulta.Take(1).CountAsync() > 0;
}
