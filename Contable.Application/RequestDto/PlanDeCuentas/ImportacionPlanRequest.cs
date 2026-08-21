using System.ComponentModel.DataAnnotations;

namespace Contable.Application.RequestDto.PlanDeCuentas;

/// <summary>
/// Carga masiva del plan de cuentas.
/// </summary>
/// <remarks>
/// El archivo lo interpreta el front, que ya tiene la libreria de Excel cargada para exportar. Lo
/// que llega aca son las filas ya leidas: el backend no parsea planillas, pero **valida todo de
/// nuevo**. Lo que manda el navegador no se da por bueno.
/// </remarks>
public class ImportacionPlanRequest
{
    [Required]
    public List<FilaDelArchivoRequest> Filas { get; set; } = [];

    /// <summary>
    /// En `false` solo se evalua y se devuelve que pasaria. En `true` se crean las cuentas.
    /// Mismo patron que el cierre de ejercicio: primero se mira, despues se confirma.
    /// </summary>
    public bool Confirmar { get; set; }
}

public class FilaDelArchivoRequest
{
    /// <summary>Numero de fila en la planilla, para poder senalar el error donde esta.</summary>
    public int Fila { get; set; }

    public string? Codigo { get; set; }

    public string? Nombre { get; set; }

    /// <summary>'D' deudor o 'A' acreedor. Se acepta tambien la palabra completa.</summary>
    public string? TipoSaldo { get; set; }

    /// <summary>S/N, y las variantes habituales que se escriben a mano.</summary>
    public string? Imputable { get; set; }

    public int? IdNaturaleza { get; set; }
}
