namespace Contable.Application.Services.Interfaces;

public interface IJwtService
{
    /// <summary>Emite el token propio del modulo para el usuario que ya autentico el portal.</summary>
    /// <param name="idUsuario">Id del usuario en el portal.</param>
    /// <param name="idJurisdiccionDelPortal">
    /// El <c>idJur</c> con el que el portal mando al usuario. Si viene, gana sobre la jurisdiccion
    /// guardada en la persona. Puede ser de un ejercicio anterior: se traduce internamente.
    /// </param>
    Task<OperationResponse<string>> CrearTokenAdmin(int idUsuario, int? idJurisdiccionDelPortal = null);

    /// <summary>
    /// Datos del usuario del token con el que vino el pedido. Se leen del principal autenticado:
    /// el token no viaja por querystring.
    /// </summary>
    OperationResponse<UsuarioResponse> GetDataToken();
}
