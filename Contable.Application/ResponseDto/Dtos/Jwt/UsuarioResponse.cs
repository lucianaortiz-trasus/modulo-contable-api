namespace Contable.Application.ResponseDto.Dtos.Jwt;

/// <summary>
/// Identidad del usuario tal como la consume el front. Mismo contrato que el modulo de Tesoreria,
/// para que el portal y los modulos hablen igual.
/// </summary>
public class UsuarioResponse
{
    public string JurisdiccionId { get; set; } = string.Empty;

    public string IdUnidadAdministrativa { get; set; } = string.Empty;

    public string IdVigencia { get; set; } = string.Empty;

    public string UsuarioId { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string PersonaId { get; set; } = string.Empty;

    public string Dni { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Cuil { get; set; } = string.Empty;

    public string ApellidoYNombre { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}
