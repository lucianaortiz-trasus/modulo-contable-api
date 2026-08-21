using Contable.Application.RequestDto.PlanDeCuentas;
using Contable.Application.ResponseDto.Dtos.PlanDeCuentas;
using Contable.Domain.Reglas;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Plan de cuentas: arbol, grilla y ABM.
/// </summary>
/// <remarks>
/// Reemplaza a PKG_T_CUENTAS_CONTABLES. Las decisiones de fondo estan en _analisis/REGLAS.md:
/// el arbol se arma por la relacion padre-hijo y no por prefijo de codigo (D22), el proximo codigo
/// se calcula de cero en lugar de portar las FUNC_PROXIMO_NIVEL rotas (D6), y el alta con codigo
/// repetido devuelve un error de verdad en vez de un id de secuencia que hace creer que grabo (D7).
/// </remarks>
public class PlanDeCuentasService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IPlanDeCuentasService
{
    private const string Secuencia = "EJECUCION.SQ_T_CUENTAS_CONTABLES";

    private const string Si = ReglasJerarquiaCuentas.MarcaSi;

    private const string No = ReglasJerarquiaCuentas.MarcaNo;

    private const string Activa = ReglasJerarquiaCuentas.CuentaActiva;

    private const string Inactiva = ReglasJerarquiaCuentas.CuentaInactiva;

    public async Task<OperationResponse<ArbolPlanDeCuentasResponse>> GetArbol(bool incluirInactivas)
    {
        var cuentas = await ConsultaBase()
            .Where(c => incluirInactivas || c.Activa == Activa)
            .ToListAsync();

        var propias = cuentas.Count;
        var deContexto = await CompletarAncestros(cuentas);

        var arbol = ArbolDeCuentas.Armar(cuentas);

        var respuesta = new ArbolPlanDeCuentasResponse
        {
            Raices = [.. arbol.Raices.Select(nodo => MapearNodo(nodo, deContexto))],
            TotalCuentas = propias,
            CuentasDeContexto = deContexto.Count,
            TotalImputables = cuentas.Count(c => c.EsImputable && !deContexto.Contains(c.IdCtaContable)),
            CuentasConAnomalias =
            [
                .. arbol.ConAnomalias
                    .OrderBy(n => CodigoCuenta.Normalizar(n.Cuenta.Codigo), StringComparer.Ordinal)
                    .Select(n => new CuentaConAnomaliaResponse
                    {
                        IdCtaContable = n.Cuenta.IdCtaContable,
                        Codigo = n.Cuenta.Codigo,
                        Nombre = n.Cuenta.Nombre,
                        Anomalias = DescribirAnomalias(n.Anomalias),
                    })
            ],
        };

        return Ok(respuesta);
    }

