namespace Contable.Application.ResponseDto.Dtos.PlanDeCuentas;

/// <summary>Lo que va a pasar —o lo que paso— con una carga masiva del plan.</summary>
public class ImportacionPlanResponse
{
    public int Total { get; set; }

    /// <summary>Cuantas se van a crear, o se crearon si ya estaba confirmado.</summary>
    public int ACrear { get; set; }

    /// <summary>Codigos que ya estaban en el plan. No se tocan.</summary>
    public int YaExisten { get; set; }

    public int Rechazadas { get; set; }

    /// <summary>`false` mientras es solo una vista previa.</summary>
    public bool Confirmado { get; set; }

    /// <summary>Cuentas efectivamente creadas. Cero en la vista previa.</summary>
    public int Creadas { get; set; }

    public List<FilaDeImportacionResponse> Filas { get; set; } = [];
}

public class FilaDeImportacionResponse
{
    public int Fila { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string? Nombre { get; set; }

    public int Nivel { get; set; }

    public string TipoSaldo { get; set; } = string.Empty;

    public bool Imputable { get; set; }

    /// <summary>`Crear`, `YaExiste` o `Rechazada`.</summary>
    public string Estado { get; set; } = string.Empty;

    public string? Motivo { get; set; }
}
