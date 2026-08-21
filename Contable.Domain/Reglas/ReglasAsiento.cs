namespace Contable.Domain.Reglas;

/// <summary>Cabecera de asiento que se quiere dar de alta.</summary>
/// <param name="FecContable">Fecha de imputacion. De ella salen el ejercicio y el año.</param>
/// <param name="IdUnidadAdm">Unidad administrativa que la pantalla envia.</param>
/// <param name="IdTipoAsiento">Tipo de asiento.</param>
/// <param name="Leyenda">Descripcion del asiento.</param>
public record DatosAsiento(DateTime? FecContable, int? IdUnidadAdm, int? IdTipoAsiento, string? Leyenda);

/// <summary>Lo que hay que resolver contra la base antes de validar la cabecera.</summary>
/// <param name="VigenciaDelAnio">Vigencia cuyo ejercicio coincide con el año de la fecha contable.</param>
/// <param name="EjercicioDelAnio">Ejercicio de esa vigencia.</param>
/// <param name="IdJurisdiccion">Jurisdiccion resuelta desde la unidad administrativa.</param>
/// <param name="TipoAsientoExiste">El tipo de asiento existe en el catalogo.</param>
/// <param name="EjercicioCerrado">La vigencia ya tiene asiento de cierre.</param>
public record ContextoAsiento(
    int? VigenciaDelAnio = null,
    int? EjercicioDelAnio = null,
    int? IdJurisdiccion = null,
    bool TipoAsientoExiste = false,
    bool EjercicioCerrado = false);

/// <summary>
/// Reglas de la cabecera del asiento.
/// </summary>
/// <remarks>
/// Dos comportamientos del PL/SQL que no son evidentes y se conservan porque de ellos depende como
/// quedaron cargados los asientos que ya existen (ver R3 y R4 de _analisis/REGLAS.md):
///
/// - <b>La vigencia y el año no se toman de lo que manda el llamador.</b> `PKG_T_ASIENTOS.INSERTAR`
///   recibe `p_ID_VIGENCIA` y `p_ANIO` y los descarta: los recalcula buscando la vigencia cuyo
///   EJERCICIO coincide con el año de la fecha contable. Cambiar la fecha cambia el ejercicio.
/// - <b>La jurisdiccion se deriva de la unidad administrativa, traducida entre vigencias por
///   NUMERO_UNIDAD_ADM.</b> La tabla guarda ID_JURISDICCION y no ID_UNIDAD_ADM, aunque el
///   procedimiento reciba la unidad.
/// </remarks>
public static class ReglasAsiento
{
    /// <summary>Año contable, que sale de la fecha y no del parametro.</summary>
    public static int? AnioDe(DateTime? fecContable) => fecContable?.Year;

    public static Validacion ValidarAlta(DatosAsiento datos, ContextoAsiento contexto)
    {
        var validacion = new Validacion();

        validacion.AgregarSi(datos.FecContable is null, "Hay que indicar la fecha contable.");
        validacion.AgregarSi(string.IsNullOrWhiteSpace(datos.Leyenda), "La leyenda es obligatoria.");

        validacion.AgregarSi(
            datos.Leyenda is not null && datos.Leyenda.Length > 4000,
            "La leyenda no puede superar los 4000 caracteres.");

        validacion.AgregarSi(datos.IdUnidadAdm is null or <= 0, "Hay que indicar la unidad administrativa.");
        validacion.AgregarSi(datos.IdTipoAsiento is null or <= 0, "Hay que indicar el tipo de asiento.");
        validacion.AgregarSi(
            datos.IdTipoAsiento is > 0 && !contexto.TipoAsientoExiste,
            "El tipo de asiento no existe.");

        if (datos.FecContable is not null)
        {
            validacion.AgregarSi(
                contexto.VigenciaDelAnio is null,
                $"No hay un ejercicio abierto para el año {datos.FecContable.Value.Year}. " +
                "La fecha contable determina el ejercicio del asiento.");

            validacion.AgregarSi(
                contexto.EjercicioCerrado,
                $"El ejercicio {contexto.EjercicioDelAnio} ya tiene asiento de cierre: no admite asientos nuevos.");

            // El ABM viejo rechazaba las fechas futuras con el mismo criterio.
            validacion.AgregarSi(
                datos.FecContable.Value.Date > DateTime.Today,
                "La fecha contable no puede ser posterior al dia de hoy.");
        }

        if (datos.IdUnidadAdm is > 0)
        {
            validacion.AgregarSi(
                contexto.IdJurisdiccion is null or <= 0,
                "No se pudo resolver la jurisdiccion de la unidad administrativa para ese ejercicio.");
        }

        return validacion;
    }
}

/// <summary>
/// Numeracion de asientos.
/// </summary>
/// <remarks>
/// Portada de `EJECUCION.F_NUMERO_ASIENTO`: <c>NVL(MAX(numero), 0) + 1</c> filtrando **solo por
/// jurisdiccion**. Es un correlativo perpetuo: no se reinicia por ejercicio ni por vigencia.
///
/// El original no toma ningun bloqueo, asi que dos altas concurrentes en la misma jurisdiccion
/// obtienen el mismo numero (defecto D4). Aca la regla sigue siendo la misma, pero el servicio la
/// aplica dentro de una transaccion que bloquea la jurisdiccion antes de leer el maximo.
/// </remarks>
public static class ReglasNumeracionAsiento
{
    public static int ProximoNumero(int? maximoDeLaJurisdiccion) => (maximoDeLaJurisdiccion ?? 0) + 1;
}
