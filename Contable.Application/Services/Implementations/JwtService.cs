namespace Contable.Application.Services.Implementations;

/// <summary>
/// Emision y lectura del token del modulo.
/// </summary>
/// <remarks>
/// El portal SIGEMyT deja el usuario en sessionStorage; el modulo lo lee, pide aca su propio
/// token y con el resuelve jurisdiccion, unidad administrativa y vigencia activa.
/// </remarks>
public class JwtService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), IJwtService
{
    public async Task<OperationResponse<string>> CrearTokenAdmin(int idUsuario, int? idJurisdiccionDelPortal = null)
    {
        var usuario = await _portalContext.TUsuariosNew
            .AsNoTracking()
            .Include(x => x.Persona)
            .FirstOrDefaultAsync(x => x.UsuarioId == idUsuario);

        if (usuario is null) return NotFound<string>("El usuario no existe en el portal.");

        // La vigencia de trabajo es la que tiene ACTIVO_EJECUCION = 'S'. No se deduce del ano ni
        // se recibe por parametro: es el ejercicio que Presupuesto tiene abierto para ejecutar.
        var vigencia = await _presupuestoContext.TVigencias
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ActivoEjecucion == "S");

        if (vigencia is null) return BadRequest<string>("No hay una vigencia abierta para ejecucion.");

        var jurisdiccion = await ResolverJurisdiccion(
            vigencia.IdVigencia,
            idJurisdiccionDelPortal is > 0 ? idJurisdiccionDelPortal : usuario.Persona?.IdJurisdiccion);

        if (jurisdiccion.Error is not null) return BadRequest<string>(jurisdiccion.Error);

        var idJurisdiccion = jurisdiccion.Id;

        var idUnidadAdministrativa = await _presupuestoContext.TUnidadesAdministrativa
            .AsNoTracking()
            .Where(x => x.IdJurisdiccion == idJurisdiccion)
            .Select(x => x.IdUnidadAdm)
            .FirstOrDefaultAsync();

