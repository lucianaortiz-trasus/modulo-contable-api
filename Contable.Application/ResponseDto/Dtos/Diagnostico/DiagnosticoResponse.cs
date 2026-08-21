namespace Contable.Application.ResponseDto.Dtos.Diagnostico;

/// <summary>
/// Foto del estado de los datos contables. Sirve de prueba de humo del armado: si esto responde,
/// la cadena controller / service / EF Core / Oracle esta completa.
/// </summary>
public class DiagnosticoResponse
{
    public string Modulo { get; set; } = "Contable SIGEMyT";

    public string Entorno { get; set; } = string.Empty;

    public int? EjercicioActivo { get; set; }

    public int? IdVigenciaActiva { get; set; }

    public int CuentasContables { get; set; }

    public int CuentasImputables { get; set; }

    public List<CuentasPorNivel> PlanDeCuentasPorNivel { get; set; } = [];

    public int Asientos { get; set; }

    public int AsientosDetalle { get; set; }

    public int FilasMatrizContable { get; set; }

    public List<TipoAsientoResumen> TiposDeAsiento { get; set; } = [];
}

public class CuentasPorNivel
{
    public int? Nivel { get; set; }

    public int Cantidad { get; set; }
}

public class TipoAsientoResumen
{
    public int Id { get; set; }

    public string? Codigo { get; set; }

    public string? Descripcion { get; set; }
}
