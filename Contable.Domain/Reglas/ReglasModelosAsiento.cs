namespace Contable.Domain.Reglas;

public static class ReglasModelosAsiento
{
    public const string Si = "S";
    public const string No = "N";

    public const string OrigenPlanCuentas = "PLAN_CUENTAS";
    public const string OrigenMatrizContable = "MATRIZ_CONTABLE";

    public const string AccionGenerar = "GENERAR";
    public const string AccionModificar = "MODIFICAR";

    public const string TextoDefinidaEnMatriz = "Definida en la matriz";
    public const string TooltipMatriz =
        "La cuenta se determina por la relación [bien/servicio] - [objeto del gasto] - [cuenta contable del plan de cuentas]";
    public const string TextoFormula =
        "[Total Facturado] = [Neto Gravado] + [Exento] + [Total IVA] + [Total Otros Tributos]";

    public const string MensajeCombinacionDuplicada =
        "Ya existe un modelo de asiento contable registrado para esta operación y tipo de documento";
    public const string MensajeSinDatos = "No se encontraron datos";
    public const string MensajeSinPlantilla =
        "No hay una plantilla definida para esa combinación de operación, documento y tipo de comprobante.";

    /// <summary>Datos de la combinacion que identifica a un modelo.</summary>
    /// <param name="IdTipoOperacion">Id de T_TIPOS_OPERACION_ASIENTO.</param>
    /// <param name="IdTipodoc">Id de T_TIPODOC.</param>
    /// <param name="IdTipodocLetra">Id de T_TIPODOC_LETRA; nulo si el documento no tiene letras.</param>
    /// <param name="DiscriminaOt">Si discrimina otros tributos.</param>
    public record DatosCombinacion(
        int? IdTipoOperacion,
        int? IdTipodoc,
        int? IdTipodocLetra,
        bool DiscriminaOt);

    /// <summary>Lo que hay que consultar para validar una combinacion.</summary>
    /// <param name="OperacionExiste">La operacion existe en el catalogo.</param>
    /// <param name="DocumentoExiste">El documento existe y esta vigente.</param>
    /// <param name="LetraExiste">La letra existe, esta activa y pertenece al documento.</param>
    /// <param name="DocumentoTieneLetras">El documento tiene letras activas en el catalogo.</param>
    /// <param name="CombinacionYaExiste">Ya hay un modelo activo con esta combinacion.</param>
    public record ContextoCombinacion(
        bool OperacionExiste = false,
        bool DocumentoExiste = false,
        bool LetraExiste = false,
        bool DocumentoTieneLetras = false,
        bool CombinacionYaExiste = false);

    /// <summary>Una cuenta del plan, con lo justo para decidir si se puede imputar.</summary>
    public record CuentaParaValidacion(
        int IdCtaContable,
        string? Codigo,
        string? Nombre,
        bool Activa,
        bool Imputable)
    {
        public string Etiqueta => string.IsNullOrWhiteSpace(Codigo) ? Nombre ?? string.Empty : $"{Codigo} - {Nombre}";
    }

    /// <summary>Un renglon de la plantilla con la cuenta que eligio el usuario.</summary>
    public record RenglonParaValidacion(
        int IdPlantillaAsientoDetalle,
        string OrigenCuenta,
        string Concepto,
        int? IdCtaContable);

    /// <summary>Un cambio de cuenta para el comentario del historial.</summary>
    public record CambioDeCuenta(string Concepto, string? CuentaAnterior, string? CuentaNueva);

    /// <summary>
    /// Clave estable de la combinacion. La letra nula se normaliza a 0, igual que hace el
    /// indice unico de Oracle, para que "sin letra" y "letra 0" no convivan como dos modelos.
    /// </summary>
    public static string Clave(DatosCombinacion datos)
        => $"{datos.IdTipoOperacion ?? 0}|{datos.IdTipodoc ?? 0}|{datos.IdTipodocLetra ?? 0}|{(datos.DiscriminaOt ? Si : No)}";

    public static Validacion ValidarCombinacion(DatosCombinacion datos, ContextoCombinacion contexto)
    {
        var validacion = new Validacion();

        validacion.AgregarSi(datos.IdTipoOperacion is null or <= 0, "Hay que indicar el tipo de operación.");
        validacion.AgregarSi(datos.IdTipodoc is null or <= 0, "Hay que indicar el tipo de documento.");

        if (datos.IdTipoOperacion is > 0)
            validacion.AgregarSi(!contexto.OperacionExiste, "El tipo de operación no existe en el catálogo.");

        if (datos.IdTipodoc is > 0)
            validacion.AgregarSi(!contexto.DocumentoExiste, "El tipo de documento no existe o no está vigente.");

        if (contexto.DocumentoTieneLetras)
        {
            validacion.AgregarSi(
                datos.IdTipodocLetra is null or <= 0,
                "Hay que indicar el tipo de comprobante para este tipo de documento.");

            if (datos.IdTipodocLetra is > 0)
                validacion.AgregarSi(!contexto.LetraExiste, "El tipo de comprobante no existe o no está activo para ese documento.");
        }
        else
        {
            // El selector va bloqueado: no se puede mandar una letra para un documento que no las tiene.
            validacion.AgregarSi(
                datos.IdTipodocLetra is > 0,
                "El tipo de documento indicado no admite tipo de comprobante.");
        }

        validacion.AgregarSi(contexto.CombinacionYaExiste, MensajeCombinacionDuplicada);

        return validacion;
    }

    /// <summary>
    /// Valida las cuentas de los renglones PLAN_CUENTAS. Los MATRIZ_CONTABLE se ignoran: su
    /// cuenta se resuelve al generar el asiento, no al parametrizar el modelo.
    /// </summary>
    public static Validacion ValidarCuentas(
        IEnumerable<RenglonParaValidacion> renglones,
        IReadOnlyDictionary<int, CuentaParaValidacion> cuentas)
    {
        var validacion = new Validacion();

        foreach (var renglon in renglones.Where(r => r.OrigenCuenta == OrigenPlanCuentas))
        {
            var rotulo = $"En el concepto {renglon.Concepto}";

            if (renglon.IdCtaContable is null or <= 0)
            {
                validacion.Agregar($"{rotulo} hay que indicar la cuenta contable.");
                continue;
            }

            if (!cuentas.TryGetValue(renglon.IdCtaContable.Value, out var cuenta))
            {
                validacion.Agregar($"{rotulo} la cuenta contable no existe.");
                continue;
            }

            validacion.AgregarSi(
                !cuenta.Activa,
                $"{rotulo} la cuenta {cuenta.Etiqueta} está dada de baja.");

            validacion.AgregarSi(
                !cuenta.Imputable,
                $"{rotulo} la cuenta {cuenta.Etiqueta} no es imputable.");
        }

        return validacion;
    }

    /// <summary>Texto del historial cuando se da de alta un modelo.</summary>
    public static string FormatearComentarioGenerar() => "Se crea el modelo de asiento contable";

    /// <summary>
    /// Texto del historial al modificar: una línea por cada cuenta cambiada, en el orden de la
    /// plantilla. Si no hay cambios, se deja constancia de que se guardó igual.
    /// </summary>
    public static string FormatearComentarioModificar(IEnumerable<CambioDeCuenta> cambios)
    {
        var lineas = cambios
            .Select(c => $"En el concepto {c.Concepto} se cambia la cuenta contable de {c.CuentaAnterior} a {c.CuentaNueva}")
            .ToList();

        return lineas.Count == 0
            ? "Se modifica el modelo de asiento contable"
            : string.Join("\n", lineas);
    }
}
