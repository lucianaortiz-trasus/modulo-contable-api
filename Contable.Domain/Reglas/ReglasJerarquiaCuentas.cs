namespace Contable.Domain.Reglas;

/// <summary>Resultado de una validacion de negocio.</summary>
public class Validacion
{
    public List<string> Errores { get; } = [];

    public bool EsValida => Errores.Count == 0;

    public string Mensaje => string.Join(" ", Errores);

    public Validacion Agregar(string error)
    {
        Errores.Add(error);
        return this;
    }

    public Validacion AgregarSi(bool condicion, string error)
    {
        if (condicion) Errores.Add(error);
        return this;
    }
}

/// <summary>Datos de una cuenta que se quiere dar de alta o modificar.</summary>
/// <param name="Codigo">Codigo completo, ya compuesto.</param>
/// <param name="Nombre">Descripcion de la cuenta.</param>
/// <param name="Nivel">Nivel declarado.</param>
/// <param name="TipoSaldo">'D' deudor o 'A' acreedor.</param>
/// <param name="Imputable">Si recibe movimientos.</param>
/// <param name="Activa">Si esta habilitada.</param>
public record DatosCuenta(
    string? Codigo,
    string? Nombre,
    int? Nivel,
    string? TipoSaldo,
    bool Imputable,
    bool Activa);

/// <summary>Contexto que hace falta consultar en la base para poder validar.</summary>
/// <param name="CodigoYaExiste">Si el codigo ya lo usa otra cuenta.</param>
/// <param name="PadreExiste">Si la cuenta padre existe.</param>
/// <param name="CodigoPadre">Codigo de la cuenta padre.</param>
/// <param name="NivelPadre">Nivel de la cuenta padre.</param>
/// <param name="PadreEsImputable">Si el padre esta marcado como imputable.</param>
/// <param name="CantidadDeHijas">Cuentas que ya cuelgan de la cuenta que se valida.</param>
/// <param name="TieneMovimientos">Si la cuenta ya recibio asientos.</param>
public record ContextoCuenta(
    bool CodigoYaExiste = false,
    bool PadreExiste = false,
    string? CodigoPadre = null,
    int? NivelPadre = null,
    bool PadreEsImputable = false,
    int CantidadDeHijas = 0,
    bool TieneMovimientos = false);

/// <summary>
/// Reglas estructurales del plan de cuentas.
/// </summary>
/// <remarks>
/// Estas validaciones corren al escribir, no al leer. El plan que ya esta cargado en Morteros las
/// incumple en varios puntos: catorce cuentas con el nivel desalineado del padre, catorce cuyo
/// codigo no cuelga del padre y tres imputables con hijas. Aplicarlas hacia atras dejaria el plan
/// existente inutilizable, asi que las cuentas que ya estan se leen como estan y las anomalias se
/// informan por ArbolDeCuentas, que es lo que despues alimenta la pantalla de auditoria.
/// </remarks>
public static class ReglasJerarquiaCuentas
{
    public const string SaldoDeudor = "D";

    public const string SaldoAcreedor = "A";

    public static readonly string[] TiposDeSaldoValidos = [SaldoDeudor, SaldoAcreedor];

    // T_CUENTAS_CONTABLES no usa una sola convencion para sus marcas de un caracter: ACTIVA va con
    // A / I mientras IMPUTABLE y las demas van con S / N. Esta a la vista en el ABM viejo
    // (t_cuentas_contablesABM.aspx.cs, lineas 273 y 392). No unificar sin migrar los datos: las
    // 350 cuentas del plan tienen ACTIVA = 'A'.

    public const string CuentaActiva = "A";

    public const string CuentaInactiva = "I";

    public const string MarcaSi = "S";

    public const string MarcaNo = "N";

