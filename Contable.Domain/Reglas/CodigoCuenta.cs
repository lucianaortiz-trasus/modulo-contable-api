namespace Contable.Domain.Reglas;

/// <summary>
/// Composicion y descomposicion del codigo del plan de cuentas.
/// </summary>
/// <remarks>
/// El codigo es posicional: siete segmentos separados por punto, de dos digitos cada uno salvo el
/// septimo que lleva cuatro. El ABM viejo mostraba justamente siete cuadros de texto y leia cada
/// nivel por posicion fija (PKG_T_CUENTAS_CONTABLES.TRAER_COD_NIVEL_COM).
///
/// Cuidado con el nivel 1: la base lo guarda sin completar con ceros ("1" en vez de "01"), asi que
/// el codigo del padre no es prefijo del codigo del hijo justo en el salto de nivel 1 a nivel 2.
/// Por eso toda comparacion entre padre e hijo se hace sobre la forma normalizada.
/// </remarks>
public static class CodigoCuenta
{
    /// <summary>Cantidad de digitos de cada segmento, del nivel 1 al 7.</summary>
    public static readonly int[] AnchoDeSegmento = [2, 2, 2, 2, 2, 2, 4];

    public const int NivelMaximo = 7;

    public const char Separador = '.';

    /// <summary>Valor mas alto que admite el segmento de un nivel: 99, salvo el nivel 7 que llega a 9999.</summary>
    public static int ValorMaximo(int nivel) => (int)Math.Pow(10, AnchoDeSegmento[nivel - 1]) - 1;

    /// <summary>
    /// Largo que tiene un codigo completo hasta el nivel indicado, ya normalizado.
    /// Nivel 1 son 2 caracteres, nivel 2 son 5, nivel 3 son 8, y asi hasta 22 en el nivel 7.
    /// </summary>
    public static int LargoNormalizado(int nivel)
    {
        if (!EsNivelValido(nivel)) throw new ArgumentOutOfRangeException(nameof(nivel));

        var largo = 0;
        for (var i = 0; i < nivel; i++) largo += AnchoDeSegmento[i] + (i > 0 ? 1 : 0);

        return largo;
    }

    public static bool EsNivelValido(int nivel) => nivel >= 1 && nivel <= NivelMaximo;

    /// <summary>
    /// Parte el codigo en sus segmentos. Acepta tanto la forma guardada como la normalizada.
    /// </summary>
    public static string[] Descomponer(string? codigo)
        => string.IsNullOrWhiteSpace(codigo)
            ? []
            : codigo.Split(Separador, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Completa cada segmento con ceros a la izquierda hasta el ancho que le corresponde.
    /// Es la forma que hay que usar para comparar, nunca la que viene de la base.
    /// </summary>
    public static string Normalizar(string? codigo)
    {
        var segmentos = Descomponer(codigo);
        if (segmentos.Length == 0) return string.Empty;

        var normalizados = segmentos
            .Take(NivelMaximo)
            .Select((seg, i) => seg.Trim().PadLeft(AnchoDeSegmento[i], '0'));

        return string.Join(Separador, normalizados);
    }

    /// <summary>Arma el codigo a partir de los valores numericos de cada nivel.</summary>
    public static string Componer(IEnumerable<int> segmentos)
    {
        var valores = segmentos.ToArray();

        if (valores.Length == 0 || valores.Length > NivelMaximo)
        {
            throw new ArgumentException($"Un codigo tiene entre 1 y {NivelMaximo} segmentos.", nameof(segmentos));
        }

        var partes = valores.Select((valor, i) =>
        {
            if (valor < 0 || valor > ValorMaximo(i + 1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segmentos),
                    $"El segmento de nivel {i + 1} admite valores entre 0 y {ValorMaximo(i + 1)}.");
            }

            return valor.ToString().PadLeft(AnchoDeSegmento[i], '0');
        });

        return string.Join(Separador, partes);
    }

    /// <summary>Agrega un segmento al codigo de la cuenta padre.</summary>
    public static string ComponerDesdePadre(string? codigoPadre, int nivelHijo, int valor)
    {
        if (!EsNivelValido(nivelHijo)) throw new ArgumentOutOfRangeException(nameof(nivelHijo));

        if (valor < 1 || valor > ValorMaximo(nivelHijo))
        {
            throw new ArgumentOutOfRangeException(
                nameof(valor),
                $"El segmento de nivel {nivelHijo} admite valores entre 1 y {ValorMaximo(nivelHijo)}.");
        }

        var segmento = valor.ToString().PadLeft(AnchoDeSegmento[nivelHijo - 1], '0');

        // El nivel 1 no tiene padre: el codigo es el segmento solo.
        if (nivelHijo == 1) return segmento;

        var prefijo = Normalizar(codigoPadre);

        if (string.IsNullOrEmpty(prefijo))
        {
            throw new ArgumentException("Una cuenta de nivel 2 o mayor necesita el codigo del padre.", nameof(codigoPadre));
        }

        return $"{prefijo}{Separador}{segmento}";
    }

    /// <summary>
    /// Nivel que se deduce del codigo por su cantidad de segmentos. Devuelve 0 si el codigo esta vacio.
    /// </summary>
    public static int NivelSegunCodigo(string? codigo) => Descomponer(codigo).Length;

    /// <summary>
    /// Valor numerico del segmento del nivel pedido, o null si el codigo no llega a ese nivel o el
    /// segmento no es numerico.
    /// </summary>
    public static int? SegmentoDeNivel(string? codigo, int nivel)
    {
        if (!EsNivelValido(nivel)) return null;

        var segmentos = Descomponer(codigo);
        if (segmentos.Length < nivel) return null;

        return int.TryParse(segmentos[nivel - 1], out var valor) ? valor : null;
    }

    /// <summary>
    /// Indica si el codigo del hijo cuelga del codigo del padre, comparando en forma normalizada.
    /// </summary>
    public static bool CuelgaDe(string? codigoHijo, string? codigoPadre)
    {
        var hijo = Normalizar(codigoHijo);
        var padre = Normalizar(codigoPadre);

        if (string.IsNullOrEmpty(hijo) || string.IsNullOrEmpty(padre)) return false;

        return hijo.StartsWith(padre + Separador, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifica que el codigo tenga la forma esperada: segmentos numericos y del ancho correcto.
    /// </summary>
    public static bool TieneFormatoValido(string? codigo)
    {
        var segmentos = Descomponer(codigo);

        if (segmentos.Length == 0 || segmentos.Length > NivelMaximo) return false;

        for (var i = 0; i < segmentos.Length; i++)
        {
            var seg = segmentos[i];

            // Se acepta el nivel 1 sin completar con ceros porque asi esta cargado el plan actual.
            var anchoAceptable = i == 0 ? seg.Length is >= 1 and <= 2 : seg.Length == AnchoDeSegmento[i];

            if (!anchoAceptable) return false;
            if (!seg.All(char.IsDigit)) return false;
        }

        return true;
    }
}
