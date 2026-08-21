namespace Contable.Domain.ModelsPresupuesto;

/// <summary>Partida de gasto. Solo lectura desde este modulo.</summary>
public class TObjetoGasto
{
    public int IdObjetoGasto { get; set; }

    public int? IdObjetoGastoRel { get; set; }

    /// <summary>Numero estable entre vigencias: es la clave de la traduccion entre ejercicios.</summary>
    public int? NumeroObjeto { get; set; }

    public string? NombreObjeto { get; set; }

    public int? IdVigencia { get; set; }

    public int? IdJurisdiccion { get; set; }

    /// <summary>'S' en las hojas del clasificador, que son las unicas que se imputan.</summary>
    public string? UltimoNivel { get; set; }

    public string? ImputaEjecucion { get; set; }

    [NotMapped]
    public bool EsUltimoNivel => UltimoNivel == "S";
}