    /// <summary>Valida el alta de una cuenta.</summary>
    public static Validacion ValidarAlta(DatosCuenta datos, ContextoCuenta contexto)
    {
        var validacion = ValidarCamposComunes(datos, contexto);

        var nivel = datos.Nivel ?? 0;

        if (nivel > 1)
        {
            validacion.AgregarSi(!contexto.PadreExiste, "La cuenta padre no existe.");

            if (contexto.PadreExiste)
            {
                validacion.AgregarSi(
                    contexto.NivelPadre is not null && nivel != contexto.NivelPadre + 1,
                    $"Una cuenta de nivel {nivel} tiene que colgar de una de nivel {nivel - 1}.");

                validacion.AgregarSi(
                    contexto.PadreEsImputable,
                    "La cuenta padre esta marcada como imputable. Una cuenta que recibe movimientos no puede tener hijas.");

                validacion.AgregarSi(
                    !CodigoCuenta.CuelgaDe(datos.Codigo, contexto.CodigoPadre),
                    $"El codigo {datos.Codigo} no cuelga del codigo de la cuenta padre ({contexto.CodigoPadre}).");
            }
        }

        return validacion;
    }

    /// <summary>Valida la edicion de una cuenta que ya existe.</summary>
    public static Validacion ValidarEdicion(DatosCuenta datos, ContextoCuenta contexto)
    {
        var validacion = ValidarCamposComunes(datos, contexto);

        validacion.AgregarSi(
            datos.Imputable && contexto.CantidadDeHijas > 0,
            $"No se puede marcar como imputable: la cuenta tiene {contexto.CantidadDeHijas} cuenta(s) hija(s).");

        validacion.AgregarSi(
            !datos.Activa && contexto.TieneMovimientos,
            "La cuenta tiene movimientos registrados. Desactivarla no borra los asientos ya imputados.");

        return validacion;
    }

    /// <summary>Valida que la cuenta se pueda eliminar.</summary>
    public static Validacion ValidarEliminacion(ContextoCuenta contexto)
    {
        var validacion = new Validacion();

        validacion.AgregarSi(
            contexto.CantidadDeHijas > 0,
            $"No se puede eliminar: la cuenta tiene {contexto.CantidadDeHijas} cuenta(s) hija(s).");

        validacion.AgregarSi(
            contexto.TieneMovimientos,
            "No se puede eliminar: la cuenta tiene asientos imputados.");

        return validacion;
    }

    private static Validacion ValidarCamposComunes(DatosCuenta datos, ContextoCuenta contexto)
    {
        var validacion = new Validacion();

        validacion.AgregarSi(string.IsNullOrWhiteSpace(datos.Nombre), "El nombre es obligatorio.");

        validacion.AgregarSi(
            datos.Nombre is not null && datos.Nombre.Length > 500,
            "El nombre no puede superar los 500 caracteres.");

        validacion.AgregarSi(string.IsNullOrWhiteSpace(datos.Codigo), "El codigo es obligatorio.");

        validacion.AgregarSi(
            !string.IsNullOrWhiteSpace(datos.Codigo) && !CodigoCuenta.TieneFormatoValido(datos.Codigo),
            $"El codigo {datos.Codigo} no tiene el formato esperado (NN.NN.NN.NN.NN.NN.NNNN).");

        validacion.AgregarSi(contexto.CodigoYaExiste, $"Ya existe una cuenta con el codigo {datos.Codigo}.");

        var nivel = datos.Nivel ?? 0;

        validacion.AgregarSi(
            !CodigoCuenta.EsNivelValido(nivel),
            $"El nivel tiene que estar entre 1 y {CodigoCuenta.NivelMaximo}.");

        if (CodigoCuenta.EsNivelValido(nivel) && !string.IsNullOrWhiteSpace(datos.Codigo))
        {
            validacion.AgregarSi(
                CodigoCuenta.NivelSegunCodigo(datos.Codigo) != nivel,
                $"El codigo {datos.Codigo} tiene {CodigoCuenta.NivelSegunCodigo(datos.Codigo)} segmento(s) pero el nivel declarado es {nivel}.");
        }

        validacion.AgregarSi(
            !TiposDeSaldoValidos.Contains(datos.TipoSaldo),
            "El tipo de saldo tiene que ser D (deudor) o A (acreedor).");

        return validacion;
    }
}
