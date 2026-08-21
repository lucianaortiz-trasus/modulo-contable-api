namespace Contable.Application.RequestDto.MatrizContable;

/// <summary>Alta o edicion de una fila de la matriz contable.</summary>
public class MatrizContableRequest
{
    public int? IdCtaContable { get; set; }

    /// <summary>Partida de gasto. Excluyente con IdRecurso.</summary>
    public int? IdObjetoGasto { get; set; }

    /// <summary>Partida de recurso. Excluyente con IdObjetoGasto.</summary>
    public int? IdRecurso { get; set; }

    /// <summary>Si se omite, se toma la vigencia del token.</summary>
    public int? IdVigencia { get; set; }
}

/// <summary>Filtros de la grilla de la matriz.</summary>
public class FiltroMatrizRequest
{
    public int? IdVigencia { get; set; }

    public int? IdCtaContable { get; set; }

    public int? IdObjetoGasto { get; set; }

    public int? IdRecurso { get; set; }

    /// <summary>"Gasto", "Recurso" o vacio para las dos.</summary>
    public string? TipoPartida { get; set; }

    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 20;
}

/// <summary>Busqueda de partidas presupuestarias para los selectores.</summary>
public class FiltroPartidaRequest
{
    public int? IdVigencia { get; set; }

    public string? Texto { get; set; }

    /// <summary>Solo las partidas hoja del clasificador, que son las unicas imputables.</summary>
    public bool SoloImputables { get; set; } = true;

    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 30;
}
