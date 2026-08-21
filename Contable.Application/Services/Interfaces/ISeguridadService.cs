namespace Contable.Application.Services.Interfaces;

public interface ISeguridadService
{
    /// <summary>
    /// Canjea el token que trae el usuario desde el Portal SIGEMyT por el token propio del modulo.
    /// </summary>
    /// <param name="atoken">
    /// Token que el Portal puso en el querystring. Para este modulo es <b>opaco</b>: no se
    /// desencripta, se reenvia tal cual al Portal.
    /// </param>
    /// <param name="idJurisdiccion">
    /// El <c>idJur</c> del querystring: la jurisdiccion que el usuario eligio al entrar.
    /// </param>
    Task<OperationResponse<string>> LoginFederado(string? atoken, int? idJurisdiccion);
}