        var roles = await _portalContext.RolesUsuariosNew
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuario.UsuarioId)
            .Include(x => x.Rol)
            .Select(x => x.Rol.Nombre)
            .ToListAsync();

        var persona = usuario.Persona;

        var claims = new List<Claim>
        {
            new("IdUsuario", usuario.UsuarioId.ToString()),
            new("NombreUsuario", usuario.NombreUsuario ?? string.Empty),
            new("IdPersona", usuario.IdPersona ?? string.Empty),
            new("DNI", persona?.DocumentoNro ?? string.Empty),
            new("Cuil", persona?.Cuil ?? string.Empty),
            new("Email", persona?.Email ?? string.Empty),
            new("ApellidoYNombre", $"{persona?.Apellido} {persona?.Nombre}".Trim()),
            new("IdJurisdiccion", idJurisdiccion.ToString()),
            new("IdUnidadAdministrativa", idUnidadAdministrativa.ToString()),
            new("IdVigencia", vigencia.IdVigencia.ToString()),
            new("Roles", string.Join(",", roles)),
        };

        var token = GenerarToken(
            new ClaimsIdentity(claims),
            AppSettings.Jwt.Audience!,
            AppSettings.Jwt.Issuer!,
            AppSettings.Jwt.SecretKey,
            AppSettings.Jwt.Minutes);

        return Ok(token);
    }

    /// <summary>
    /// Datos del usuario del token con el que vino el pedido.
    /// </summary>
    /// <remarks>
    /// Lee del principal autenticado, no de un token que llegue por parametro. Antes recibia el
    /// token en el querystring y eso tenia dos problemas: la credencial quedaba en los logs del
    /// servidor y en el historial del navegador, y el endpoint decodificaba cualquier JWT que le
    /// mandaran sin validar la firma.
    ///
    /// Ademas era la causa de un 401 en produccion: el endpoint no esta marcado como anonimo, la
    /// politica de respaldo exige usuario autenticado cuando <c>Uses:UseAuth</c> esta en true, y el
    /// interceptor del front lo excluia de mandar el header. En desarrollo no se veia porque alli
    /// <c>UseAuth</c> esta en false.
    /// </remarks>
    public OperationResponse<UsuarioResponse> GetDataToken()
    {
        string Claim(string tipo) => _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(x => x.Type == tipo)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(Claim("IdUsuario")))
            return Unauthorized<UsuarioResponse>("El pedido no trae un token valido.");

        var datos = new UsuarioResponse
        {
            UsuarioId = Claim("IdUsuario"),
            NombreUsuario = Claim("NombreUsuario"),
            PersonaId = Claim("IdPersona"),
            Dni = Claim("DNI"),
            Cuil = Claim("Cuil"),
            Email = Claim("Email"),
            ApellidoYNombre = Claim("ApellidoYNombre"),
            JurisdiccionId = Claim("IdJurisdiccion"),
            IdUnidadAdministrativa = Claim("IdUnidadAdministrativa"),
            IdVigencia = Claim("IdVigencia"),
            Roles = [.. Claim("Roles").Split(',', StringSplitOptions.RemoveEmptyEntries)],
        };

        return Ok(datos);
    }

    /// <summary>
    /// Jurisdiccion con la que el usuario va a trabajar, siempre dentro de la vigencia activa.
    /// </summary>
    /// <remarks>
    /// La regla es: la vigencia sale de <c>ACTIVO_EJECUCION = 'S'</c> y la jurisdiccion tiene que
    /// pertenecer <b>a esa vigencia</b>. Lo que el portal manda en <c>idJur</c> puede ser de un
    /// ejercicio anterior —el portal manda la del ejercicio donde esta parado el usuario, y en
    /// Morteros llega la de 2024— asi que no se usa tal cual: sirve para saber <b>cual</b> de las
    /// jurisdicciones de la vigencia activa corresponde, a traves del numero, que es lo unico
    /// estable entre ejercicios.
    ///
    /// Antes esto era un `FirstOrDefaultAsync` suelto y cuando no encontraba nada devolvia
    /// <b>0</b>. El token salia con jurisdiccion 0, el modulo filtraba todo por 0 y se veia entero
    /// vacio, sin un solo mensaje. Ahora cada camino sin salida dice que paso.
    /// </remarks>
    private async Task<(int Id, string? Error)> ResolverJurisdiccion(int idVigencia, int? idJurisdiccionOrigen)
    {
        var deLaVigencia = await _presupuestoContext.TJurisdicciones
            .AsNoTracking()
            .Where(j => j.IdVigencia == idVigencia)
            .Select(j => new { j.IdJurisdiccion, j.NumeroJurisdiccion })
            .ToListAsync();

        if (deLaVigencia.Count == 0)
            return (0, "El ejercicio abierto no tiene ninguna jurisdiccion cargada en Presupuesto.");

        var numero = idJurisdiccionOrigen is null or 0
            ? null
            : await _presupuestoContext.TJurisdicciones
                .AsNoTracking()
                .Where(j => j.IdJurisdiccion == idJurisdiccionOrigen)
                .Select(j => j.NumeroJurisdiccion)
                .FirstOrDefaultAsync();

        var equivalente = numero is null
            ? null
            : deLaVigencia.FirstOrDefault(j => j.NumeroJurisdiccion == numero);

        if (equivalente is not null) return (equivalente.IdJurisdiccion, null);

        // Sin equivalencia. Si el ejercicio abierto tiene una sola jurisdiccion no hay ambiguedad
        // posible y se usa esa; si tiene varias, elegir una al azar seria peor que no entrar.
        if (deLaVigencia.Count == 1) return (deLaVigencia[0].IdJurisdiccion, null);

        return (0,
            $"La jurisdiccion con la que entraste no existe en el ejercicio abierto, y ese " +
            $"ejercicio tiene {deLaVigencia.Count} jurisdicciones, asi que el modulo no puede " +
            $"elegir una. Avisale al area de sistemas.");
    }

    private static string GenerarToken(ClaimsIdentity claims, string audience, string issuer, string secretKey, int minutes)
    {
        var key = Encoding.ASCII.GetBytes(secretKey);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(minutes),
            NotBefore = DateTime.UtcNow,
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = audience,
            Issuer = issuer,
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
