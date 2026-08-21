namespace Contable.Application;

/// <summary>
/// Configuracion de la aplicacion, cargada una sola vez al arrancar.
/// </summary>
public abstract class AppSettings
{
    public class Jwt
    {
        public static string SecretKey { get; set; } = string.Empty;

        public static string? Issuer { get; set; }

        public static string? Audience { get; set; }

        public static int Minutes { get; set; }
    }

    public class Uses
    {
        public static bool UseJwt { get; set; }

        public static bool UseAuth { get; set; }

        public static bool UseSwagger { get; set; }
    }

    /// <summary>
    /// Federacion de la sesion con el Portal SIGEMyT.
    /// </summary>
    /// <remarks>
    /// El Portal manda al usuario al modulo con <c>?token=...&amp;idJur=...&amp;numJur=...</c>. Para
    /// el modulo ese token es <b>opaco</b>: no se desencripta nunca, se reenvia tal cual a
    /// <see cref="AuthUrl"/> y el Portal responde quien es el usuario. Por eso el formato interno
    /// del token puede cambiar sin tocar este modulo.
    ///
    /// Es el mismo endpoint que consumen los otros once modulos de la plataforma.
    /// </remarks>
    public class Portal
    {
        /// <summary>URL completa de <c>api/Seguridad/Login</c> del Portal, sin el querystring.</summary>
        public static string AuthUrl { get; set; } = string.Empty;

        /// <summary>Segundos de espera antes de dar por perdida la validacion contra el Portal.</summary>
        public static int TimeoutSegundos { get; set; }
    }

    /// <summary>
    /// Parametros contables que en el sistema viejo estaban escritos a mano dentro del PL/SQL y
    /// que cambian de un municipio a otro. Ver _analisis/REGLAS.md seccion 2.
    /// </summary>
    public class Contabilidad
    {
        /// <summary>
        /// Cuenta que recibe la diferencia residual del asiento de cierre. En el PL/SQL provincial
        /// estaba fija en 11270, id que no existe en Morteros.
        /// </summary>
        public static int CuentaBalanceoCierre { get; set; }

        /// <summary>
        /// Unidad administrativa a la que se imputan cierre y apertura. El PL/SQL la buscaba por
        /// el literal 'Administracion Central', que en Morteros no existe.
        /// </summary>
        public static int UnidadAdministrativaCierre { get; set; }

        /// <summary>Recurso debe/haber del renglon de balanceo cuando el saldo es acreedor.</summary>
        public static int RecursoBalanceoHaber { get; set; }

        /// <summary>Recurso debe/haber del renglon de balanceo cuando el saldo es deudor.</summary>
        public static int RecursoBalanceoDebe { get; set; }

        /// <summary>Naturalezas de cuentas de resultado. Provincial: 4 y 5.</summary>
        public static int[] NaturalezasResultado { get; set; } = [];

        /// <summary>Naturalezas patrimoniales. Provincial: 1 y 2.</summary>
        public static int[] NaturalezasPatrimoniales { get; set; } = [];

        /// <summary>Id del tipo de asiento de cierre.</summary>
        public static int TipoAsientoCierre { get; set; }

        /// <summary>
        /// Id del tipo de asiento de apertura. El PL/SQL escribia la apertura con el tipo de
        /// cierre, y despues la releia como si fuera un cierre. Ver defecto D3.
        /// </summary>
        public static int TipoAsientoApertura { get; set; }

        /// <summary>
        /// Si los renglones anulados suman en saldos y reportes. El sistema viejo los incluye;
        /// se corrige y la desviacion queda documentada en los tests de paridad. Ver D14.
        /// </summary>
        public static bool IncluirAnuladosEnSaldos { get; set; }
    }

    /// <summary>Vuelca la configuracion del appsettings a las propiedades estaticas.</summary>
    public static void Bind(IConfiguration configuration)
    {
        Jwt.SecretKey = configuration["Jwt:SecretKey"] ?? string.Empty;
        Jwt.Issuer = configuration["Jwt:Issuer"];
        Jwt.Audience = configuration["Jwt:Audience"];
        Jwt.Minutes = configuration.GetValue<int>("Jwt:Minutes");

        Uses.UseJwt = configuration.GetValue<bool>("Uses:UseJwt");
        Uses.UseAuth = configuration.GetValue<bool>("Uses:UseAuth");
        Uses.UseSwagger = configuration.GetValue<bool>("Uses:UseSwagger");

        Portal.AuthUrl = configuration["Portal:AuthUrl"] ?? string.Empty;
        Portal.TimeoutSegundos = configuration.GetValue<int?>("Portal:TimeoutSegundos") ?? 15;

        Contabilidad.CuentaBalanceoCierre = configuration.GetValue<int>("Contabilidad:CuentaBalanceoCierre");
        Contabilidad.UnidadAdministrativaCierre = configuration.GetValue<int>("Contabilidad:UnidadAdministrativaCierre");
        Contabilidad.RecursoBalanceoHaber = configuration.GetValue<int>("Contabilidad:RecursoBalanceoHaber");
        Contabilidad.RecursoBalanceoDebe = configuration.GetValue<int>("Contabilidad:RecursoBalanceoDebe");
        Contabilidad.NaturalezasResultado = configuration.GetSection("Contabilidad:NaturalezasResultado").Get<int[]>() ?? [];
        Contabilidad.NaturalezasPatrimoniales = configuration.GetSection("Contabilidad:NaturalezasPatrimoniales").Get<int[]>() ?? [];
        Contabilidad.TipoAsientoCierre = configuration.GetValue<int>("Contabilidad:TipoAsientoCierre");
        Contabilidad.TipoAsientoApertura = configuration.GetValue<int>("Contabilidad:TipoAsientoApertura");
        Contabilidad.IncluirAnuladosEnSaldos = configuration.GetValue<bool>("Contabilidad:IncluirAnuladosEnSaldos");
    }
}
