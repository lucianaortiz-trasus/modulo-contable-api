using System.Text.Json;
using Dapper;
using Contable.Application.RequestDto.MatrizContable;
using Contable.Application.ResponseDto.Dtos.MatrizContable;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Matriz contable: que cuenta recibe el movimiento de cada partida presupuestaria.
/// </summary>
/// <remarks>
/// Reemplaza a PKG_T_MATRIZ_CONTABLE. Ademas de los defectos que corrigen las reglas de dominio
/// (D9 a D12), evita el de BUSCAR_DATOS_MATRIZ: esa consulta armaba el SQL concatenando texto y
/// solo agregaba las condiciones de join **dentro del if de la vigencia**, con lo que al buscar
/// sin ejercicio el FROM quedaba con cuatro tablas sin relacionar y producia un producto
/// cartesiano. Aca los joins son parte de la consulta, no de una rama opcional.
/// </remarks>
public class MatrizContableService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IMatrizContableService
{
    private const string Secuencia = "EJECUCION.SEQ_MATRIZ_CBLE";

    public async Task<OperationResponse<List<MatrizContableResponse>>> Buscar(FiltroMatrizRequest filtro)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamano = filtro.TamanoPagina is < 1 or > 200 ? 20 : filtro.TamanoPagina;

        var (ordenadas, _) = await ObtenerFilasFiltradas(filtro);

        var total = ordenadas.Count;
        var paged = ordenadas
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToList();

