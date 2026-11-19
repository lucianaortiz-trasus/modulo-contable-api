namespace Contable.Application.RequestDto.Comun;

/// <summary>Pagina y tamaño de pagina comunes a los filtros de grilla.</summary>
public class PaginacionRequest
{
    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 20;
}
