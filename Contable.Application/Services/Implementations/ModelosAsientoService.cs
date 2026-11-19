using System.Data;
using Contable.Application.RequestDto.ModelosAsiento;
using Contable.Application.ResponseDto.Dtos.ModelosAsiento;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Parametrizacion de modelos de asiento contable.
/// </summary>
/// <remarks>
/// La plantilla es fija; aca solo se arma la combinacion, se valida y se persiste.
/// El modelo es lo que el usuario parametriza: una combinacion, a lo sumo un modelo activo, y una cuenta por renglon de tipo PLAN_CUENTAS.
/// </remarks>
public class ModelosAsientoService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IModelosAsientoService
{
    private const string SecuenciaModelo = "EJECUCION.SQ_T_MODELOS_ASIENTO";
    private const string SecuenciaModeloDet = "EJECUCION.SQ_T_MODELOS_ASIENTO_DET";
    private const string SecuenciaHistorial = "EJECUCION.SQ_T_HISTORIALES_MODELOS_ASIENTO";

    private const string Si = ReglasModelosAsiento.Si;

    // ---- Catalogos del formulario ----------------------------------------

    public async Task<OperationResponse<List<CatalogoItemResponse>>> GetTipoOperaciones()
    {
        var operaciones = await _ejecucionContext.TTiposOperacionAsiento
            .AsNoTracking()
            .Where(o => o.FecBaja == null)
            .OrderBy(o => o.Orden)
            .ThenBy(o => o.NTipoOperacion)
            .Select(o => new CatalogoItemResponse
            {
                Id = o.IdTipoOperacion,
                Descripcion = o.NTipoOperacion,
            })
            .ToListAsync();

        return Ok(operaciones);
    }

    public async Task<OperationResponse<List<TipoDocumentoCatalogoResponse>>> GetTipoDocumentos()
    {
        var hoy = DateTime.Today;

        var documentos = await _ejecucionContext.TTiposDocumento
            .AsNoTracking()
            .Where(d => (d.Baja == null || d.Baja != Si)
                        && (d.FechaHasta == null || d.FechaHasta >= hoy))
            .OrderBy(d => d.Descripcion)
            .Select(d => new { d.IdTipodoc, d.Descripcion })
            .ToListAsync();

        var ids = documentos.Select(d => d.IdTipodoc).ToList();
        var porDocumento = new Dictionary<int, List<TipoComprobanteResponse>>();

        if (ids.Count > 0)
        {
            var letras = await _ejecucionContext.TTiposDocumentoLetra
                .AsNoTracking()
                .Where(l => ids.Contains(l.IdTipoDoc) && l.Activo == 1 && l.FecBaja == null)
                .OrderBy(l => l.Codigo)
                .Select(l => new
                {
                    l.IdTipoDoc,
                    l.IdTipodocLetra,
                    l.Codigo,
                })
                .ToListAsync();

            porDocumento = letras
                .GroupBy(l => l.IdTipoDoc)
                .ToDictionary(g => g.Key, g => g.Select(l => new TipoComprobanteResponse
                {
                    IdTipodocLetra = l.IdTipodocLetra,
                    Codigo = l.Codigo,
                }).ToList());
        }

        var respuesta = documentos.Select(d =>
        {
            var delDocumento = porDocumento.GetValueOrDefault(d.IdTipodoc);

            return new TipoDocumentoCatalogoResponse
            {
                Id = d.IdTipodoc,
                Descripcion = d.Descripcion,
                TieneLetras = delDocumento is not null,
                Letras = delDocumento,
            };
        }).ToList();

        return Ok(respuesta);
    }

    public async Task<OperationResponse<List<TipoComprobanteResponse>>> GetLetras(int idTipodoc)
    {
        var letras = await _ejecucionContext.TTiposDocumentoLetra
            .AsNoTracking()
            .Where(l => l.IdTipoDoc == idTipodoc && l.Activo == 1 && l.FecBaja == null)
            .OrderBy(l => l.Codigo)
            .Select(l => new TipoComprobanteResponse
            {
                IdTipodocLetra = l.IdTipodocLetra,
                Codigo = l.Codigo,
            })
            .ToListAsync();

        return Ok(letras);
    }

