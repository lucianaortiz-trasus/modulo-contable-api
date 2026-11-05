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

        var consulta = _ejecucionContext.TMatrizContable.AsNoTracking();

        var vigencia = filtro.IdVigencia ?? GetIdVigencia();
        if (vigencia > 0) consulta = consulta.Where(m => m.IdVigencia == vigencia);

        if (filtro.IdCtaContable is > 0)
        {
            var idsCuentas = await ObtenerDescendientesCuenta(filtro.IdCtaContable.Value, vigencia);
            consulta = consulta.Where(m => m.IdCtaContable != null && idsCuentas.Contains(m.IdCtaContable.Value));
        }

        if (filtro.IdObjetoGasto is > 0)
        {
            var idsObjetos = await ObtenerDescendientesObjetoGasto(filtro.IdObjetoGasto.Value, vigencia);
            consulta = consulta.Where(m => m.IdObjetoGasto != null && idsObjetos.Contains(m.IdObjetoGasto.Value));
        }

        if (filtro.IdRecurso is > 0)
        {
            var idsRecursos = await ObtenerDescendientesRecurso(filtro.IdRecurso.Value, vigencia);
            consulta = consulta.Where(m => m.IdRecurso != null && idsRecursos.Contains(m.IdRecurso.Value));
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

        var total = ordenadas.Count;
        var paged = ordenadas
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToList();

        return Paginado(paged, total);
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
        var vigencia = request.IdVigencia ?? GetIdVigencia();
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
        return Ok(enriquecidas[0]);
    }

    public async Task<OperationResponse<MatrizContableResponse>> Editar(int idMatrizCble, MatrizContableRequest request)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is null) return NotFound<MatrizContableResponse>("La relacion no existe.");

        var vigencia = request.IdVigencia ?? fila.IdVigencia ?? GetIdVigencia();
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
        return Ok(enriquecidas[0]);
    }

    public async Task<OperationResponse<bool>> Eliminar(int idMatrizCble)
    {
        var fila = await _ejecucionContext.TMatrizContable
            .FirstOrDefaultAsync(m => m.IdMatrizCble == idMatrizCble);

        if (fila is null) return NotFound<bool>("La relacion no existe.");

        _ejecucionContext.TMatrizContable.Remove(fila);
        await _ejecucionContext.SaveChangesAsync();

        return Ok(true);
    }

    public async Task<OperationResponse<List<PartidaResponse>>> BuscarObjetosGasto(FiltroPartidaRequest filtro)
    {
        var vigencia = filtro.IdVigencia ?? GetIdVigencia();
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
        var vigencia = filtro.IdVigencia ?? GetIdVigencia();
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
        var vigencia = idVigencia ?? GetIdVigencia();
        var consulta = _presupuestoContext.TObjetosGasto.AsNoTracking();
        if (vigencia > 0) consulta = consulta.Where(o => o.IdVigencia == vigencia);

        var lista = await consulta
            .OrderBy(o => o.NumeroObjeto)
            .ToListAsync();

        var nodos = lista.ToDictionary(
            o => o.IdObjetoGasto,
            o => new PartidaArbolResponse
            {
                Id = o.IdObjetoGasto,
                IdPadre = o.IdObjetoGastoRel,
                Numero = o.NumeroObjeto,
                Nombre = o.NombreObjeto,
                EsHoja = o.EsUltimoNivel
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
        var vigencia = idVigencia ?? GetIdVigencia();
        var consulta = _presupuestoContext.TRecursos.AsNoTracking();
        if (vigencia > 0) consulta = consulta.Where(r => r.IdVigencia == vigencia);

        var lista = await consulta
            .OrderBy(r => r.NumeroRecurso)
            .ToListAsync();

        var nodos = lista.ToDictionary(
            r => r.IdRecurso,
            r => new PartidaArbolResponse
            {
                Id = r.IdRecurso,
                IdPadre = r.IdRecursoRel,
                Numero = r.NumeroRecurso,
                Nombre = r.NombreRecurso,
                EsHoja = false
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
        var vigencia = idVigencia ?? GetIdVigencia();

        var objetosConCuenta = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .Where(m => (vigencia <= 0 || m.IdVigencia == vigencia) && m.IdObjetoGasto != null)
            .Select(m => m.IdObjetoGasto!.Value)
            .Distinct()
            .ToListAsync();

        var recursosConCuenta = await _ejecucionContext.TMatrizContable
            .AsNoTracking()
            .Where(m => (vigencia <= 0 || m.IdVigencia == vigencia) && m.IdRecurso != null)
            .Select(m => m.IdRecurso!.Value)
            .Distinct()
            .ToListAsync();

        var sinRelacionarGastos = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => (vigencia <= 0 || o.IdVigencia == vigencia) && o.UltimoNivel == "S")
            .Where(o => !objetosConCuenta.Contains(o.IdObjetoGasto))
            .CountAsync();

        var idsRecursosPadre = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => (vigencia <= 0 || r.IdVigencia == vigencia) && r.IdRecursoRel != null)
            .Select(r => r.IdRecursoRel!.Value)
            .Distinct()
            .ToListAsync();

        var sinRelacionarRecursos = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => (vigencia <= 0 || r.IdVigencia == vigencia))
            .Where(r => !idsRecursosPadre.Contains(r.IdRecurso))
            .Where(r => !recursosConCuenta.Contains(r.IdRecurso))
            .CountAsync();

        return Ok(new PendientesMatrizResponse
        {
            GastosSinRelacionar = sinRelacionarGastos,
            RecursosSinRelacionar = sinRelacionarRecursos
        });
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
    /// Se resuelve en tres consultas por lote, no una por fila.
    /// </summary>
    private async Task<List<MatrizContableResponse>> Enriquecer(List<TMatrizContable> filas)
    {
        var idsCuenta = filas.Where(f => f.IdCtaContable is not null).Select(f => f.IdCtaContable!.Value).Distinct().ToList();
        var idsObjeto = filas.Where(f => f.IdObjetoGasto is not null).Select(f => f.IdObjetoGasto!.Value).Distinct().ToList();
        var idsRecurso = filas.Where(f => f.IdRecurso is not null).Select(f => f.IdRecurso!.Value).Distinct().ToList();
        var idsVigencia = filas.Where(f => f.IdVigencia is not null).Select(f => f.IdVigencia!.Value).Distinct().ToList();

        var cuentas = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => idsCuenta.Contains(c.IdCtaContable))
            .Select(c => new { c.IdCtaContable, c.Codigo, c.Nombre, c.Activa })
            .ToDictionaryAsync(c => c.IdCtaContable);

        var objetos = await _presupuestoContext.TObjetosGasto
            .AsNoTracking()
            .Where(o => idsObjeto.Contains(o.IdObjetoGasto))
            .Select(o => new { o.IdObjetoGasto, o.NumeroObjeto, o.NombreObjeto })
            .ToDictionaryAsync(o => o.IdObjetoGasto);

        var recursos = await _presupuestoContext.TRecursos
            .AsNoTracking()
            .Where(r => idsRecurso.Contains(r.IdRecurso))
            .Select(r => new { r.IdRecurso, r.NumeroRecurso, r.NombreRecurso })
            .ToDictionaryAsync(r => r.IdRecurso);

        var ejercicios = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .Where(v => idsVigencia.Contains(v.IdVigencia))
            .ToDictionaryAsync(v => v.IdVigencia, v => v.Ejercicio);

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

                return new MatrizContableResponse
                {
                    IdMatrizCble = f.IdMatrizCble,
                    IdCtaContable = f.IdCtaContable,
                    CodigoCuenta = cuenta?.Codigo,
                    NombreCuenta = cuenta?.Nombre,
                    Activa = estaActiva,
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
}
