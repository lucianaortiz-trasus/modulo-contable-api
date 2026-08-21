using System.Text.Json.Serialization;

namespace Contable.Application.ResponseDto.Dtos.Portal;

// ==============================================================================================
// CONTRATO DEL PORTAL SIGEMyT — LO DEFINE EL PORTAL, NO ESTE MODULO
// ----------------------------------------------------------------------------------------------
// Es lo que devuelve GET {Portal:AuthUrl}?atoken=. Los nombres del JSON estan congelados del lado
// del Portal y verificados alla campo por campo.
//
// Se deserializa solo lo que este modulo necesita. Agregar campos del lado del Portal es seguro:
// los que no estan aca se ignoran. Lo que no se puede es renombrar.
//
// EL RIESGO ES QUE FALLA EN SILENCIO: una propiedad que no matchea no da error, queda en 0 o en
// null. Si "jurisdiccionId" estuviera mal escrito se leeria como 0 y el modulo filtraria todo por
// la jurisdiccion 0. Por eso cada nombre va con [JsonPropertyName] explicito y no se deriva de la
// convencion de serializacion.
// ==============================================================================================

public class SesionDelPortalResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("usuario")]
    public UsuarioDelPortal? Usuario { get; set; }
}

public class UsuarioDelPortal
{
    [JsonPropertyName("usuarioId")]
    public int UsuarioId { get; set; }

    [JsonPropertyName("nombreUsuario")]
    public string? NombreUsuario { get; set; }

    /// <summary>El campo critico, con j minuscula. De aca sale la jurisdiccion del usuario.</summary>
    [JsonPropertyName("jurisdiccionId")]
    public int JurisdiccionId { get; set; }

    [JsonPropertyName("personaId")]
    public string? PersonaId { get; set; }

    /// <summary>Numero, no texto: el Portal serializa el enum por su valor.</summary>
    [JsonPropertyName("estado")]
    public int Estado { get; set; }

    /// <summary>
    /// Los roles del acceso de Administracion Financiera, que es donde vive lo contable.
    /// Va siempre, vacia si el usuario no tiene ninguno.
    /// </summary>
    [JsonPropertyName("rolesAdministracionFinanciera")]
    public List<RolDelPortal> RolesAdministracionFinanciera { get; set; } = [];
}

public class RolDelPortal
{
    [JsonPropertyName("rolId")]
    public int RolId { get; set; }

    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }
}