    public async Task<OperationResponse<List<CuentaModeloResponse>>> BuscarCuentas(FiltroCuentasModeloRequest filtro)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamano = filtro.TamanoPagina is < 1 or > 200 ? 20 : filtro.TamanoPagina;

        var consulta = _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => c.Activa == ReglasJerarquiaCuentas.CuentaActiva
                        && c.Imputable == ReglasJerarquiaCuentas.MarcaSi);

        var jurisdiccion = GetJurisdiccionCache();
        if (jurisdiccion > 0) consulta = consulta.Where(c => c.IdJurisdiccion == jurisdiccion);

        var vigencia = GetIdVigencia();
        if (vigencia > 0) consulta = consulta.Where(c => c.IdVigencia == vigencia);

        if (!string.IsNullOrWhiteSpace(filtro.Codigo))
        {
            var codigo = filtro.Codigo.Trim();
            consulta = consulta.Where(c => c.Codigo.StartsWith(codigo));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.Trim().ToUpper();
            consulta = consulta.Where(c => c.Nombre.ToUpper().Contains(nombre));
        }

        var total = await consulta.CountAsync();

        var cuentas = await consulta
            .OrderBy(c => c.Codigo)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(c => new CuentaModeloResponse
            {
                IdCtaContable = c.IdCtaContable,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                TipoSaldo = c.TipoSaldo,
            })
            .ToListAsync();

        return Paginado(cuentas, total);
    }

    // ---- Consulta general y particular -----------------------------------

    public async Task<OperationResponse<List<ModeloAsientoResumenResponse>>> Buscar(FiltroModelosAsientoRequest filtro)
    {
        var consulta = _ejecucionContext.TModelosAsiento
            .AsNoTracking()
            .Where(m => m.FecBaja == null);

        if (filtro.IdTipoOperacion is > 0)
            consulta = consulta.Where(m => m.Plantilla!.IdTipoOperacion == filtro.IdTipoOperacion.Value);

        if (filtro.IdTipodoc is > 0)
            consulta = consulta.Where(m => m.Plantilla!.IdTipodoc == filtro.IdTipodoc.Value);

        if (filtro.IdTipodocLetra is > 0)
            consulta = consulta.Where(m => m.Plantilla!.IdTipodocLetra == filtro.IdTipodocLetra.Value);

        if (filtro.DiscriminaOt is not null)
        {
            var marca = filtro.DiscriminaOt.Value ? Si : ReglasModelosAsiento.No;
            consulta = consulta.Where(m => m.Plantilla!.DiscriminaOt == marca);
        }

        var filas = await consulta
            .Select(m => new
            {
                m.IdModeloAsiento,
                IdPlantillaAsiento = m.IdPlantillaAsiento ?? 0,
                IdTipoOperacion = m.Plantilla!.IdTipoOperacion ?? 0,
                IdTipodoc = m.Plantilla!.IdTipodoc ?? 0,
                IdTipodocLetra = m.Plantilla!.IdTipodocLetra,
                DiscriminaOt = m.Plantilla!.DiscriminaOt,
                Operacion = _ejecucionContext.TTiposOperacionAsiento
                    .Where(o => o.IdTipoOperacion == m.Plantilla!.IdTipoOperacion)
                    .Select(o => o.NTipoOperacion)
                    .FirstOrDefault(),
                TipoDocumento = m.Plantilla!.TipoDocumento!.Descripcion,
                TipoComprobante = m.Plantilla!.TipoDocumentoLetra!.Codigo,
                CantidadDeRenglones = _ejecucionContext.TPlantillasAsientoDetalle
                    .Count(pd => pd.IdPlantillaAsiento == m.IdPlantillaAsiento && pd.FecBaja == null),
                UsrIng = m.UsrIng,
                FecIng = m.FecIng ?? default,
            })
            .ToListAsync();

        var modelos = filas.Select(f => new ModeloAsientoResumenResponse
        {
            IdModeloAsiento = f.IdModeloAsiento,
            IdPlantillaAsiento = f.IdPlantillaAsiento,
            IdTipoOperacion = f.IdTipoOperacion,
            IdTipodoc = f.IdTipodoc,
            IdTipodocLetra = f.IdTipodocLetra,
            DiscriminaOt = f.DiscriminaOt == Si,
            Operacion = f.Operacion,
            TipoDocumento = f.TipoDocumento,
            TipoComprobante = f.TipoComprobante,
            CantidadDeRenglones = f.CantidadDeRenglones,
            UsrIng = f.UsrIng,
            FecIng = f.FecIng,
        }).ToList();

        var ordenados = modelos
            .OrderBy(m => m.Operacion, StringComparer.OrdinalIgnoreCase)
            .ThenBy(m => m.TipoDocumento, StringComparer.OrdinalIgnoreCase)
            .ThenBy(m => m.TipoComprobante, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(ordenados, ordenados.Count);
    }

    public async Task<OperationResponse<ModeloAsientoResponse>> ObtenerPlantilla(ObtenerPlantillaRequest request)
    {
        var datos = DatosDe(request);

        var contexto = await ContextoDe(datos, combinacionYaExiste: false);
        var validacion = ReglasModelosAsiento.ValidarCombinacion(datos, contexto);

        if (!validacion.EsValida) return BadRequest<ModeloAsientoResponse>(validacion.Mensaje);

        var plantilla = await BuscarPlantilla(datos);

        if (plantilla is null) return BadRequest<ModeloAsientoResponse>(ReglasModelosAsiento.MensajeSinPlantilla);

        return Ok(await ArmarDetalle(plantilla, modelo: null));
    }

    public async Task<OperationResponse<ModeloAsientoResponse>> ObtenerModelo(int idModeloAsiento)
    {
        var modelo = await ModeloActivo(idModeloAsiento);

        if (modelo is null) return NotFound<ModeloAsientoResponse>("El modelo de asiento no existe.");

        var plantilla = await _ejecucionContext.TPlantillasAsiento
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPlantillaAsiento == modelo.IdPlantillaAsiento);

        if (plantilla is null) return NotFound<ModeloAsientoResponse>("La plantilla del modelo no existe.");

        return Ok(await ArmarDetalle(plantilla, modelo));
    }

    // ---- Guardar, clonar y editar ----------------------------------------

    public async Task<OperationResponse<ModeloAsientoResponse>> Guardar(GuardarModeloAsientoRequest request)
    {
        var datos = DatosDe(request);
        var plantilla = await BuscarPlantilla(datos);

        if (plantilla is null) return BadRequest<ModeloAsientoResponse>(ReglasModelosAsiento.MensajeSinPlantilla);

        var duplicado = await Existe(_ejecucionContext.TModelosAsiento
            .Where(m => m.FecBaja == null && m.IdPlantillaAsiento == plantilla.IdPlantillaAsiento));

        var contexto = await ContextoDe(datos, duplicado);
        var validacion = ReglasModelosAsiento.ValidarCombinacion(datos, contexto);

        if (!validacion.EsValida) return BadRequest<ModeloAsientoResponse>(validacion.Mensaje);

        var renglones = await RenglonesDe(plantilla.IdPlantillaAsiento);
        var porRenglon = request.Cuentas.ToDictionary(c => c.IdPlantillaAsientoDetalle, c => c.IdCtaContable);
        var cuentas = await CuentasPorId(porRenglon.Values.Where(id => id is > 0).Select(id => id!.Value));

        var paraValidar = renglones.Select(r => new ReglasModelosAsiento.RenglonParaValidacion(
            r.IdPlantillaAsientoDetalle,
            r.OrigenCuenta ?? string.Empty,
            r.Concepto ?? string.Empty,
            r.EsPlanCuentas ? porRenglon.GetValueOrDefault(r.IdPlantillaAsientoDetalle) : null));

        var validacionCuentas = ReglasModelosAsiento.ValidarCuentas(paraValidar, cuentas);

        if (!validacionCuentas.EsValida) return BadRequest<ModeloAsientoResponse>(validacionCuentas.Mensaje);

        var usuario = UsuarioActual();
        var ahora = DateTime.Now;

        var estrategia = _ejecucionContext.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _ejecucionContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var modelo = new TModeloAsiento
            {
                IdModeloAsiento = await GetSecuencia(_ejecucionContext, SecuenciaModelo),
                IdPlantillaAsiento = plantilla.IdPlantillaAsiento,
                UsrIng = usuario,
                FecIng = ahora,
            };

            _ejecucionContext.TModelosAsiento.Add(modelo);

            foreach (var renglon in renglones.Where(r => r.EsPlanCuentas))
            {
                _ejecucionContext.TModelosAsientoDetalle.Add(new TModeloAsientoDetalle
                {
                    IdModeloAsientoDetalle = await GetSecuencia(_ejecucionContext, SecuenciaModeloDet),
                    IdModeloAsiento = modelo.IdModeloAsiento,
                    IdPlantillaAsientoDetalle = renglon.IdPlantillaAsientoDetalle,
                    IdCtaContable = porRenglon[renglon.IdPlantillaAsientoDetalle]!.Value,
                    UsrIng = usuario,
                    FecIng = ahora,
                });
            }

            _ejecucionContext.THistorialesModelosAsiento.Add(new THistorialModeloAsiento
            {
                IdHistorialModeloAsiento = await GetSecuencia(_ejecucionContext, SecuenciaHistorial),
                IdModeloAsiento = modelo.IdModeloAsiento,
                Accion = ReglasModelosAsiento.AccionGenerar,
                Comentarios = ReglasModelosAsiento.FormatearComentarioGenerar(),
                UsrIng = usuario,
                FecIng = ahora,
            });

            await _ejecucionContext.SaveChangesAsync();
            await transaccion.CommitAsync();

            return await ObtenerModelo(modelo.IdModeloAsiento);
        });
    }

    public async Task<OperationResponse<ModeloAsientoResponse>> Editar(
        int idModeloAsiento,
        EditarModeloAsientoRequest request)
    {
        var modelo = await ModeloActivo(idModeloAsiento);

        if (modelo is null) return NotFound<ModeloAsientoResponse>("El modelo de asiento no existe.");

        var plantilla = await _ejecucionContext.TPlantillasAsiento
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPlantillaAsiento == modelo.IdPlantillaAsiento);

        if (plantilla is null) return NotFound<ModeloAsientoResponse>("La plantilla del modelo no existe.");

        var renglones = await RenglonesDe(plantilla.IdPlantillaAsiento);

        var detalles = (await _ejecucionContext.TModelosAsientoDetalle
                .AsNoTracking()
                .Where(d => d.IdModeloAsiento == modelo.IdModeloAsiento && d.FecBaja == null)
                .ToListAsync())
            .Where(d => d.IdPlantillaAsientoDetalle is not null).ToDictionary(d => d.IdPlantillaAsientoDetalle!.Value);

        var porRenglon = request.Cuentas.ToDictionary(c => c.IdPlantillaAsientoDetalle, c => c.IdCtaContable);
        var idsParticipantes = porRenglon.Values.Where(id => id is > 0).Select(id => id!.Value)
            .Concat(detalles.Values.Where(d => d.IdCtaContable is not null).Select(d => d.IdCtaContable!.Value))
            .Distinct();
        var cuentas = await CuentasPorId(idsParticipantes);

        var paraValidar = renglones.Select(r => new ReglasModelosAsiento.RenglonParaValidacion(
            r.IdPlantillaAsientoDetalle,
            r.OrigenCuenta ?? string.Empty,
            r.Concepto ?? string.Empty,
            r.EsPlanCuentas ? porRenglon.GetValueOrDefault(r.IdPlantillaAsientoDetalle) : null));

        var validacion = ReglasModelosAsiento.ValidarCuentas(paraValidar, cuentas);

        if (!validacion.EsValida) return BadRequest<ModeloAsientoResponse>(validacion.Mensaje);

        var cambios = new List<ReglasModelosAsiento.CambioDeCuenta>();

        foreach (var renglon in renglones.Where(r => r.EsPlanCuentas))
        {
            var anterior = detalles.GetValueOrDefault(renglon.IdPlantillaAsientoDetalle);
            var nuevoId = porRenglon.GetValueOrDefault(renglon.IdPlantillaAsientoDetalle);

            if (anterior?.IdCtaContable == nuevoId) continue;

            cambios.Add(new ReglasModelosAsiento.CambioDeCuenta(
                renglon.Concepto ?? string.Empty,
                anterior is null ? "(sin cuenta)" : cuentas.GetValueOrDefault(anterior.IdCtaContable ?? 0)?.Etiqueta ?? "(sin cuenta)",
                nuevoId is null ? "(sin cuenta)" : cuentas.GetValueOrDefault(nuevoId.Value)?.Etiqueta ?? "(sin cuenta)"));
        }

        var usuario = UsuarioActual();
        var ahora = DateTime.Now;

        var estrategia = _ejecucionContext.Database.CreateExecutionStrategy();

        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await _ejecucionContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var grabables = await _ejecucionContext.TModelosAsientoDetalle
                .Where(d => d.IdModeloAsiento == modelo.IdModeloAsiento)
                .ToListAsync();

            foreach (var renglon in renglones.Where(r => r.EsPlanCuentas))
            {
                var idCuenta = porRenglon[renglon.IdPlantillaAsientoDetalle]!.Value;
                var actual = grabables.FirstOrDefault(d => d.IdPlantillaAsientoDetalle == renglon.IdPlantillaAsientoDetalle);

                if (actual is null)
                {
                    _ejecucionContext.TModelosAsientoDetalle.Add(new TModeloAsientoDetalle
                    {
                        IdModeloAsientoDetalle = await GetSecuencia(_ejecucionContext, SecuenciaModeloDet),
                        IdModeloAsiento = modelo.IdModeloAsiento,
                        IdPlantillaAsientoDetalle = renglon.IdPlantillaAsientoDetalle,
                        IdCtaContable = idCuenta,
                        UsrIng = usuario,
                        FecIng = ahora,
                    });
                }
                else
                {
                    actual.IdCtaContable = idCuenta;
                    actual.UsrMod = usuario;
                    actual.FecMod = ahora;
                }
            }

            var idsValidos = renglones.Where(r => r.EsPlanCuentas)
                .Select(r => r.IdPlantillaAsientoDetalle)
                .ToHashSet();
            _ejecucionContext.TModelosAsientoDetalle.RemoveRange(
                grabables.Where(d => d.IdPlantillaAsientoDetalle is not int id || !idsValidos.Contains(id)));

            modelo.UsrMod = usuario;
            modelo.FecMod = ahora;

            _ejecucionContext.THistorialesModelosAsiento.Add(new THistorialModeloAsiento
            {
                IdHistorialModeloAsiento = await GetSecuencia(_ejecucionContext, SecuenciaHistorial),
                IdModeloAsiento = modelo.IdModeloAsiento,
                Accion = ReglasModelosAsiento.AccionModificar,
                Comentarios = ReglasModelosAsiento.FormatearComentarioModificar(cambios),
                UsrIng = usuario,
                FecIng = ahora,
            });

            await _ejecucionContext.SaveChangesAsync();
            await transaccion.CommitAsync();

            return await ObtenerModelo(modelo.IdModeloAsiento);
        });
    }

    // ---- Historial --------------------------------------------------------

    public async Task<OperationResponse<List<HistorialModeloResponse>>> Historial(int idModeloAsiento)
    {
        if (!await Existe(_ejecucionContext.TModelosAsiento
                .Where(m => m.IdModeloAsiento == idModeloAsiento && m.FecBaja == null)))
        {
            return NotFound<List<HistorialModeloResponse>>("El modelo de asiento no existe.");
        }

        var filas = await _ejecucionContext.THistorialesModelosAsiento
            .AsNoTracking()
            .Where(h => h.IdModeloAsiento == idModeloAsiento)
            .OrderByDescending(h => h.FecIng)
            .ThenByDescending(h => h.IdHistorialModeloAsiento)
            .Select(h => new
            {
                h.IdHistorialModeloAsiento,
                h.FecIng,
                h.UsrIng,
                h.Accion,
                h.Comentarios,
            })
            .ToListAsync();

        var historial = filas.Select(h => new HistorialModeloResponse
        {
            IdHistorialModeloAsiento = h.IdHistorialModeloAsiento,
            FechaHs = h.FecIng ?? default,
            Usuario = h.UsrIng ?? string.Empty,
            Accion = NombreDeLaAccion(h.Accion ?? string.Empty),
            Comentarios = h.Comentarios ?? string.Empty,
        }).ToList();

        return Ok(historial);
    }

    // ---- Auxiliares -------------------------------------------------------

    private static ReglasModelosAsiento.DatosCombinacion DatosDe(DatosCombinacionRequest request)
        => new(request.IdTipoOperacion, request.IdTipodoc, request.IdTipodocLetra, request.DiscriminaOt);

    private async Task<ReglasModelosAsiento.ContextoCombinacion> ContextoDe(
        ReglasModelosAsiento.DatosCombinacion datos,
        bool combinacionYaExiste)
    {
        var hoy = DateTime.Today;

        var operacionExiste = datos.IdTipoOperacion is > 0
                              && await Existe(_ejecucionContext.TTiposOperacionAsiento
                                  .Where(o => o.IdTipoOperacion == datos.IdTipoOperacion
                                              && o.FecBaja == null));

        var documentoExiste = datos.IdTipodoc is > 0
                              && await Existe(_ejecucionContext.TTiposDocumento
                                  .Where(d => d.IdTipodoc == datos.IdTipodoc
                                              && (d.Baja == null || d.Baja != Si)
                                              && (d.FechaHasta == null || d.FechaHasta >= hoy)));

        var tieneLetras = datos.IdTipodoc is > 0
                          && await _ejecucionContext.TTiposDocumentoLetra
                              .CountAsync(l => l.IdTipoDoc == datos.IdTipodoc && l.Activo == 1 && l.FecBaja == null) > 0;

        var letraExiste = datos.IdTipodocLetra is > 0
                          && await Existe(_ejecucionContext.TTiposDocumentoLetra
                              .Where(l => l.IdTipodocLetra == datos.IdTipodocLetra
                                          && l.IdTipoDoc == datos.IdTipodoc
                                          && l.Activo == 1
                                          && l.FecBaja == null));

        return new ReglasModelosAsiento.ContextoCombinacion(
            operacionExiste, documentoExiste, letraExiste, tieneLetras, combinacionYaExiste);
    }

    private async Task<TPlantillaAsiento?> BuscarPlantilla(ReglasModelosAsiento.DatosCombinacion datos)
        => await _ejecucionContext.TPlantillasAsiento
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.FecBaja == null
                                      && p.IdTipoOperacion == datos.IdTipoOperacion
                                      && p.IdTipodoc == datos.IdTipodoc
                                      && p.IdTipodocLetra == datos.IdTipodocLetra
                                      && p.DiscriminaOt == (datos.DiscriminaOt ? Si : ReglasModelosAsiento.No));

    private async Task<TModeloAsiento?> ModeloActivo(int idModeloAsiento)
        => await _ejecucionContext.TModelosAsiento
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdModeloAsiento == idModeloAsiento && m.FecBaja == null);

    private async Task<List<TPlantillaAsientoDetalle>> RenglonesDe(int idPlantillaAsiento)
        => await _ejecucionContext.TPlantillasAsientoDetalle
            .AsNoTracking()
            .Where(d => d.IdPlantillaAsiento == idPlantillaAsiento && d.FecBaja == null)
            .OrderBy(d => d.Orden)
            .ToListAsync();

    private async Task<Dictionary<int, ReglasModelosAsiento.CuentaParaValidacion>> CuentasPorId(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();

        return await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => lista.Contains(c.IdCtaContable))
            .Select(c => new ReglasModelosAsiento.CuentaParaValidacion(
                c.IdCtaContable, c.Codigo, c.Nombre, c.EstaActiva, c.EsImputable))
            .ToDictionaryAsync(c => c.IdCtaContable);
    }

    /// <summary>Arma el detalle comun a ObtenerPlantilla y ObtenerModelo.</summary>
    private async Task<ModeloAsientoResponse> ArmarDetalle(TPlantillaAsiento plantilla, TModeloAsiento? modelo)
    {
        var renglones = await RenglonesDe(plantilla.IdPlantillaAsiento);

        var detalles = modelo is null
            ? new Dictionary<int, TModeloAsientoDetalle>()
            : (await _ejecucionContext.TModelosAsientoDetalle
                .AsNoTracking()
                .Where(d => d.IdModeloAsiento == modelo.IdModeloAsiento && d.FecBaja == null)
                .ToListAsync())
              .Where(d => d.IdPlantillaAsientoDetalle is not null).ToDictionary(d => d.IdPlantillaAsientoDetalle!.Value);

        var cuentas = await CuentasPorId(detalles.Values.Where(d => d.IdCtaContable is not null).Select(d => d.IdCtaContable!.Value));

        var operacion = await _ejecucionContext.TTiposOperacionAsiento
            .AsNoTracking()
            .Where(o => o.IdTipoOperacion == plantilla.IdTipoOperacion)
            .Select(o => o.NTipoOperacion)
            .FirstOrDefaultAsync();

        var documento = await _ejecucionContext.TTiposDocumento
            .AsNoTracking()
            .Where(d => d.IdTipodoc == plantilla.IdTipodoc)
            .Select(d => d.Descripcion)
            .FirstOrDefaultAsync();

        var letra = plantilla.IdTipodocLetra is null
            ? null
            : await _ejecucionContext.TTiposDocumentoLetra
                .AsNoTracking()
                .Where(l => l.IdTipodocLetra == plantilla.IdTipodocLetra)
                .Select(l => l.Codigo)
                .FirstOrDefaultAsync();

        return new ModeloAsientoResponse
        {
            IdModeloAsiento = modelo?.IdModeloAsiento ?? 0,
            IdPlantillaAsiento = plantilla.IdPlantillaAsiento,
            IdTipoOperacion = plantilla.IdTipoOperacion ?? 0,
            Operacion = operacion,
            IdTipodoc = plantilla.IdTipodoc ?? 0,
            TipoDocumento = documento,
            IdTipodocLetra = plantilla.IdTipodocLetra,
            TipoComprobante = letra,
            DiscriminaOt = plantilla.DiscriminaOtrosTributos,
            CantidadDeRenglones = renglones.Count,
            UsrIng = modelo?.UsrIng,
            FecIng = modelo?.FecIng ?? default,
            Formula = ReglasModelosAsiento.TextoFormula,
            Renglones =
            [
                .. renglones.Select(r =>
                {
                    var detalle = detalles.GetValueOrDefault(r.IdPlantillaAsientoDetalle);
                    var cuenta = detalle is null ? null : cuentas.GetValueOrDefault(detalle.IdCtaContable ?? 0);

                    return new RenglonPlantillaResponse
                    {
                        IdPlantillaAsientoDetalle = r.IdPlantillaAsientoDetalle,
                        Orden = r.Orden ?? 0,
                        OrigenCuenta = r.OrigenCuenta ?? string.Empty,
                        Concepto = r.Concepto ?? string.Empty,
                        CuentaContable = r.EsMatrizContable
                            ? ReglasModelosAsiento.TextoDefinidaEnMatriz
                            : cuenta?.Etiqueta ?? string.Empty,
                        Debita = r.Debita == Si,
                        Acredita = r.Acredita == Si,
                        BaseCalculo = r.BaseCalculo,
                        IdCtaContable = r.EsMatrizContable ? null : detalle?.IdCtaContable,
                        CodigoCuenta = cuenta?.Codigo,
                        NombreCuenta = cuenta?.Nombre,
                        Tooltip = r.EsMatrizContable ? ReglasModelosAsiento.TooltipMatriz : null,
                    };
                })
            ],
        };
    }

    /// <summary>
    /// En desarrollo no hay token y GetUserCache() devuelve vacio. Oracle guarda '' como NULL y
    /// los campos de auditoria son NOT NULL, asi que hace falta un valor de respaldo.
    /// </summary>
    private string UsuarioActual()
    {
        var usuario = GetUserCache();
        return string.IsNullOrWhiteSpace(usuario) ? "sistema" : usuario;
    }

    private static string NombreDeLaAccion(string accion) => accion switch
    {
        ReglasModelosAsiento.AccionGenerar => "Generar",
        ReglasModelosAsiento.AccionModificar => "Modificar",
        _ => accion,
    };

    private static async Task<bool> Existe<TEntidad>(IQueryable<TEntidad> consulta)
        => await consulta.Take(1).CountAsync() > 0;
}
