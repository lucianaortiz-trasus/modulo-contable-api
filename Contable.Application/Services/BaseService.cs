namespace Contable.Application.Services;

/// <summary>
/// Base comun de los casos de uso. Aporta los contextos, los claims del usuario autenticado,
/// los helpers de respuesta y el acceso a las secuencias Oracle.
/// </summary>
public abstract class BaseService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache)
{
    protected readonly EjecucionContext _ejecucionContext = ejecucionContext;

    protected readonly PresupuestoContext _presupuestoContext = presupuestoContext;

    protected readonly PortalContext _portalContext = portalContext;

    protected readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected readonly IMemoryCache _cache = cache;

    // ---- Respuestas ------------------------------------------------------

    protected static OperationResponse<T> Ok<T>(T data, int total = default)
        => OperationResponse<T>.SuccessResponse(data, total);

    protected static OperationResponse<T> Paginado<T>(T data, int total)
        => OperationResponse<T>.SuccessResponseMassive(data, total);

    protected static OperationResponse<T> NotFound<T>()
        => OperationResponse<T>.NotFoundResponse();

    protected static OperationResponse<T> NotFound<T>(string message)
        => OperationResponse<T>.NotFoundResponse(message);

    protected static OperationResponse<T> BadRequest<T>(string message, T? data = default)
        => OperationResponse<T>.CustomErrorResponse(400, message, data);

    /// <summary>La identidad no es valida. Distinto de 400: no es un dato mal formado.</summary>
    protected static OperationResponse<T> Unauthorized<T>(string message)
        => OperationResponse<T>.CustomErrorResponse(401, message);

    // ---- Claims ----------------------------------------------------------

    /// <summary>Nombre del usuario autenticado. Es el que se graba en los campos de auditoria.</summary>
    protected string GetUserCache() => LeerClaim("NombreUsuario");

    protected string GetCuilCache() => LeerClaim("Cuil");

    protected int GetIdUsuario() => LeerClaimNumerico("IdUsuario");

    protected int GetJurisdiccionCache() => LeerClaimNumerico("IdJurisdiccion");

    protected int GetUnidadAdministrativa() => LeerClaimNumerico("IdUnidadAdministrativa");

    protected int GetIdVigencia() => LeerClaimNumerico("IdVigencia");

    private string LeerClaim(string tipo)
        => _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(x => x.Type == tipo)?.Value ?? string.Empty;

    private int LeerClaimNumerico(string tipo)
        => int.TryParse(LeerClaim(tipo), out var valor) ? valor : 0;

    // ---- Secuencias Oracle ----------------------------------------------

    /// <summary>
    /// Devuelve el proximo valor de una secuencia Oracle. Las tablas de este esquema no usan
    /// identidad: el id se pide antes de insertar.
    /// </summary>
    protected async Task<int> GetSecuencia(DbContext contexto, string secuencia)
    {
        if (!EsNombreDeObjetoValido(secuencia))
        {
            throw new ArgumentException($"Nombre de secuencia no valido: {secuencia}", nameof(secuencia));
        }

        await using var comando = contexto.Database.GetDbConnection().CreateCommand();
        comando.CommandText = $"SELECT {secuencia}.NEXTVAL FROM DUAL";

        if (comando.Connection!.State != System.Data.ConnectionState.Open)
        {
            await contexto.Database.OpenConnectionAsync();
        }

        var resultado = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(resultado);
    }

    /// <summary>
    /// El nombre de la secuencia se interpola en el SQL, asi que no puede venir de una entrada
    /// del usuario sin control.
    /// </summary>
    private static bool EsNombreDeObjetoValido(string nombre)
        => !string.IsNullOrWhiteSpace(nombre)
           && nombre.Length <= 61
           && nombre.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.' || c == '$');
}