    public async Task<OperationResponse<List<CuentaContableResponse>>> Buscar(FiltroCuentasRequest filtro)
    {
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tamano = filtro.TamanoPagina is < 1 or > 200 ? 20 : filtro.TamanoPagina;

        var consulta = ConsultaBase();

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

        if (filtro.Nivel is not null) consulta = consulta.Where(c => c.Nivel == filtro.Nivel);
        if (filtro.Imputable is not null) consulta = consulta.Where(c => c.Imputable == (filtro.Imputable.Value ? Si : No));
        if (filtro.Activa is not null) consulta = consulta.Where(c => c.Activa == (filtro.Activa.Value ? Activa : Inactiva));
        if (filtro.IdNaturaleza is not null) consulta = consulta.Where(c => c.IdNaturaleza == filtro.IdNaturaleza);

        var total = await consulta.CountAsync();

        var cuentas = await consulta
            .OrderBy(c => c.Codigo)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync();

        var ids = cuentas.Select(c => c.IdCtaContable).ToList();

        var hijasPorPadre = await _ejecucionContext.TCuentasContables
            .AsNoTracking()
            .Where(c => c.IdCtaContableRel != null && ids.Contains(c.IdCtaContableRel.Value))
            .GroupBy(c => c.IdCtaContableRel!.Value)
            .Select(g => new { Padre = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Padre, x => x.Cantidad);

        var resultado = cuentas
            .Select(c => Mapear(c, hijasPorPadre.GetValueOrDefault(c.IdCtaContable)))
            .ToList();

        return Paginado(resultado, total);
    }

    public async Task<OperationResponse<CuentaContableResponse>> GetPorId(int idCtaContable)
    {
        var cuenta = await ConsultaBase().FirstOrDefaultAsync(c => c.IdCtaContable == idCtaContable);

        if (cuenta is null) return NotFound<CuentaContableResponse>("La cuenta no existe.");

        var respuesta = Mapear(cuenta, await ContarHijas(idCtaContable));
        respuesta.TieneMovimientos = await TieneMovimientos(idCtaContable);
        respuesta.CodigoPadre = await CodigoDe(cuenta.IdCtaContableRel);

        return Ok(respuesta);
    }

    public async Task<OperationResponse<ProximoCodigoResponse>> GetProximoCodigo(int? idCtaContableRel)
    {
        string? codigoPadre = null;
        var nivelPadre = 0;

        if (idCtaContableRel is not null)
        {
            var padre = await ConsultaBase().FirstOrDefaultAsync(c => c.IdCtaContable == idCtaContableRel);

            if (padre is null) return NotFound<ProximoCodigoResponse>("La cuenta padre no existe.");

            codigoPadre = padre.Codigo;
            nivelPadre = padre.Nivel ?? CodigoCuenta.NivelSegunCodigo(padre.Codigo);
        }

        if (nivelPadre >= CodigoCuenta.NivelMaximo)
        {
            return BadRequest<ProximoCodigoResponse>(
                $"El plan de cuentas llega hasta el nivel {CodigoCuenta.NivelMaximo}.");
        }

        var codigosHijas = await ConsultaBase()
            .Where(c => idCtaContableRel == null ? c.IdCtaContableRel == null : c.IdCtaContableRel == idCtaContableRel)
            .Select(c => c.Codigo)
            .ToListAsync();

        var proximo = ReglasProximoCodigo.Calcular(codigoPadre, nivelPadre, codigosHijas);

        return Ok(new ProximoCodigoResponse
        {
            Codigo = proximo.Codigo,
            Nivel = proximo.Nivel,
            Segmento = proximo.Segmento,
            HayDisponible = proximo.HayDisponible,
            HijasIgnoradas = proximo.HijosIgnorados,
        });
    }

    public async Task<OperationResponse<CuentaContableResponse>> Crear(CuentaContableRequest request)
    {
        TCuentaContable? padre = null;

        if (request.IdCtaContableRel is not null)
        {
            padre = await ConsultaBase().FirstOrDefaultAsync(c => c.IdCtaContable == request.IdCtaContableRel);

            if (padre is null) return NotFound<CuentaContableResponse>("La cuenta padre no existe.");
        }

        var nivelPadre = padre?.Nivel ?? CodigoCuenta.NivelSegunCodigo(padre?.Codigo);
        var nivel = request.Nivel ?? nivelPadre + 1;

        // Sin codigo explicito se toma el proximo libre: es el camino normal desde el arbol.
        var codigo = request.Codigo?.Trim();

        if (string.IsNullOrWhiteSpace(codigo))
        {
            var codigosHijas = await ConsultaBase()
                .Where(c => padre == null ? c.IdCtaContableRel == null : c.IdCtaContableRel == padre.IdCtaContable)
                .Select(c => c.Codigo)
                .ToListAsync();

            var proximo = ReglasProximoCodigo.Calcular(padre?.Codigo, nivelPadre, codigosHijas);

            if (!proximo.HayDisponible)
            {
                return BadRequest<CuentaContableResponse>(
                    $"No quedan codigos libres en el nivel {proximo.Nivel} bajo la cuenta indicada.");
            }

            codigo = proximo.Codigo;
            nivel = proximo.Nivel;
        }

        var contexto = new ContextoCuenta(
            CodigoYaExiste: await Existe(_ejecucionContext.TCuentasContables.Where(c => c.Codigo == codigo)),
            PadreExiste: padre is not null,
            CodigoPadre: padre?.Codigo,
            NivelPadre: padre?.Nivel,
            PadreEsImputable: padre?.EsImputable ?? false);

        var datos = new DatosCuenta(codigo, request.Nombre, nivel, request.TipoSaldo, request.Imputable, request.Activa);
        var validacion = ReglasJerarquiaCuentas.ValidarAlta(datos, contexto);

        if (!validacion.EsValida) return BadRequest<CuentaContableResponse>(validacion.Mensaje);

        var cuenta = new TCuentaContable
        {
            // Oracle no usa identidad: el id se pide a la secuencia antes de insertar.
            IdCtaContable = await GetSecuencia(_ejecucionContext, Secuencia),
            IdCtaContableRel = padre?.IdCtaContable,
            Codigo = codigo!,
            Nombre = request.Nombre.Trim(),
            Nivel = nivel,
            Activa = request.Activa ? Activa : Inactiva,
            Imputable = request.Imputable ? Si : No,
            TipoSaldo = request.TipoSaldo,
            IdNaturaleza = request.IdNaturaleza ?? padre?.IdNaturaleza,
            IdRecursoDebe = request.IdRecursoDebe,
            IdRecursoHaber = request.IdRecursoHaber,
            Auxiliar = request.Auxiliar ? Si : No,
            AsientoOps = request.AsientoOps ? Si : No,
            Inventariable = request.Inventariable ? Si : No,
            IdCtaBancaria = request.IdCtaBancaria,
            IdProveedor = request.IdProveedor,
            Amortizacion = request.Amortizacion,
            IngresoPropio = request.IngresoPropio,
            IdJurisdiccion = padre?.IdJurisdiccion ?? GetJurisdiccionCache(),
            IdVigencia = padre?.IdVigencia ?? GetIdVigencia(),
            UserIng = GetUserCache(),
            FecIng = DateTime.Now,
        };

        _ejecucionContext.TCuentasContables.Add(cuenta);
        await _ejecucionContext.SaveChangesAsync();

        return Ok(Mapear(cuenta, 0));
    }

    public async Task<OperationResponse<CuentaContableResponse>> Editar(int idCtaContable, CuentaContableRequest request)
    {
        var cuenta = await _ejecucionContext.TCuentasContables
            .FirstOrDefaultAsync(c => c.IdCtaContable == idCtaContable);

        if (cuenta is null) return NotFound<CuentaContableResponse>("La cuenta no existe.");

        var hijas = await ContarHijas(idCtaContable);

        // El codigo y el padre no se tocan desde la edicion: mover una cuenta de lugar arrastra a
        // toda su rama y a los asientos ya imputados. Para eso hay que dar de baja y volver a crear.
        var contexto = new ContextoCuenta(
            CodigoYaExiste: false,
            PadreExiste: cuenta.IdCtaContableRel is not null,
            CodigoPadre: await CodigoDe(cuenta.IdCtaContableRel),
            NivelPadre: null,
            CantidadDeHijas: hijas,
            TieneMovimientos: await TieneMovimientos(idCtaContable));

        var datos = new DatosCuenta(
            cuenta.Codigo, request.Nombre, cuenta.Nivel, request.TipoSaldo, request.Imputable, request.Activa);

        var validacion = ReglasJerarquiaCuentas.ValidarEdicion(datos, contexto);

        if (!validacion.EsValida) return BadRequest<CuentaContableResponse>(validacion.Mensaje);

        cuenta.Nombre = request.Nombre.Trim();
        cuenta.Activa = request.Activa ? Activa : Inactiva;
        cuenta.Imputable = request.Imputable ? Si : No;
        cuenta.TipoSaldo = request.TipoSaldo;
        cuenta.IdNaturaleza = request.IdNaturaleza;
        cuenta.IdRecursoDebe = request.IdRecursoDebe;
        cuenta.IdRecursoHaber = request.IdRecursoHaber;
        cuenta.Auxiliar = request.Auxiliar ? Si : No;
        cuenta.AsientoOps = request.AsientoOps ? Si : No;
        cuenta.Inventariable = request.Inventariable ? Si : No;
        cuenta.IdCtaBancaria = request.IdCtaBancaria;
        cuenta.IdProveedor = request.IdProveedor;
        cuenta.Amortizacion = request.Amortizacion;
        cuenta.IngresoPropio = request.IngresoPropio;
        cuenta.UserAct = GetUserCache();
        cuenta.FecAct = DateTime.Now;

        await _ejecucionContext.SaveChangesAsync();

        return Ok(Mapear(cuenta, hijas));
    }

    public async Task<OperationResponse<bool>> Eliminar(int idCtaContable)
    {
        var cuenta = await _ejecucionContext.TCuentasContables
            .FirstOrDefaultAsync(c => c.IdCtaContable == idCtaContable);

        if (cuenta is null) return NotFound<bool>("La cuenta no existe.");

        var contexto = new ContextoCuenta(
            CantidadDeHijas: await ContarHijas(idCtaContable),
            TieneMovimientos: await TieneMovimientos(idCtaContable));

        var validacion = ReglasJerarquiaCuentas.ValidarEliminacion(contexto);

        if (!validacion.EsValida) return BadRequest(validacion.Mensaje, false);

        _ejecucionContext.TCuentasContables.Remove(cuenta);
        await _ejecucionContext.SaveChangesAsync();

        return Ok(true);
    }

    // ---- Auxiliares ------------------------------------------------------

    /// <summary>
    /// Base de toda consulta del plan. Acota a la jurisdiccion del usuario cuando hay token; sin
    /// token (solo en desarrollo, con UseAuth apagado) devuelve todo.
    /// </summary>
    /// <summary>
    /// Carga masiva del plan desde una planilla.
    /// </summary>
    /// <remarks>
    /// Con `Confirmar` en false solo evalua y devuelve que pasaria; en true crea las cuentas. Es el
    /// mismo patron que el cierre de ejercicio: primero se mira, despues se confirma.
    ///
    /// La validacion **se rehace del lado del servidor** aunque el front ya haya interpretado el
    /// archivo. Lo que manda el navegador no se da por bueno.
    ///
    /// Todo el alta va en **una sola transaccion**: o entra el archivo entero o no entra nada. Un
    /// plan de cuentas a medias, con ramas colgando de padres que no se llegaron a crear, es peor
    /// que no haber importado.
    /// </remarks>
    public async Task<OperationResponse<ImportacionPlanResponse>> Importar(ImportacionPlanRequest request)
    {
        var filas = request.Filas ?? [];

        if (filas.Count == 0)
            return BadRequest<ImportacionPlanResponse>("El archivo no tiene ninguna fila.");

        if (filas.Count > ReglasImportacionPlanDeCuentas.MaximoDeFilas)
        {
            return BadRequest<ImportacionPlanResponse>(
                $"El archivo tiene {filas.Count} filas y el maximo es " +
                $"{ReglasImportacionPlanDeCuentas.MaximoDeFilas}. Conviene partirlo.");
        }

        var existentes = await ConsultaBase()
            .Select(c => new { c.Codigo, c.Imputable })
            .ToListAsync();

        var codigos = new HashSet<string>(
            existentes.Select(c => CodigoCuenta.Normalizar(c.Codigo)), StringComparer.Ordinal);

        var imputables = new HashSet<string>(
            existentes.Where(c => c.Imputable == Si).Select(c => CodigoCuenta.Normalizar(c.Codigo)),
            StringComparer.Ordinal);

        var plan = ReglasImportacionPlanDeCuentas.Evaluar(
            filas.Select(f => new FilaImportada(f.Fila, f.Codigo, f.Nombre, f.TipoSaldo, f.Imputable, f.IdNaturaleza)),
            codigos,
            imputables);

        var respuesta = MapearPlan(plan);

        if (!request.Confirmar) return Ok(respuesta);

        if (plan.ACrear == 0)
            return BadRequest<ImportacionPlanResponse>("No hay ninguna cuenta para crear.", respuesta);

        var creadas = await CrearDesdeElPlan(plan);

        respuesta.Confirmado = true;
        respuesta.Creadas = creadas;

        return Ok(respuesta);
    }

    private async Task<int> CrearDesdeElPlan(PlanDeImportacionDeCuentas plan)
    {
        // El id del padre se resuelve sobre la marcha: una cuenta creada en esta misma importacion
        // todavia no esta en la base cuando se procesa su hija, asi que se va guardando por codigo.
        var idsPorCodigo = await ConsultaBase()
            .Select(c => new { c.Codigo, c.IdCtaContable, c.IdJurisdiccion, c.IdVigencia, c.IdNaturaleza })
            .ToListAsync();

        var idPorCodigo = idsPorCodigo
            .GroupBy(c => CodigoCuenta.Normalizar(c.Codigo), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().IdCtaContable, StringComparer.Ordinal);

        var datosPorCodigo = idsPorCodigo
            .GroupBy(c => CodigoCuenta.Normalizar(c.Codigo), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var jurisdiccion = GetJurisdiccionCache();
        var vigencia = GetIdVigencia();
        var usuario = GetUserCache();
        var ahora = DateTime.Now;

        await using var transaccion = await _ejecucionContext.Database.BeginTransactionAsync();

        var creadas = 0;

        foreach (var fila in plan.ParaCrear)
        {
            int? idPadre = null;

            if (!string.IsNullOrEmpty(fila.CodigoPadre) && idPorCodigo.TryGetValue(fila.CodigoPadre, out var id))
                idPadre = id;

            datosPorCodigo.TryGetValue(fila.CodigoPadre, out var padre);

            var cuenta = new TCuentaContable
            {
                IdCtaContable = await GetSecuencia(_ejecucionContext, Secuencia),
                IdCtaContableRel = idPadre,
                Codigo = fila.Codigo,
                Nombre = fila.Nombre!.Trim(),
                Nivel = fila.Nivel,
                Activa = Activa,
                Imputable = fila.Imputable ? Si : No,
                TipoSaldo = fila.TipoSaldo,
                // La naturaleza se hereda del padre si el archivo no la trae: es lo mismo que hace
                // el alta de a una, y evita tener que repetirla en cada fila de la planilla.
                IdNaturaleza = fila.IdNaturaleza ?? padre?.IdNaturaleza,
                Auxiliar = No,
                AsientoOps = No,
                Inventariable = No,
                Amortizacion = 0,
                IngresoPropio = 0,
                IdJurisdiccion = padre?.IdJurisdiccion ?? jurisdiccion,
                IdVigencia = padre?.IdVigencia ?? vigencia,
                UserIng = usuario,
                FecIng = ahora,
            };

            _ejecucionContext.TCuentasContables.Add(cuenta);
            await _ejecucionContext.SaveChangesAsync();

            idPorCodigo[fila.Codigo] = cuenta.IdCtaContable;
            datosPorCodigo[fila.Codigo] = new
            {
                cuenta.Codigo,
                cuenta.IdCtaContable,
                cuenta.IdJurisdiccion,
                cuenta.IdVigencia,
                cuenta.IdNaturaleza,
            };

            creadas++;
        }

        await transaccion.CommitAsync();

        return creadas;
    }

    private static ImportacionPlanResponse MapearPlan(PlanDeImportacionDeCuentas plan) => new()
    {
        Total = plan.Total,
        ACrear = plan.ACrear,
        YaExisten = plan.YaExisten,
        Rechazadas = plan.Rechazadas,
        Confirmado = false,
        Creadas = 0,
        Filas = [.. plan.Filas.Select(f => new FilaDeImportacionResponse
        {
            Fila = f.Fila,
            Codigo = string.IsNullOrEmpty(f.Codigo) ? f.CodigoOriginal : f.Codigo,
            Nombre = f.Nombre,
            Nivel = f.Nivel,
            TipoSaldo = f.TipoSaldo,
            Imputable = f.Imputable,
            Estado = f.Estado.ToString(),
            Motivo = f.Motivo,
        })],
    };

    private IQueryable<TCuentaContable> ConsultaBase()
    {
        var consulta = _ejecucionContext.TCuentasContables.AsNoTracking();
        var jurisdiccion = GetJurisdiccionCache();

        return jurisdiccion > 0 ? consulta.Where(c => c.IdJurisdiccion == jurisdiccion) : consulta;
    }

    private async Task<int> ContarHijas(int idCtaContable)
        => await _ejecucionContext.TCuentasContables.CountAsync(c => c.IdCtaContableRel == idCtaContable);

    /// <summary>
    /// Existe al menos una fila que cumpla la condicion.
    /// </summary>
    /// <remarks>
    /// No se usa AnyAsync: el proveedor de Oracle lo traduce a una comparacion contra el literal
    /// FALSE, que Oracle 12.2 no entiende y responde ORA-00904. Los literales booleanos en SQL
    /// recien aparecen en 23c. Take(1) + CountAsync corta despues de la primera fila.
    /// </remarks>
    private static async Task<bool> Existe<TEntidad>(IQueryable<TEntidad> consulta)
        => await consulta.Take(1).CountAsync() > 0;

    private async Task<bool> TieneMovimientos(int idCtaContable)
        => await Existe(_ejecucionContext.TAsientosDetalle.Where(d => d.IdCtaContable == idCtaContable));

    /// <summary>
    /// Agrega a la lista las cuentas padre que quedaron afuera del filtro, subiendo hasta la raiz.
    /// Devuelve los ids de las que se sumaron.
    /// </summary>
    /// <remarks>
    /// El plan de Morteros mezcla jurisdicciones dentro del mismo arbol: PASIVO y PREVISIONES
    /// CORRIENTES son de la jurisdiccion 4000 y tienen hijas de la 2437. Sin esto, filtrar por la
    /// jurisdiccion del usuario parte el arbol en pedazos y las hijas huerfanas se reportan como
    /// anomalias del dato, cuando en realidad son un corte del propio filtro.
    /// </remarks>
    private async Task<HashSet<int>> CompletarAncestros(List<TCuentaContable> cuentas)
    {
        var agregadas = new HashSet<int>();
        var presentes = cuentas.Select(c => c.IdCtaContable).ToHashSet();

        while (true)
        {
            var faltantes = cuentas
                .Where(c => c.IdCtaContableRel is not null
                            && c.IdCtaContableRel != c.IdCtaContable
                            && !presentes.Contains(c.IdCtaContableRel.Value))
                .Select(c => c.IdCtaContableRel!.Value)
                .Distinct()
                .ToList();

            if (faltantes.Count == 0) break;

            var ancestros = await _ejecucionContext.TCuentasContables
                .AsNoTracking()
                .Where(c => faltantes.Contains(c.IdCtaContable))
                .ToListAsync();

            // Si el padre no existe en ninguna jurisdiccion, es una anomalia real y la deja pasar
            // ArbolDeCuentas: cortar aca evita un bucle infinito.
            if (ancestros.Count == 0) break;

            foreach (var ancestro in ancestros)
            {
                cuentas.Add(ancestro);
                presentes.Add(ancestro.IdCtaContable);
                agregadas.Add(ancestro.IdCtaContable);
            }
        }

        return agregadas;
    }

    private async Task<string?> CodigoDe(int? idCtaContable)
        => idCtaContable is null
            ? null
            : await _ejecucionContext.TCuentasContables
                .Where(c => c.IdCtaContable == idCtaContable)
                .Select(c => c.Codigo)
                .FirstOrDefaultAsync();

    private static CuentaContableResponse Mapear(TCuentaContable c, int cantidadDeHijas) => new()
    {
        IdCtaContable = c.IdCtaContable,
        IdCtaContableRel = c.IdCtaContableRel,
        Codigo = c.Codigo,
        CodigoNormalizado = CodigoCuenta.Normalizar(c.Codigo),
        Segmentos = [.. CodigoCuenta.Descomponer(c.Codigo)],
        Nombre = c.Nombre,
        Nivel = c.Nivel,
        Activa = c.EstaActiva,
        Imputable = c.EsImputable,
        TipoSaldo = c.TipoSaldo,
        IdNaturaleza = c.IdNaturaleza,
        IdRecursoDebe = c.IdRecursoDebe,
        IdRecursoHaber = c.IdRecursoHaber,
        Auxiliar = c.Auxiliar == Si,
        AsientoOps = c.AsientoOps == Si,
        Inventariable = c.Inventariable == Si,
        IdCtaBancaria = c.IdCtaBancaria,
        IdProveedor = c.IdProveedor,
        Amortizacion = c.Amortizacion,
        IngresoPropio = c.IngresoPropio,
        IdJurisdiccion = c.IdJurisdiccion,
        IdVigencia = c.IdVigencia,
        CantidadDeHijas = cantidadDeHijas,
        UserIng = c.UserIng,
        FecIng = c.FecIng,
        UserAct = c.UserAct,
        FecAct = c.FecAct,
    };

    private static CuentaArbolResponse MapearNodo(NodoCuenta<TCuentaContable> nodo, HashSet<int> deContexto) => new()
    {
        IdCtaContable = nodo.Cuenta.IdCtaContable,
        IdCtaContableRel = nodo.Cuenta.IdCtaContableRel,
        Codigo = nodo.Cuenta.Codigo,
        Nombre = nodo.Cuenta.Nombre,
        Nivel = nodo.Cuenta.Nivel,
        Profundidad = nodo.Profundidad,
        Activa = nodo.Cuenta.EstaActiva,
        Imputable = nodo.Cuenta.EsImputable,
        TipoSaldo = nodo.Cuenta.TipoSaldo,
        Anomalias = DescribirAnomalias(nodo.Anomalias),
        FueraDeAlcance = deContexto.Contains(nodo.Cuenta.IdCtaContable),
        Hijas = [.. nodo.Hijas.Select(h => MapearNodo(h, deContexto))],
    };

    private static List<string> DescribirAnomalias(AnomaliaJerarquia anomalias)
    {
        if (anomalias == AnomaliaJerarquia.Ninguna) return [];

        return
        [
            .. Enum.GetValues<AnomaliaJerarquia>()
                .Where(a => a != AnomaliaJerarquia.Ninguna && anomalias.HasFlag(a))
                .Select(Describir)
        ];
    }

    private static string Describir(AnomaliaJerarquia anomalia) => anomalia switch
    {
        AnomaliaJerarquia.SeApuntaASiMisma => "La cuenta figura como padre de si misma.",
        AnomaliaJerarquia.PadreInexistente => "La cuenta padre no existe en el plan.",
        AnomaliaJerarquia.FormaUnCiclo => "La cadena de cuentas padre vuelve sobre si misma.",
        AnomaliaJerarquia.NivelInconsistente => "El nivel no es el del padre mas uno.",
        AnomaliaJerarquia.CodigoNoCuelgaDelPadre => "El codigo no cuelga del codigo de la cuenta padre.",
        AnomaliaJerarquia.ImputableConHijas => "Esta marcada como imputable pero tiene cuentas hijas.",
        _ => anomalia.ToString(),
    };
}
