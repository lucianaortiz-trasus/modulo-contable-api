namespace Contable.Application.ResponseDto.Dtos.Auditoria;

/// <summary>Resultado de correr los controles de auditoria sobre un ejercicio.</summary>
public class AuditoriaResponse
{
    public int Ejercicio { get; set; }

    public int Errores { get; set; }

    public int Advertencias { get; set; }

    public int Sugerencias { get; set; }

    public int ControlesCorridos { get; set; }

    /// <summary>Controles que no encontraron nada. Tambien es informacion.</summary>
    public int ControlesLimpios { get; set; }

    public List<GrupoResponse> Grupos { get; set; } = [];
}

public class GrupoResponse
{
    public string Tipo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Severidad { get; set; } = string.Empty;

    public string Explicacion { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    /// <summary>La lista viene recortada: hay mas hallazgos que los que se muestran.</summary>
    public bool Recortado { get; set; }

    public List<HallazgoResponse> Hallazgos { get; set; } = [];
}

public class HallazgoResponse
{
    public string Descripcion { get; set; } = string.Empty;

    public int? IdAsiento { get; set; }

    public int? Numero { get; set; }

    public int? IdCtaContable { get; set; }

    public string? Codigo { get; set; }

    public decimal? Importe { get; set; }
}
