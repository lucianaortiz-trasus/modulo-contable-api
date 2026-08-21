namespace Contable.Domain.ModelsPresupuesto;

/// <summary>Partida de recurso. Solo lectura desde este modulo.</summary>
public class TRecurso
{
    public int IdRecurso { get; set; }

    public int? IdRecursoRel { get; set; }

    /// <summary>Numero estable entre vigencias: es la clave de la traduccion entre ejercicios.</summary>
    public long? NumeroRecurso { get; set; }

    public string? NombreRecurso { get; set; }

    public int? IdVigencia { get; set; }

    public int? IdJurisdiccion { get; set; }
}
