namespace Contable.Domain.ModelsEjecucion;

/// <summary>
/// Catalogo de tipos de asiento. Tabla EJECUCION.T_TIPO_ASIENTO.
/// </summary>
/// <remarks>
/// El codigo 'AP' esta duplicado en dos filas (AUTOMATICO PURO y DE APERTURA): buscar siempre
/// por Id, nunca por Codigo.
/// </remarks>
public class TTipoAsiento
{
    public int IdTipoAsiento { get; set; }

    public string? Codigo { get; set; }

    public string? Descripcion { get; set; }
}