        return Paginado(paged, total);
    }

    public async Task<OperationResponse<ExportacionMatrizResponse>> Exportar(FiltroMatrizRequest filtro)
    {
        var (ordenadas, vigencia) = await ObtenerFilasFiltradas(filtro);
        var pendientes = await ObtenerRelacionesPendientes(vigencia);

        var ejercicio = ordenadas.FirstOrDefault()?.Ejercicio;
        if (ejercicio is null && vigencia > 0)
        {
            ejercicio = await _presupuestoContext.TVigencias
                .AsNoTracking()
                .Where(v => v.IdVigencia == vigencia)
                .Select(v => (int?)v.Ejercicio)
                .FirstOrDefaultAsync();
        }

        var apellidoYNombre = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(x => x.Type == "ApellidoYNombre")?.Value;
        var usuario = !string.IsNullOrWhiteSpace(apellidoYNombre) ? apellidoYNombre : GetUserCache();

        return Ok(new ExportacionMatrizResponse
        {
            Ejercicio = ejercicio,
            Usuario = usuario,
            MatrizContable = ordenadas,
            RelacionesPendientes = pendientes.RelacionesPendientes,
        });
    }

    private async Task<(List<MatrizContableResponse> Filas, int Vigencia)> ObtenerFilasFiltradas(FiltroMatrizRequest filtro)
    {
        var consulta = _ejecucionContext.TMatrizContable.AsNoTracking();

        var vigencia = await ResolverVigencia(filtro.IdVigencia);
        if (vigencia > 0) consulta = consulta.Where(m => m.IdVigencia == vigencia);

        if (filtro.IdCtaContable is > 0)
        {
            var idsCuentas = await ObtenerDescendientesCuenta(filtro.IdCtaContable.Value, vigencia);
            if (idsCuentas.Count > 0)
            {
                consulta = consulta.Where(m => m.IdCtaContable != null && idsCuentas.Contains(m.IdCtaContable.Value));
            }
        }

        if (filtro.IdObjetoGasto is > 0)
        {
            var idsObjetos = await ObtenerDescendientesObjetoGasto(filtro.IdObjetoGasto.Value, vigencia);
            if (idsObjetos.Count > 0)
            {
                consulta = consulta.Where(m => m.IdObjetoGasto != null && idsObjetos.Contains(m.IdObjetoGasto.Value));
            }
        }

        if (filtro.IdRecurso is > 0)
        {
            var idsRecursos = await ObtenerDescendientesRecurso(filtro.IdRecurso.Value, vigencia);
            if (idsRecursos.Count > 0)
            {
                consulta = consulta.Where(m => m.IdRecurso != null && idsRecursos.Contains(m.IdRecurso.Value));
            }
        }

        if (string.Equals(filtro.TipoPartida, "Gasto", StringComparison.OrdinalIgnoreCase))
        {
            consulta = consulta.Where(m => m.IdObjetoGasto != null);
        }
        else if (string.Equals(filtro.TipoPartida, "Recurso", StringComparison.OrdinalIgnoreCase))
        {
            consulta = consulta.Where(m => m.IdRecurso != null);
        }

        var todasLasFilas = await consulta.ToListAsync();
        var enriquecidas = await Enriquecer(todasLasFilas);

        if (!string.IsNullOrWhiteSpace(filtro.Estado) && !string.Equals(filtro.Estado, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            var activaFiltro = string.Equals(filtro.Estado, "Activa", StringComparison.OrdinalIgnoreCase);
            enriquecidas = enriquecidas.Where(f => f.Activa == activaFiltro).ToList();
        }

        var ordenadas = enriquecidas
            .OrderBy(f => f.CodigoCuenta ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.NombreCuenta ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.TipoPartida ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.NumeroObjeto?.ToString() ?? f.NumeroRecurso?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(f => f.NombreObjeto ?? f.NombreRecurso ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return (ordenadas, vigencia);
    }

    public async Task<OperationResponse<MatrizContableResponse>> GetPorId(int idMatrizCble)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is null) return NotFound<MatrizContableResponse>("La relacion no existe.");

        var enriquecidas = await Enriquecer([fila]);
        return Ok(enriquecidas[0]);
    }

    public async Task<OperationResponse<MatrizContableResponse>> Crear(MatrizContableRequest request)
    {
        var vigencia = await ResolverVigencia(request.IdVigencia);
        var datos = new DatosMatriz(request.IdCtaContable, request.IdObjetoGasto, request.IdRecurso, vigencia);

        var contexto = await ArmarContexto(datos, idMatrizAExcluir: null);
        var validacion = ReglasMatrizContable.ValidarAlta(datos, contexto);

        if (!validacion.EsValida) return BadRequest<MatrizContableResponse>(validacion.Mensaje);

        var fila = new TMatrizContable
        {
            IdMatrizCble = await GetSecuencia(_ejecucionContext, Secuencia),
            IdCtaContable = request.IdCtaContable,
            IdObjetoGasto = request.IdObjetoGasto,
            IdRecurso = request.IdRecurso,
            IdVigencia = vigencia,
            UserIng = GetUserCache(),
            FecIng = DateTime.Now,
        };

        _ejecucionContext.TMatrizContable.Add(fila);
        await _ejecucionContext.SaveChangesAsync();

        var enriquecidas = await Enriquecer([fila]);
        var creada = enriquecidas[0];

        await RegistrarHistorial(
            creada.IdMatrizCble,
            ReglasMatrizContable.AccionGenerar,
            ReglasMatrizContable.FormatearComentarioAlta(
                creada.CodigoCuenta,
                creada.NombreCuenta,
                creada.TipoPartida,
                creada.NumeroObjeto ?? creada.NumeroRecurso,
                creada.NombreObjeto ?? creada.NombreRecurso),
            fila.FecIng);

        return Ok(creada);
    }

    public async Task<OperationResponse<MatrizContableResponse>> Editar(int idMatrizCble, MatrizContableRequest request)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is null) return NotFound<MatrizContableResponse>("La relacion no existe.");

        if (fila.IdCtaContable.HasValue && await TieneAsientosCuenta(fila.IdCtaContable.Value))
        {
            return BadRequest<MatrizContableResponse>(
                "No se puede modificar la relación porque la cuenta contable posee asientos registrados.");
        }

        var vigencia = await ResolverVigencia(request.IdVigencia ?? fila.IdVigencia);
        var datos = new DatosMatriz(request.IdCtaContable, request.IdObjetoGasto, request.IdRecurso, vigencia);

        var contexto = await ArmarContexto(datos, idMatrizAExcluir: idMatrizCble);
        var validacion = ReglasMatrizContable.ValidarEdicion(datos, contexto);

        if (!validacion.EsValida) return BadRequest<MatrizContableResponse>(validacion.Mensaje);

        fila.IdCtaContable = request.IdCtaContable;
        fila.IdObjetoGasto = request.IdObjetoGasto;
        fila.IdRecurso = request.IdRecurso;
        fila.IdVigencia = vigencia;
        fila.UserAct = GetUserCache();
        fila.FecAct = DateTime.Now;

        await _ejecucionContext.SaveChangesAsync();

        var enriquecidas = await Enriquecer([fila]);
        var editada = enriquecidas[0];

        await RegistrarHistorial(
            editada.IdMatrizCble,
            ReglasMatrizContable.AccionModificar,
            ReglasMatrizContable.FormatearComentarioModificacion(
                editada.CodigoCuenta,
                editada.NombreCuenta,
                editada.TipoPartida,
                editada.NumeroObjeto ?? editada.NumeroRecurso,
                editada.NombreObjeto ?? editada.NombreRecurso),
            fila.FecAct);

        return Ok(editada);
    }

    public async Task<OperationResponse<bool>> CuentaTieneAsientos(int idCtaContable)
    {
        var tieneAsientos = await TieneAsientosCuenta(idCtaContable);
        return Ok(tieneAsientos);
    }

    public async Task<OperationResponse<bool>> Eliminar(int idMatrizCble)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is null) return NotFound<bool>("La relacion no existe.");

        if (fila.IdCtaContable.HasValue)
        {
            var tieneAsientos = await TieneAsientosCuenta(fila.IdCtaContable.Value);

            var validacion = ReglasMatrizContable.ValidarEliminacion(tieneAsientos);
            if (!validacion.EsValida)
            {
                return BadRequest<bool>(validacion.Mensaje);
            }
        }

        await AsegurarHistorialInicial(fila);

        _ejecucionContext.TMatrizContable.Remove(fila);
        await _ejecucionContext.SaveChangesAsync();

        await RegistrarHistorial(
            idMatrizCble,
            ReglasMatrizContable.AccionEliminar,
            ReglasMatrizContable.FormatearComentarioEliminacion(),
            DateTime.Now);

        return Ok(true);
    }

    public async Task<OperationResponse<MatrizContableResponse>> Desactivar(int idMatrizCble)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is null) return NotFound<MatrizContableResponse>("La relacion no existe.");

        await AsegurarHistorialInicial(fila);

        if (fila.IdCtaContable.HasValue)
        {
            var cuenta = await _ejecucionContext.TCuentasContables
                .FirstOrDefaultAsync(c => c.IdCtaContable == fila.IdCtaContable.Value);

            if (cuenta is not null)
            {
                cuenta.Activa = ReglasJerarquiaCuentas.CuentaInactiva;
                cuenta.UserAct = GetUserCache();
                cuenta.FecAct = DateTime.Now;
            }
        }

        fila.UserAct = GetUserCache();
        fila.FecAct = DateTime.Now;

        await _ejecucionContext.SaveChangesAsync();

        var enriquecidas = await Enriquecer([fila]);
        var desactivada = enriquecidas[0];

        await RegistrarHistorial(
            idMatrizCble,
            ReglasMatrizContable.AccionDesactivar,
            ReglasMatrizContable.FormatearComentarioDesactivacion(),
            fila.FecAct);

        return Ok(desactivada);
    }

    public async Task<OperationResponse<List<HistorialMatrizResponse>>> GetHistorial(int idMatrizCble)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is not null)
        {
            await AsegurarHistorialInicial(fila);
        }

        var historial = await LeerHistorialDeRelacion(idMatrizCble);

        var ordenados = historial
            .OrderByDescending(h => h.Fecha)
            .ThenByDescending(h => h.IdHistorial)
            .ToList();

        return Ok(ordenados);
    }

    public async Task<OperationResponse<List<PartidaResponse>>> BuscarObjetosGasto(FiltroPartidaRequest filtro)
    {
        var vigencia = await ResolverVigencia(filtro.IdVigencia);
        var consulta = _presupuestoContext.TObjetosGasto.AsNoTracking();

        if (vigencia > 0) consulta = consulta.Where(o => o.IdVigencia == vigencia);
        if (filtro.SoloImputables) consulta = consulta.Where(o => o.UltimoNivel == "S");

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim().ToUpper();
            consulta = consulta.Where(o =>
                o.NombreObjeto!.ToUpper().Contains(texto) || o.NumeroObjeto.ToString()!.Contains(texto));
        }

        var total = await consulta.CountAsync();

        var partidas = await consulta
            .OrderBy(o => o.NumeroObjeto)
            .Skip((Math.Max(filtro.Pagina, 1) - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(o => new PartidaResponse
            {
                Id = o.IdObjetoGasto,
                Numero = o.NumeroObjeto,
                Nombre = o.NombreObjeto,
                IdVigencia = o.IdVigencia,
            })
            .ToListAsync();

        return Paginado(partidas, total);
    }

    public async Task<OperationResponse<List<PartidaResponse>>> BuscarRecursos(FiltroPartidaRequest filtro)
    {
        var vigencia = await ResolverVigencia(filtro.IdVigencia);
        var consulta = _presupuestoContext.TRecursos.AsNoTracking();

        if (vigencia > 0) consulta = consulta.Where(r => r.IdVigencia == vigencia);

        if (filtro.SoloImputables)
        {
            // Los recursos no tienen marca de ultimo nivel como los objetos de gasto: la hoja es
            // la que no tiene hijos. Se resuelve con un LEFT JOIN y no con un Contains sobre
            // subconsulta, porque el proveedor de Oracle traduce ese Contains a una comparacion
            // contra el literal FALSE y Oracle 12.2 responde ORA-00904. Un LEFT JOIN ademas no
            // tiene el limite de mil elementos que tiene un IN.
            consulta =
                from r in consulta
                join h in _presupuestoContext.TRecursos.AsNoTracking()
                    on (int?)r.IdRecurso equals h.IdRecursoRel into hijos
                from hijo in hijos.DefaultIfEmpty()
                where hijo == null
                select r;
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim().ToUpper();
            consulta = consulta.Where(r =>
                r.NombreRecurso!.ToUpper().Contains(texto) || r.NumeroRecurso.ToString()!.Contains(texto));
        }

        var total = await consulta.CountAsync();

        var partidas = await consulta
            .OrderBy(r => r.NumeroRecurso)
            .Skip((Math.Max(filtro.Pagina, 1) - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(r => new PartidaResponse
            {
                Id = r.IdRecurso,
                Numero = r.NumeroRecurso,
                Nombre = r.NombreRecurso,
                IdVigencia = r.IdVigencia,
            })
            .ToListAsync();

        return Paginado(partidas, total);
    }

    public async Task<OperationResponse<PlanImportacionResponse>> PlanificarImportacion(int ejercicioDestino)
        => await Importacion(ejercicioDestino, confirmar: false);

    public async Task<OperationResponse<PlanImportacionResponse>> Importar(int ejercicioDestino)
        => await Importacion(ejercicioDestino, confirmar: true);

    // ---- Importacion -----------------------------------------------------

    private async Task<OperationResponse<PlanImportacionResponse>> Importacion(int ejercicioDestino, bool confirmar)
    {
        var vigenciaDestino = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Ejercicio == ejercicioDestino);

        if (vigenciaDestino is null)
        {
            return NotFound<PlanImportacionResponse>($"No existe el ejercicio {ejercicioDestino}.");
        }

        var ejercicioOrigen = ejercicioDestino - 1;

        var vigenciaOrigen = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Ejercicio == ejercicioOrigen);

        if (vigenciaOrigen is null)
        {
            return NotFound<PlanImportacionResponse>($"No existe el ejercicio {ejercicioOrigen} para tomar como origen.");
        }

        // Guard real. El PL/SQL hacia count(*) + 1, con lo que la condicion nunca fallaba (D11).
        var objetosDestino = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => o.IdVigencia == vigenciaDestino.IdVigencia && o.NumeroObjeto != null)
            .ToDictionaryAsync(o => (long)o.NumeroObjeto!.Value, o => o.IdObjetoGasto);

        if (objetosDestino.Count == 0)
        {
            return BadRequest<PlanImportacionResponse>(
                $"El ejercicio {ejercicioDestino} no tiene objetos de gasto cargados.");
        }

        var recursosDestino = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => r.IdVigencia == vigenciaDestino.IdVigencia && r.NumeroRecurso != null)
            .ToDictionaryAsync(r => r.NumeroRecurso!.Value, r => r.IdRecurso);

        if (recursosDestino.Count == 0)
        {
            return BadRequest<PlanImportacionResponse>(
                $"El ejercicio {ejercicioDestino} no tiene recursos cargados.");
        }

        var numeroDeObjeto = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => o.IdVigencia == vigenciaOrigen.IdVigencia && o.NumeroObjeto != null)
            .ToDictionaryAsync(o => o.IdObjetoGasto, o => (long)o.NumeroObjeto!.Value);

        var numeroDeRecurso = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => r.IdVigencia == vigenciaOrigen.IdVigencia && r.NumeroRecurso != null)
            .ToDictionaryAsync(r => r.IdRecurso, r => r.NumeroRecurso!.Value);

        var origen = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .Where(m => m.IdVigencia == vigenciaOrigen.IdVigencia)
            .Select(m => new FilaMatrizOrigen(m.IdMatrizCble, m.IdCtaContable, m.IdObjetoGasto, m.IdRecurso))
            .ToListAsync();

        var yaEnDestino = (await _ejecucionContext.TMatrizContable
                .AsNoTracking()
                .Where(m => m.IdVigencia == vigenciaDestino.IdVigencia && m.IdCtaContable != null)
                .Select(m => new { m.IdCtaContable, m.IdObjetoGasto, m.IdRecurso })
                .ToListAsync())
            .Where(m => m.IdObjetoGasto is not null || m.IdRecurso is not null)
            .Select(m => (
                m.IdCtaContable!.Value,
                m.IdObjetoGasto ?? m.IdRecurso!.Value,
                m.IdObjetoGasto is not null ? TipoDePartida.Gasto : TipoDePartida.Recurso))
            .ToHashSet();

        var plan = ReglasImportacionMatriz.Planificar(
            origen, objetosDestino, recursosDestino, numeroDeObjeto, numeroDeRecurso, yaEnDestino);

        if (confirmar && plan.AImportar.Count > 0)
        {
            var usuario = GetUserCache();
            var ahora = DateTime.Now;

            foreach (var fila in plan.AImportar)
            {
                _ejecucionContext.TMatrizContable.Add(new TMatrizContable
                {
                    IdMatrizCble = await GetSecuencia(_ejecucionContext, Secuencia),
                    IdCtaContable = fila.IdCtaContable,
                    IdObjetoGasto = fila.IdObjetoGasto,
                    IdRecurso = fila.IdRecurso,
                    IdVigencia = vigenciaDestino.IdVigencia,
                    UserIng = usuario,
                    FecIng = ahora,
                });
            }

            await _ejecucionContext.SaveChangesAsync();
        }

        return Ok(new PlanImportacionResponse
        {
            EjercicioOrigen = ejercicioOrigen,
            EjercicioDestino = ejercicioDestino,
            TotalOrigen = plan.TotalOrigen,
            AImportar = plan.AImportar.Count,
            Gastos = plan.CantidadDeGastos,
            Recursos = plan.CantidadDeRecursos,
            YaExistentes = plan.YaExistentes,
            Confirmada = confirmar,
            NoImportables =
            [
                .. plan.NoImportables.Select(f => new FilaNoImportableResponse
                {
                    IdMatrizOrigen = f.IdMatrizOrigen,
                    TipoPartida = f.Tipo?.ToString(),
                    NumeroPartida = f.NumeroPartida,
                    Motivo = f.Motivo,
                })
            ],
        });
    }

    // ---- Auxiliares ------------------------------------------------------

    private async Task<ContextoMatriz> ArmarContexto(DatosMatriz datos, int? idMatrizAExcluir)
    {
        var tipo = ReglasMatrizContable.TipoDe(datos);

        var cuenta = datos.IdCtaContable is > 0
            ? await _ejecucionContext.TCuentasContables
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCtaContable == datos.IdCtaContable)
            : null;

        var partidaExiste = false;
        var partidaEsDeLaVigencia = false;
        var partidaEsImputable = false;

        if (tipo == TipoDePartida.Gasto)
        {
            var objeto = await _presupuestoContext.TObjetosGasto
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.IdObjetoGasto == datos.IdObjetoGasto);

            partidaExiste = objeto is not null;
            partidaEsDeLaVigencia = objeto?.IdVigencia == datos.IdVigencia;
            partidaEsImputable = objeto?.EsUltimoNivel ?? false;
        }
        else if (tipo == TipoDePartida.Recurso)
        {
            var recurso = await _presupuestoContext.TRecursos
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IdRecurso == datos.IdRecurso);

            partidaExiste = recurso is not null;
            partidaEsDeLaVigencia = recurso?.IdVigencia == datos.IdVigencia;

            if (recurso is not null)
            {
                var tieneHijos = await _presupuestoContext.TRecursos
                    .Where(h => h.IdRecursoRel == recurso.IdRecurso)
                    .Take(1)
                    .CountAsync() > 0;

                partidaEsImputable = !tieneHijos;
            }
        }

        var relacionYaCargada = false;
        string? partidaYaAsignadaA = null;

        if (tipo is not null && datos.IdCtaContable is > 0)
        {
            var mismasPartidas = _ejecucionContext.TMatrizContable
                .AsNoTracking()
                .Where(m => m.IdVigencia == datos.IdVigencia)
                .Where(m => idMatrizAExcluir == null || m.IdMatrizCble != idMatrizAExcluir)
                .Where(m => tipo == TipoDePartida.Gasto
                    ? m.IdObjetoGasto == datos.IdObjetoGasto
                    : m.IdRecurso == datos.IdRecurso);

            relacionYaCargada = await mismasPartidas
                .Where(m => m.IdCtaContable == datos.IdCtaContable)
                .Take(1)
                .CountAsync() > 0;

            if (!relacionYaCargada)
            {
                partidaYaAsignadaA = await mismasPartidas
                    .Where(m => m.IdCtaContable != null)
                    .Join(_ejecucionContext.TCuentasContables,
                        m => m.IdCtaContable,
                        c => c.IdCtaContable,
                        (m, c) => c.Codigo)
                    .FirstOrDefaultAsync();
            }
        }

        return new ContextoMatriz(
            CuentaExiste: cuenta is not null,
            CuentaEsImputable: cuenta?.EsImputable ?? false,
            CuentaEstaActiva: cuenta?.EstaActiva ?? false,
            PartidaExiste: partidaExiste,
            PartidaEsDeLaVigencia: partidaEsDeLaVigencia,
            PartidaEsImputable: partidaEsImputable,
            RelacionYaCargada: relacionYaCargada,
            PartidaYaAsignadaA: partidaYaAsignadaA);
    }

    public async Task<OperationResponse<List<PartidaArbolResponse>>> GetArbolObjetosGasto(int? idVigencia)
    {
        var vigencia = await ResolverVigencia(idVigencia);
        var consulta = _presupuestoContext.TObjetosGasto.AsNoTracking();
        if (vigencia > 0) consulta = consulta.Where(o => o.IdVigencia == vigencia);

        var lista = await consulta
            .OrderBy(o => o.NumeroObjeto)
            .ToListAsync();

        var (objetosRelacionados, _) = await ObtenerPartidasConRelacionActiva(vigencia);

        var nodos = lista.ToDictionary(
            o => o.IdObjetoGasto,
            o => new PartidaArbolResponse
            {
                Id = o.IdObjetoGasto,
                IdPadre = o.IdObjetoGastoRel,
                Numero = o.NumeroObjeto,
                Nombre = o.NombreObjeto,
                EsHoja = o.EsUltimoNivel,
                YaRelacionada = objetosRelacionados.Contains(o.IdObjetoGasto)
            });

        var raices = new List<PartidaArbolResponse>();
        foreach (var o in lista)
        {
            var nodo = nodos[o.IdObjetoGasto];
            if (o.IdObjetoGastoRel.HasValue && nodos.TryGetValue(o.IdObjetoGastoRel.Value, out var padre) && padre != nodo)
            {
                padre.Hijas.Add(nodo);
            }
            else
            {
                raices.Add(nodo);
            }
        }

        foreach (var n in nodos.Values)
        {
            if (n.Hijas.Count == 0) n.EsHoja = true;
        }

        return Ok(raices);
    }

    public async Task<OperationResponse<List<PartidaArbolResponse>>> GetArbolRecursos(int? idVigencia)
    {
        var vigencia = await ResolverVigencia(idVigencia);
        var consulta = _presupuestoContext.TRecursos.AsNoTracking();
        if (vigencia > 0) consulta = consulta.Where(r => r.IdVigencia == vigencia);

        var lista = await consulta
            .OrderBy(r => r.NumeroRecurso)
            .ToListAsync();

        var (_, recursosRelacionados) = await ObtenerPartidasConRelacionActiva(vigencia);

        var nodos = lista.ToDictionary(
            r => r.IdRecurso,
            r => new PartidaArbolResponse
            {
                Id = r.IdRecurso,
                IdPadre = r.IdRecursoRel,
                Numero = r.NumeroRecurso,
                Nombre = r.NombreRecurso,
                EsHoja = false,
                YaRelacionada = recursosRelacionados.Contains(r.IdRecurso)
            });

        var raices = new List<PartidaArbolResponse>();
        foreach (var r in lista)
        {
            var nodo = nodos[r.IdRecurso];
            if (r.IdRecursoRel.HasValue && nodos.TryGetValue(r.IdRecursoRel.Value, out var padre) && padre != nodo)
            {
                padre.Hijas.Add(nodo);
            }
            else
            {
                raices.Add(nodo);
            }
        }

        foreach (var n in nodos.Values)
        {
            if (n.Hijas.Count == 0) n.EsHoja = true;
        }

        return Ok(raices);
    }

    public async Task<OperationResponse<PendientesMatrizResponse>> GetPendientes(int? idVigencia)
    {
        var vigencia = await ResolverVigencia(idVigencia);
        var pendientes = await ObtenerRelacionesPendientes(vigencia);
        return Ok(pendientes);
    }

    private async Task<(HashSet<int> ObjetosActivos, HashSet<int> RecursosActivos)> ObtenerPartidasConRelacionActiva(int vigencia)
    {
        var relacionesActivas = await (
            from m in _ejecucionContext.TMatrizContable.AsNoTracking()
            join c in _ejecucionContext.TCuentasContables.AsNoTracking()
                on m.IdCtaContable equals c.IdCtaContable into cuentasJoin
            from c in cuentasJoin.DefaultIfEmpty()
            where vigencia <= 0 || m.IdVigencia == vigencia
            where c == null || c.Activa == ReglasJerarquiaCuentas.CuentaActiva
            select new { m.IdObjetoGasto, m.IdRecurso }
        ).ToListAsync();

        var objetosActivos = relacionesActivas
            .Where(r => r.IdObjetoGasto != null)
            .Select(r => r.IdObjetoGasto!.Value)
            .ToHashSet();

        var recursosActivos = relacionesActivas
            .Where(r => r.IdRecurso != null)
            .Select(r => r.IdRecurso!.Value)
            .ToHashSet();

        return (objetosActivos, recursosActivos);
    }

    private async Task<PendientesMatrizResponse> ObtenerRelacionesPendientes(int vigencia)
    {
        var (objetosConCuentaActiva, recursosConCuentaActiva) = await ObtenerPartidasConRelacionActiva(vigencia);

        var gastosUltimoNivel = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => (vigencia <= 0 || o.IdVigencia == vigencia) && o.UltimoNivel == "S")
            .OrderBy(o => o.NumeroObjeto)
            .Select(o => new { o.IdObjetoGasto, o.NumeroObjeto, o.NombreObjeto })
            .ToListAsync();

        var gastosPendientes = gastosUltimoNivel
            .Where(o => !objetosConCuentaActiva.Contains(o.IdObjetoGasto))
            .Select(o => new RelacionPendienteResponse
            {
                Id = o.IdObjetoGasto,
                Tipo = "Gasto",
                Numero = o.NumeroObjeto,
                Nombre = o.NombreObjeto,
            })
            .ToList();

        var todosLosRecursos = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => vigencia <= 0 || r.IdVigencia == vigencia)
            .OrderBy(r => r.NumeroRecurso)
            .Select(r => new { r.IdRecurso, r.IdRecursoRel, r.NumeroRecurso, r.NombreRecurso })
            .ToListAsync();

        var idsRecursosPadre = todosLosRecursos
            .Where(r => r.IdRecursoRel != null)
            .Select(r => r.IdRecursoRel!.Value)
            .ToHashSet();

        var recursosPendientes = todosLosRecursos
            .Where(r => !idsRecursosPadre.Contains(r.IdRecurso) && !recursosConCuentaActiva.Contains(r.IdRecurso))
            .Select(r => new RelacionPendienteResponse
            {
                Id = r.IdRecurso,
                Tipo = "Recurso",
                Numero = r.NumeroRecurso,
                Nombre = r.NombreRecurso,
            })
            .ToList();

        return new PendientesMatrizResponse
        {
            GastosSinRelacionar = gastosPendientes.Count,
            RecursosSinRelacionar = recursosPendientes.Count,
            RelacionesPendientes = [.. gastosPendientes, .. recursosPendientes],
        };
    }

    private async Task<HashSet<int>> ObtenerDescendientesCuenta(int idCtaContable, int vigencia)
    {
        var cuentas = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => vigencia <= 0 || c.IdVigencia == vigencia)
            .Select(c => new { c.IdCtaContable, c.IdCtaContableRel })
            .ToListAsync();

        var descendientes = new HashSet<int> { idCtaContable };
        bool agregado;
        do
        {
            agregado = false;
            foreach (var c in cuentas)
            {
                if (c.IdCtaContableRel.HasValue && descendientes.Contains(c.IdCtaContableRel.Value) && descendientes.Add(c.IdCtaContable))
                {
                    agregado = true;
                }
            }
        } while (agregado);

        return descendientes;
    }

    private async Task<HashSet<int>> ObtenerDescendientesObjetoGasto(int idObjetoGasto, int vigencia)
    {
        var objetos = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => vigencia <= 0 || o.IdVigencia == vigencia)
            .Select(o => new { o.IdObjetoGasto, o.IdObjetoGastoRel })
            .ToListAsync();

        var descendientes = new HashSet<int> { idObjetoGasto };
        bool agregado;
        do
        {
            agregado = false;
            foreach (var o in objetos)
            {
                if (o.IdObjetoGastoRel.HasValue && descendientes.Contains(o.IdObjetoGastoRel.Value) && descendientes.Add(o.IdObjetoGasto))
                {
                    agregado = true;
                }
            }
        } while (agregado);

        return descendientes;
    }

    private async Task<HashSet<int>> ObtenerDescendientesRecurso(int idRecurso, int vigencia)
    {
        var recursos = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => vigencia <= 0 || r.IdVigencia == vigencia)
            .Select(r => new { r.IdRecurso, r.IdRecursoRel })
            .ToListAsync();

        var descendientes = new HashSet<int> { idRecurso };
        bool agregado;
        do
        {
            agregado = false;
            foreach (var r in recursos)
            {
                if (r.IdRecursoRel.HasValue && descendientes.Contains(r.IdRecursoRel.Value) && descendientes.Add(r.IdRecurso))
                {
                    agregado = true;
                }
            }
        } while (agregado);

        return descendientes;
    }

    /// <summary>
    /// Completa las filas con el codigo y el nombre de la cuenta y de la partida.
    /// Se resuelve en consultas por lote, no una por fila.
    /// </summary>
    private async Task<List<MatrizContableResponse>> Enriquecer(List<TMatrizContable> filas)
    {
        if (filas.Count == 0) return [];

        var idsCuenta = filas.Where(f => f.IdCtaContable is not null).Select(f => f.IdCtaContable!.Value).Distinct().ToList();
        var idsObjeto = filas.Where(f => f.IdObjetoGasto is not null).Select(f => f.IdObjetoGasto!.Value).Distinct().ToList();
        var idsRecurso = filas.Where(f => f.IdRecurso is not null).Select(f => f.IdRecurso!.Value).Distinct().ToList();
        var idsVigencia = filas.Where(f => f.IdVigencia is not null).Select(f => f.IdVigencia!.Value).Distinct().ToList();

        var cuentas = idsCuenta.Count > 0
            ? await _ejecucionContext.TCuentasContables
                .AsNoTracking()
                .Where(c => idsCuenta.Contains(c.IdCtaContable))
                .Select(c => new { c.IdCtaContable, c.Codigo, c.Nombre, c.Activa })
                .ToDictionaryAsync(c => c.IdCtaContable)
            : [];

        var objetos = idsObjeto.Count > 0
            ? await _presupuestoContext.TObjetosGasto
                .AsNoTracking()
                .Where(o => idsObjeto.Contains(o.IdObjetoGasto))
                .Select(o => new { o.IdObjetoGasto, o.NumeroObjeto, o.NombreObjeto })
                .ToDictionaryAsync(o => o.IdObjetoGasto)
            : [];

        var recursos = idsRecurso.Count > 0
            ? await _presupuestoContext.TRecursos
                .AsNoTracking()
                .Where(r => idsRecurso.Contains(r.IdRecurso))
                .Select(r => new { r.IdRecurso, r.NumeroRecurso, r.NombreRecurso })
                .ToDictionaryAsync(r => r.IdRecurso)
            : [];

        var ejercicios = idsVigencia.Count > 0
            ? await _presupuestoContext.TVigencias
                .AsNoTracking()
                .Where(v => idsVigencia.Contains(v.IdVigencia))
                .ToDictionaryAsync(v => v.IdVigencia, v => v.Ejercicio)
            : [];

        var cuentasConAsientos = idsCuenta.Count > 0
            ? (await _ejecucionContext.TAsientosDetalle
                .AsNoTracking()
                .Where(d => idsCuenta.Contains(d.IdCtaContable))
                .Select(d => d.IdCtaContable)
                .Distinct()
                .ToListAsync())
                .ToHashSet()
            : [];

        return
        [
            .. filas.Select(f =>
            {
                var cuenta = f.IdCtaContable is not null && cuentas.TryGetValue(f.IdCtaContable.Value, out var c) ? c : null;
                var objeto = f.IdObjetoGasto is not null && objetos.TryGetValue(f.IdObjetoGasto.Value, out var o) ? o : null;
                var recurso = f.IdRecurso is not null && recursos.TryGetValue(f.IdRecurso.Value, out var r) ? r : null;

                var tipo = ReglasMatrizContable.TipoDe(
                    new DatosMatriz(f.IdCtaContable, f.IdObjetoGasto, f.IdRecurso, f.IdVigencia));

                var estaActiva = cuenta != null ? cuenta.Activa == ReglasJerarquiaCuentas.CuentaActiva : true;
                var tieneAsientos = f.IdCtaContable is not null && cuentasConAsientos.Contains(f.IdCtaContable.Value);

                return new MatrizContableResponse
                {
                    IdMatrizCble = f.IdMatrizCble,
                    IdCtaContable = f.IdCtaContable,
                    CodigoCuenta = cuenta?.Codigo,
                    NombreCuenta = cuenta?.Nombre,
                    Activa = estaActiva,
                    TieneAsientos = tieneAsientos,
                    IdObjetoGasto = f.IdObjetoGasto,
                    NumeroObjeto = objeto?.NumeroObjeto,
                    NombreObjeto = objeto?.NombreObjeto,
                    IdRecurso = f.IdRecurso,
                    NumeroRecurso = recurso?.NumeroRecurso,
                    NombreRecurso = recurso?.NombreRecurso,
                    TipoPartida = tipo?.ToString(),
                    IdVigencia = f.IdVigencia,
                    Ejercicio = f.IdVigencia is not null && ejercicios.TryGetValue(f.IdVigencia.Value, out var e) ? e : null,
                    UserIng = f.UserIng,
                    FecIng = f.FecIng,
                    UserAct = f.UserAct,
                    FecAct = f.FecAct,
                };
            })
        ];
    }

    /// <summary>
    /// Verifica si existen renglones de asiento para la cuenta.
    /// No se usa AnyAsync porque el proveedor de Oracle lo traduce al literal booleano FALSE,
    /// que Oracle 12.2 rechaza con ORA-00904. Take(1) + CountAsync corta en la primera fila.
    /// </summary>
    private async Task<bool> TieneAsientosCuenta(int idCtaContable)
        => await _ejecucionContext.TAsientosDetalle
            .AsNoTracking()
            .Where(d => d.IdCtaContable == idCtaContable)
            .Take(1)
            .CountAsync() > 0;

    private async Task<int> ResolverVigencia(int? idVigencia)
    {
        if (idVigencia is > 0) return idVigencia.Value;
        var claim = GetIdVigencia();
        if (claim > 0) return claim;

        var activa = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => v.ActivoEjecucion == "S")
            .Select(v => v.IdVigencia)
            .FirstOrDefaultAsync();

        return activa;
    }

    // ---- Historial de Relaciones (T.16409) -------------------------------

    private static readonly object HistorialLock = new();
    private static readonly string ArchivoHistorialLocal = Path.Combine(AppContext.BaseDirectory, "historial-matriz.json");
    private static bool TablaHistorialVerificada;

    private async Task AsegurarHistorialInicial(TMatrizContable fila)
    {
        var existentes = await LeerHistorialDeRelacion(fila.IdMatrizCble);
        if (existentes.Any(h => string.Equals(h.Accion, ReglasMatrizContable.AccionGenerar, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var enriquecidas = await Enriquecer([fila]);
        if (enriquecidas.Count == 0) return;

        var r = enriquecidas[0];
        var usuarioIng = await ResolverNombreUsuarioAuditoria(fila.UserIng);

        await RegistrarHistorial(
            fila.IdMatrizCble,
            ReglasMatrizContable.AccionGenerar,
            ReglasMatrizContable.FormatearComentarioAlta(
                r.CodigoCuenta,
                r.NombreCuenta,
                r.TipoPartida,
                r.NumeroObjeto ?? r.NumeroRecurso,
                r.NombreObjeto ?? r.NombreRecurso),
            fila.FecIng ?? DateTime.Now,
            usuarioIng);

        if (fila.FecAct.HasValue && existingHasNoModOrDeactivate(existentes))
        {
            var usuarioAct = await ResolverNombreUsuarioAuditoria(fila.UserAct ?? fila.UserIng);
            if (!r.Activa)
            {
                await RegistrarHistorial(
                    fila.IdMatrizCble,
                    ReglasMatrizContable.AccionDesactivar,
                    ReglasMatrizContable.FormatearComentarioDesactivacion(),
                    fila.FecAct.Value,
                    usuarioAct);
            }
            else
            {
                await RegistrarHistorial(
                    fila.IdMatrizCble,
                    ReglasMatrizContable.AccionModificar,
                    ReglasMatrizContable.FormatearComentarioModificacion(
                        r.CodigoCuenta,
                        r.NombreCuenta,
                        r.TipoPartida,
                        r.NumeroObjeto ?? r.NumeroRecurso,
                        r.NombreObjeto ?? r.NombreRecurso),
                    fila.FecAct.Value,
                    usuarioAct);
            }
        }
    }

    private static bool existingHasNoModOrDeactivate(List<HistorialMatrizResponse> existentes)
        => !existentes.Any(h =>
            string.Equals(h.Accion, ReglasMatrizContable.AccionModificar, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(h.Accion, ReglasMatrizContable.AccionDesactivar, StringComparison.OrdinalIgnoreCase));

    private async Task RegistrarHistorial(
        int idMatrizCble,
        string accion,
        string comentarios,
        DateTime? fecha = null,
        string? usuarioExplicito = null)
    {
        var fechaEvento = fecha ?? DateTime.Now;
        var usuario = !string.IsNullOrWhiteSpace(usuarioExplicito)
            ? usuarioExplicito
            : await ObtenerUsuarioActualFormateado();

        var nuevoEvento = new HistorialMatrizResponse
        {
            IdHistorial = (int)(DateTime.UtcNow.Ticks % int.MaxValue),
            IdMatrizCble = idMatrizCble,
            Fecha = fechaEvento,
            Usuario = usuario,
            Accion = accion,
            Comentarios = comentarios,
        };

        GuardarEnArchivoLocal(nuevoEvento);
        await IntentarGuardarEnTablaOracle(nuevoEvento);
    }

    private async Task<List<HistorialMatrizResponse>> LeerHistorialDeRelacion(int idMatrizCble)
    {
        var desdeArchivo = LeerDesdeArchivoLocal(idMatrizCble);
        var desdeOracle = await IntentarLeerDesdeTablaOracle(idMatrizCble);

        var combinados = new List<HistorialMatrizResponse>(desdeOracle);
        foreach (var item in desdeArchivo)
        {
            var duplicado = combinados.Any(c =>
                string.Equals(c.Accion, item.Accion, StringComparison.OrdinalIgnoreCase) &&
                Math.Abs((c.Fecha - item.Fecha).TotalSeconds) < 2);
            if (!duplicado)
            {
                combinados.Add(item);
            }
        }

        return combinados;
    }

    private async Task<string> ObtenerUsuarioActualFormateado()
    {
        var idPersona = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(x => x.Type == "IdPersona")?.Value;

        if (!string.IsNullOrWhiteSpace(idPersona))
        {
            var persona = await _portalContext.TPersonas
                .AsNoTracking()
                .Where(p => p.IdPersona == idPersona)
                .Select(p => new { p.Apellido, p.Nombre })
                .FirstOrDefaultAsync();

            if (persona is not null && (!string.IsNullOrWhiteSpace(persona.Apellido) || !string.IsNullOrWhiteSpace(persona.Nombre)))
            {
                return $"{persona.Apellido?.Trim()}, {persona.Nombre?.Trim()}".Trim(' ', ',');
            }
        }

        var userCache = GetUserCache();
        if (!string.IsNullOrWhiteSpace(userCache))
        {
            return await ResolverNombreUsuarioAuditoria(userCache);
        }

        var apellidoYNombre = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(x => x.Type == "ApellidoYNombre")?.Value;

        return !string.IsNullOrWhiteSpace(apellidoYNombre) ? apellidoYNombre : "Sistema";
    }

    private async Task<string> ResolverNombreUsuarioAuditoria(string? nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario)) return "Sistema";

        try
        {
            var limpio = nombreUsuario.Trim().ToUpper();
            var datosPersona = await (
                from u in _portalContext.TUsuariosNew.AsNoTracking()
                join p in _portalContext.TPersonas.AsNoTracking()
                    on u.IdPersona equals p.IdPersona
                where u.NombreUsuario != null && u.NombreUsuario.ToUpper() == limpio
                select new { p.Apellido, p.Nombre }
            ).FirstOrDefaultAsync();

            if (datosPersona is not null && (!string.IsNullOrWhiteSpace(datosPersona.Apellido) || !string.IsNullOrWhiteSpace(datosPersona.Nombre)))
            {
                return $"{datosPersona.Apellido?.Trim()}, {datosPersona.Nombre?.Trim()}".Trim(' ', ',');
            }
        }
        catch
        {
            // Si no se puede resolver en PORTAL, se conserva el identificador de usuario.
        }

        return nombreUsuario;
    }

    private static void GuardarEnArchivoLocal(HistorialMatrizResponse evento)
    {
        lock (HistorialLock)
        {
            try
            {
                var lista = new List<HistorialMatrizResponse>();
                if (File.Exists(ArchivoHistorialLocal))
                {
                    var json = File.ReadAllText(ArchivoHistorialLocal);
                    lista = JsonSerializer.Deserialize<List<HistorialMatrizResponse>>(json) ?? [];
                }

                evento.IdHistorial = lista.Count > 0 ? lista.Max(x => x.IdHistorial) + 1 : 1;
                lista.Add(evento);
                File.WriteAllText(ArchivoHistorialLocal, JsonSerializer.Serialize(lista));
            }
            catch
            {
                // Ignorar errores de sistema de archivos de solo lectura
            }
        }
    }

    private static List<HistorialMatrizResponse> LeerDesdeArchivoLocal(int idMatrizCble)
    {
        lock (HistorialLock)
        {
            try
            {
                if (!File.Exists(ArchivoHistorialLocal)) return [];
                var json = File.ReadAllText(ArchivoHistorialLocal);
                var lista = JsonSerializer.Deserialize<List<HistorialMatrizResponse>>(json) ?? [];
                return lista.Where(x => x.IdMatrizCble == idMatrizCble).ToList();
            }
            catch
            {
                return [];
            }
        }
    }

    private async Task AsegurarTablaOracle()
    {
        if (TablaHistorialVerificada) return;
        TablaHistorialVerificada = true;

        try
        {
            var conn = _ejecucionContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await _ejecucionContext.Database.OpenConnectionAsync();
            }

            const string ddl = """
                DECLARE
                    v_existe NUMBER := 0;
                BEGIN
                    SELECT COUNT(*) INTO v_existe
                      FROM ALL_TABLES
                     WHERE OWNER = 'EJECUCION'
                       AND TABLE_NAME = 'T_MATRIZ_CONTABLE_HIST';
                    IF v_existe = 0 THEN
                        EXECUTE IMMEDIATE '
                            CREATE TABLE EJECUCION.T_MATRIZ_CONTABLE_HIST (
                                ID_HISTORIAL NUMBER PRIMARY KEY,
                                ID_MATRIZ_CBLE NUMBER NOT NULL,
                                FECHA DATE NOT NULL,
                                USUARIO VARCHAR2(200),
                                ACCION VARCHAR2(50),
                                COMENTARIOS VARCHAR2(1000)
                            )';
                    END IF;
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;
                """;
            await conn.ExecuteAsync(ddl);
        }
        catch
        {
            // Si el usuario de BD no posee permisos DDL, se opera con el almacén local.
        }
    }

    private async Task IntentarGuardarEnTablaOracle(HistorialMatrizResponse evento)
    {
        try
        {
            await AsegurarTablaOracle();
            var conn = _ejecucionContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await _ejecucionContext.Database.OpenConnectionAsync();
            }

            const string sql = """
                INSERT INTO EJECUCION.T_MATRIZ_CONTABLE_HIST
                    (ID_HISTORIAL, ID_MATRIZ_CBLE, FECHA, USUARIO, ACCION, COMENTARIOS)
                VALUES
                    (:IdHistorial, :IdMatrizCble, :Fecha, :Usuario, :Accion, :Comentarios)
                """;
            await conn.ExecuteAsync(sql, evento);
        }
        catch
        {
            // Fallback silencioso a almacenamiento local si la tabla no existe en el esquema
        }
    }

    private async Task<List<HistorialMatrizResponse>> IntentarLeerDesdeTablaOracle(int idMatrizCble)
    {
        try
        {
            await AsegurarTablaOracle();
            var conn = _ejecucionContext.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await _ejecucionContext.Database.OpenConnectionAsync();
            }

            const string sql = """
                SELECT ID_HISTORIAL AS IdHistorial,
                       ID_MATRIZ_CBLE AS IdMatrizCble,
                       FECHA AS Fecha,
                       USUARIO AS Usuario,
                       ACCION AS Accion,
                       COMENTARIOS AS Comentarios
                  FROM EJECUCION.T_MATRIZ_CONTABLE_HIST
                 WHERE ID_MATRIZ_CBLE = :IdMatrizCble
                 ORDER BY FECHA DESC, ID_HISTORIAL DESC
                """;
            var filas = await conn.QueryAsync<HistorialMatrizResponse>(sql, new { IdMatrizCble = idMatrizCble });
            return filas.ToList();
        }
        catch
        {
            return [];
        }
    }
}
