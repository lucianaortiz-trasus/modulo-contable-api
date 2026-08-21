namespace Contable.Domain.Reglas;

/// <summary>De dónde salió una sugerencia de cuenta.</summary>
public enum OrigenDeSugerencia
{
    /// <summary>La matriz contable relaciona esa partida con esa cuenta.</summary>
    MatrizContable,

    /// <summary>Asientos anteriores con una leyenda parecida usaron esa cuenta.</summary>
    LeyendaParecida,

    /// <summary>Es de las cuentas que más se usan en el ejercicio.</summary>
    UsoFrecuente,
}

/// <summary>Una cuenta propuesta, con el motivo por el que se propone.</summary>
public record SugerenciaDeCuenta(
    int IdCtaContable,
    decimal Puntaje,
    OrigenDeSugerencia Origen,
    string Motivo);

/// <summary>Un antecedente del historial: qué cuenta se usó con qué leyenda.</summary>
public record AntecedenteDeImputacion(int IdCtaContable, string? Leyenda, int Veces);

/// <summary>
/// Sugerencia de cuenta contable al cargar un asiento.
/// </summary>
/// <remarks>
/// Tres fuentes, en orden de confianza:
///
/// 1. <b>La matriz contable</b>. Si la partida presupuestaria ya tiene cuenta asignada, esa es la
///    respuesta: no es una estimación sino la relación que alguien cargó a propósito.
/// 2. <b>Asientos anteriores con leyenda parecida</b>. Si veinte veces se escribió "pago de
///    energía eléctrica" y las veinte fueron a la misma cuenta, la vigesimoprimera probablemente
///    también.
/// 3. <b>Uso frecuente</b>. Como último recurso, las cuentas más movidas del ejercicio.
///
/// Todo es determinístico y explicable: cada sugerencia dice de dónde salió. En contabilidad una
/// recomendación que no se puede justificar no sirve, porque quien firma el asiento necesita poder
/// defender la imputación.
///
/// El parecido entre leyendas se mide por palabras compartidas, no por distancia de edición: las
/// leyendas contables son frases cortas y repetitivas donde lo que importa es el vocabulario
/// ("energía", "haberes", "proveedor"), no las erratas.
/// </remarks>
public static class ReglasSugerenciaDeImputacion
{
    /// <summary>Palabras demasiado comunes como para que aporten al parecido.</summary>
    private static readonly HashSet<string> Vacias = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "las", "el", "los", "y", "a", "en", "por", "para", "con", "su", "un",
        "una", "al", "se", "que", "sobre", "cta", "nro", "n", "s",
    };

    public const int LargoMinimoDePalabra = 3;

    /// <summary>Debajo de este parecido no se propone nada por leyenda.</summary>
    public const decimal ParecidoMinimo = 0.34m;

    /// <summary>
    /// Arma la lista de cuentas sugeridas.
    /// </summary>
    /// <param name="cuentaDeLaMatriz">Cuenta que la matriz contable asigna a la partida, si la hay.</param>
    /// <param name="antecedentes">Historial de imputaciones con su leyenda.</param>
    /// <param name="leyenda">Leyenda que se está escribiendo.</param>
    /// <param name="frecuentes">Cuentas más usadas, de mayor a menor.</param>
    /// <param name="maximo">Cantidad de sugerencias a devolver.</param>
    public static List<SugerenciaDeCuenta> Sugerir(
        int? cuentaDeLaMatriz,
        IEnumerable<AntecedenteDeImputacion> antecedentes,
        string? leyenda,
        IEnumerable<int> frecuentes,
        int maximo = 5)
    {
        var sugerencias = new List<SugerenciaDeCuenta>();
        var yaPropuestas = new HashSet<int>();

        if (cuentaDeLaMatriz is > 0)
        {
            sugerencias.Add(new SugerenciaDeCuenta(
                cuentaDeLaMatriz.Value, 1m, OrigenDeSugerencia.MatrizContable,
                "La matriz contable relaciona esta partida con esta cuenta."));

            yaPropuestas.Add(cuentaDeLaMatriz.Value);
        }

        var palabras = Palabras(leyenda);

        if (palabras.Count > 0)
        {
            var porCuenta = new Dictionary<int, (decimal Mejor, int Veces, string Leyenda)>();

            foreach (var antecedente in antecedentes)
            {
                var parecido = Parecido(palabras, Palabras(antecedente.Leyenda));

                if (parecido < ParecidoMinimo) continue;

                var actual = porCuenta.GetValueOrDefault(antecedente.IdCtaContable);

                if (parecido > actual.Mejor)
                {
                    porCuenta[antecedente.IdCtaContable] =
                        (parecido, actual.Veces + antecedente.Veces, antecedente.Leyenda ?? string.Empty);
                }
                else
                {
                    porCuenta[antecedente.IdCtaContable] =
                        (actual.Mejor, actual.Veces + antecedente.Veces, actual.Leyenda);
                }
            }

            foreach (var (idCuenta, datos) in porCuenta.OrderByDescending(x => x.Value.Mejor).ThenByDescending(x => x.Value.Veces))
            {
                if (!yaPropuestas.Add(idCuenta)) continue;

                sugerencias.Add(new SugerenciaDeCuenta(
                    idCuenta,
                    decimal.Round(datos.Mejor, 2),
                    OrigenDeSugerencia.LeyendaParecida,
                    $"{datos.Veces} asiento(s) anterior(es) con una leyenda parecida usaron esta cuenta, " +
                    $"por ejemplo \"{Recortar(datos.Leyenda)}\"."));

                if (sugerencias.Count >= maximo) return sugerencias;
            }
        }

        foreach (var idCuenta in frecuentes)
        {
            if (sugerencias.Count >= maximo) break;
            if (!yaPropuestas.Add(idCuenta)) continue;

            sugerencias.Add(new SugerenciaDeCuenta(
                idCuenta, 0.1m, OrigenDeSugerencia.UsoFrecuente,
                "Es una de las cuentas más usadas en el ejercicio."));
        }

        return sugerencias;
    }

    /// <summary>
    /// Parecido entre dos conjuntos de palabras: cuántas comparten sobre cuántas distintas tienen
    /// entre las dos.
    /// </summary>
    public static decimal Parecido(HashSet<string> unas, HashSet<string> otras)
    {
        if (unas.Count == 0 || otras.Count == 0) return 0m;

        var comunes = unas.Count(p => otras.Contains(p));
        var distintas = unas.Count + otras.Count - comunes;

        return distintas == 0 ? 0m : (decimal)comunes / distintas;
    }

    /// <summary>Palabras significativas de una leyenda, sin acentos ni relleno.</summary>
    public static HashSet<string> Palabras(string? leyenda)
    {
        if (string.IsNullOrWhiteSpace(leyenda)) return [];

        var limpia = new string(leyenda
            .Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                        != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());

        return
        [
            .. limpia
                .Split([' ', '.', ',', ';', ':', '-', '/', '(', ')', '[', ']', '\t', '\n'],
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLowerInvariant())
                .Where(p => p.Length >= LargoMinimoDePalabra)
                .Where(p => !Vacias.Contains(p))
                // Los números de expediente y de orden cambian en cada asiento: no dicen nada
                // sobre a qué cuenta va.
                .Where(p => !p.All(char.IsDigit))
        ];
    }

    private static string Recortar(string texto) => texto.Length <= 48 ? texto : texto[..45] + "...";
}
