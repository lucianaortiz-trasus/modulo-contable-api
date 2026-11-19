using Contable.Application.RequestDto.ModelosAsiento;
using Contable.Application.ResponseDto.Dtos.ModelosAsiento;

namespace Contable.Application.Services.Interfaces;

/// <summary>Parametrizacion de modelos de asiento contable.</summary>
public interface IModelosAsientoService
{
    /// <summary>Tipos de operacion activos para el formulario.</summary>
    Task<OperationResponse<List<CatalogoItemResponse>>> GetTipoOperaciones();

    /// <summary>Tipos de documento activos, con sus letras (null cuando no tiene).</summary>
    Task<OperationResponse<List<TipoDocumentoCatalogoResponse>>> GetTipoDocumentos();

    /// <summary>Letras activas de un documento. Vacio bloquea el selector.</summary>
    Task<OperationResponse<List<TipoComprobanteResponse>>> GetLetras(int idTipodoc);

    /// <summary>Plan de cuentas activo e imputable para el buscador de cuenta contable.</summary>
    Task<OperationResponse<List<CuentaModeloResponse>>> BuscarCuentas(FiltroCuentasModeloRequest filtro);

    /// <summary>Grilla de la consulta general, ordenada por operacion, documento y comprobante.</summary>
    Task<OperationResponse<List<ModeloAsientoResumenResponse>>> Buscar(FiltroModelosAsientoRequest filtro);

    /// <summary>Devuelve la plantilla predefinida.</summary>
    Task<OperationResponse<ModeloAsientoResponse>> ObtenerPlantilla(ObtenerPlantillaRequest request);

    /// <summary>Alta del modelo con sus cuentas.</summary>
    Task<OperationResponse<ModeloAsientoResponse>> Guardar(GuardarModeloAsientoRequest request);

    /// <summary>Obtiene un modelo con sus cuentas, para verlo o precargar una clonacion.</summary>
    Task<OperationResponse<ModeloAsientoResponse>> ObtenerModelo(int idModeloAsiento);

    /// <summary>Modifica solo las cuentas.</summary>
    Task<OperationResponse<ModeloAsientoResponse>> Editar(int idModeloAsiento, EditarModeloAsientoRequest request);

    /// <summary>Historial del modelo.</summary>
    Task<OperationResponse<List<HistorialModeloResponse>>> Historial(int idModeloAsiento);
}
