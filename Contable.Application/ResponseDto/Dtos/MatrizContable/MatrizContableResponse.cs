namespace Contable.Application.ResponseDto.Dtos.MatrizContable;

/// <summary>Una fila de la matriz, con la cuenta y la partida ya resueltas.</summary>
public class MatrizContableResponse
{
    public int IdMatrizCble { get; set; }

    public int? IdCtaContable { get; set; }

    public string? CodigoCuenta { get; set; }

    public string? NombreCuenta { get; set; }

    public int? IdObjetoGasto { get; set; }

    public long? NumeroObjeto { get; set; }

    public string? NombreObjeto { get; set; }

    public int? IdRecurso { get; set; }

    public long? NumeroRecurso { get; set; }

    public string? NombreRecurso { get; set; }

    /// <summary>"Gasto" o "Recurso". Vacio si la fila esta mal cargada.</summary>
    public string? TipoPartida { get; set; }

    public bool Activa { get; set; } = true;

    public string Estado => Activa ? "Activa" : "Inactiva";

    /// <summary>Indica si la cuenta contable tiene asientos registrados.</summary>
    public bool TieneAsientos { get; set; }

    /// <summary>La edición solo está disponible si la cuenta no tiene asientos contables registrados.</summary>
    public bool PermiteEditar => !TieneAsientos;

    public int? IdVigencia { get; set; }

    public int? Ejercicio { get; set; }

    public string? UserIng { get; set; }

    public DateTime? FecIng { get; set; }

    public string? UserAct { get; set; }

    public DateTime? FecAct { get; set; }
}

/// <summary>Una partida presupuestaria, para los buscadores de la pantalla.</summary>
public class PartidaResponse
{
    public int Id { get; set; }

    public long? Numero { get; set; }

    public string? Nombre { get; set; }

    public int? IdVigencia { get; set; }
}

/// <summary>Lo que va a pasar si se confirma la importacion entre ejercicios.</summary>
public class PlanImportacionResponse
{
    public int EjercicioOrigen { get; set; }

    public int EjercicioDestino { get; set; }

    public int TotalOrigen { get; set; }

    public int AImportar { get; set; }

    public int Gastos { get; set; }

    public int Recursos { get; set; }

    /// <summary>Relaciones que en el ejercicio destino ya estaban cargadas.</summary>
    public int YaExistentes { get; set; }

    public List<FilaNoImportableResponse> NoImportables { get; set; } = [];

    /// <summary>True cuando la importacion ya se ejecuto; false si es solo la vista previa.</summary>
    public bool Confirmada { get; set; }
}

public class FilaNoImportableResponse
{
    public int IdMatrizOrigen { get; set; }

    public string? TipoPartida { get; set; }

    public long? NumeroPartida { get; set; }

    public string Motivo { get; set; } = string.Empty;
}

/// <summary>Nodo de árbol de partida presupuestaria (Objeto de Gasto o Recurso) para modales de selección.</summary>
public class PartidaArbolResponse
{
    public int Id { get; set; }

    public int? IdPadre { get; set; }

    public long? Numero { get; set; }

    public string Codigo => Numero?.ToString() ?? string.Empty;

    public string? Nombre { get; set; }

    public bool EsHoja { get; set; }

    /// <summary>Indica si la partida ya posee una relación activa en la matriz contable para este ejercicio.</summary>
    public bool YaRelacionada { get; set; }

    public List<PartidaArbolResponse> Hijas { get; set; } = [];
}

/// <summary>Estado de partidas sin relacionar en la matriz contable.</summary>
public class PendientesMatrizResponse
{
    public bool HayPendientes => TotalPendientes > 0;

    public int TotalPendientes => GastosSinRelacionar + RecursosSinRelacionar;

    public int GastosSinRelacionar { get; set; }

    public int RecursosSinRelacionar { get; set; }
}
