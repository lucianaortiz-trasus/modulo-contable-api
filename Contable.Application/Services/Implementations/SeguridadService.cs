using System.Net.Http;
using System.Text.Json;
using Contable.Application.ResponseDto.Dtos.Portal;

namespace Contable.Application.Services.Implementations;

/// <summary>
/// Federacion de la sesion con el Portal SIGEMyT.
/// </summary>
/// <remarks>
/// El flujo es el mismo que usan los otros once modulos de la plataforma:
///
/// <code>
/// 1. El usuario entra al Portal        POST api/Seguridad/ValidarUsuario  -> { token, usuario }
/// 2. Clickea el modulo                 window.location = &lt;url&gt;?token=...&amp;idJur=...&amp;numJur=...
/// 3. El front del modulo llama aca     GET  api/Seguridad/Login?atoken=...
/// 4. Este servicio llama al Portal     GET  {Portal:AuthUrl}?atoken=...
/// 5. El Portal responde                200 { token, usuario { ... } }  |  401
/// </code>
///
/// El token del Portal es <b>opaco</b>: se reenvia tal cual y no se desencripta nunca. Por eso el
/// Portal puede cambiar el formato interno sin tocar este modulo.
///
/// Lo que el modulo devuelve no es el token del Portal sino el suyo propio, con los claims de
/// jurisdiccion, unidad administrativa y vigencia ya resueltos.
/// </remarks>
public class SeguridadService(
    EjecucionContext ejecucionContext,
    PresupuestoContext presupuestoContext,
    PortalContext portalContext,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache,
    IHttpClientFactory fabricaDeClientes,
    IJwtService jwtService,
    ILogger<SeguridadService> log)
    : BaseService(ejecucionContext, presupuestoContext, portalContext, httpContextAccessor, cache), ISeguridadService
{
    private readonly IHttpClientFactory _fabricaDeClientes = fabricaDeClientes;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<SeguridadService> _log = log;

    public async Task<OperationResponse<string>> LoginFederado(string? atoken, int? idJurisdiccion)
    {
        if (string.IsNullOrWhiteSpace(atoken))
            return BadRequest<string>("No llego el token del portal.");

        if (string.IsNullOrWhiteSpace(AppSettings.Portal.AuthUrl))
        {
            // Sin esto el modulo no puede validar a nadie. Se dice explicito en vez de devolver un
            // 401 generico, porque son problemas distintos y se arreglan en lugares distintos.
            _log.LogError("Portal:AuthUrl no esta configurado: el modulo no puede federar la sesion.");

            return BadRequest<string>(
                "El modulo no tiene configurada la direccion del portal (Portal:AuthUrl). " +
                "Avisale al area de sistemas.");
        }

        var sesion = await ConsultarAlPortal(atoken);

        if (sesion?.Usuario is null || sesion.Usuario.UsuarioId <= 0)
            return Unauthorized<string>("El portal no reconocio la sesion. Volve a entrar desde el portal.");

        // La jurisdiccion del querystring gana sobre la que trae el usuario del portal: es la que
        // la persona eligio al entrar. Si no vino, queda la del portal.
        var jurisdiccion = idJurisdiccion is > 0 ? idJurisdiccion : sesion.Usuario.JurisdiccionId;

        _log.LogInformation(
            "Sesion federada para {Usuario} (id {Id}), jurisdiccion {Jurisdiccion}.",
            sesion.Usuario.NombreUsuario, sesion.Usuario.UsuarioId, jurisdiccion);

        return await _jwtService.CrearTokenAdmin(sesion.Usuario.UsuarioId, jurisdiccion);
    }

    private async Task<SesionDelPortalResponse?> ConsultarAlPortal(string atoken)
    {
        var cliente = _fabricaDeClientes.CreateClient(NombreDelCliente);

        // El token va como parametro codificado: llega con base64, que trae '+' y '=' y sin
        // codificar se rompe en el camino.
        var url = $"{AppSettings.Portal.AuthUrl.TrimEnd('/')}?atoken={Uri.EscapeDataString(atoken)}";

        try
        {
            var respuesta = await cliente.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode)
            {
                // Sin el token en el log: es una credencial.
                _log.LogWarning("El portal rechazo la federacion con {Codigo}.", (int)respuesta.StatusCode);

                return null;
            }

            var cuerpo = await respuesta.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<SesionDelPortalResponse>(cuerpo, OpcionesDeJson);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "No se pudo consultar al portal en {Url}.", AppSettings.Portal.AuthUrl);

            return null;
        }
    }

    /// <summary>Nombre del HttpClient con nombre que registra <c>ServicesConfig</c>.</summary>
    public const string NombreDelCliente = "Portal";

    /// <summary>
    /// El contrato del Portal viene con <c>[JsonPropertyName]</c> explicito en cada campo, pero se
    /// deja igual sin distinguir mayusculas por si algun ambiente sirve el legacy con otra
    /// convencion de nombres.
    /// </summary>
    private static readonly JsonSerializerOptions OpcionesDeJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
