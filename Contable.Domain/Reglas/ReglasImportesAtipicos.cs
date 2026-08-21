namespace Contable.Domain.Reglas;

/// <summary>Cuán lejos del comportamiento habitual queda un importe.</summary>
public enum GradoDeAtipicidad
{
    Normal,

    /// <summary>Llamativo, pero puede ser legítimo.</summary>
    Inusual,

    /// <summary>Muy lejos de lo habitual de esa cuenta.</summary>
    MuyAtipico,
}

/// <summary>Resultado de evaluar un importe contra el historial de su cuenta.</summary>
/// <param name="Grado">Qué tan atípico es.</param>
/// <param name="Mediana">Importe habitual de la cuenta.</param>
/// <param name="Desviacion">Dispersión robusta del historial.</param>
/// <param name="Veces">Cuántas desviaciones separan al importe de la mediana.</param>
/// <param name="Motivo">Explicación en palabras.</param>
public record Atipicidad(
    GradoDeAtipicidad Grado,
    decimal Mediana,
    decimal Desviacion,
    decimal Veces,
    string? Motivo)
{
    public bool EsAtipico => Grado != GradoDeAtipicidad.Normal;
}

/// <summary>
/// Detección de importes fuera de lo habitual para una cuenta.
/// </summary>
/// <remarks>
/// Se usa la mediana y la desviación absoluta mediana (MAD), no el promedio y el desvío estándar.
/// La razón es práctica: en contabilidad municipal un solo asiento grande —una compra de
/// maquinaria, una transferencia extraordinaria— corre el promedio lo suficiente como para que
/// después nada parezca raro. La mediana no se mueve por unos pocos valores extremos, que es
/// justamente lo que se quiere detectar.
///
/// El criterio es deliberadamente conservador: avisa, no bloquea. Un importe atípico no está mal,
/// solo es infrecuente, y quien carga es el que sabe si corresponde.
/// </remarks>
public static class ReglasImportesAtipicos
{
    /// <summary>Debajo de esta cantidad de antecedentes no se opina.</summary>
    public const int MinimoDeAntecedentes = 8;

    /// <summary>Desviaciones a partir de las cuales el importe se marca como inusual.</summary>
    public const decimal UmbralInusual = 5m;

    /// <summary>Desviaciones a partir de las cuales se marca como muy atípico.</summary>
    public const decimal UmbralMuyAtipico = 10m;

    /// <summary>
    /// Compara un importe contra el historial de su cuenta.
    /// </summary>
    /// <param name="importe">Importe a evaluar.</param>
    /// <param name="historial">Importes anteriores de la misma cuenta.</param>
    public static Atipicidad Evaluar(decimal importe, IEnumerable<decimal> historial)
    {
        var valores = historial.Where(v => v != 0).OrderBy(v => v).ToList();

        if (valores.Count < MinimoDeAntecedentes)
        {
            return new Atipicidad(
                GradoDeAtipicidad.Normal, 0m, 0m, 0m,
                $"La cuenta tiene {valores.Count} antecedente(s): hacen falta {MinimoDeAntecedentes} para comparar.");
        }

        var mediana = Mediana(valores);

        // Desviación absoluta mediana: la mediana de las distancias a la mediana.
        var distancias = valores.Select(v => Math.Abs(v - mediana)).OrderBy(v => v).ToList();
        var mad = Mediana(distancias);

        if (mad == 0)
        {
            // Todos los importes anteriores son iguales. Cualquier valor distinto salta a la vista
            // sin necesidad de estadística.
            var iguales = importe == mediana;

            return new Atipicidad(
                iguales ? GradoDeAtipicidad.Normal : GradoDeAtipicidad.Inusual,
                mediana, 0m, 0m,
                iguales
                    ? null
                    : $"Los {valores.Count} movimientos anteriores de esta cuenta son todos de {mediana:N2}.");
        }

        var veces = Math.Abs(importe - mediana) / mad;

        var grado = veces >= UmbralMuyAtipico
            ? GradoDeAtipicidad.MuyAtipico
            : veces >= UmbralInusual
                ? GradoDeAtipicidad.Inusual
                : GradoDeAtipicidad.Normal;

        var motivo = grado == GradoDeAtipicidad.Normal
            ? null
            : $"El importe habitual de esta cuenta ronda los {mediana:N2}. " +
              $"Este es {(importe > mediana ? "mayor" : "menor")} y se aparta {veces:N1} veces la dispersión " +
              $"de sus {valores.Count} movimientos anteriores.";

        return new Atipicidad(grado, mediana, mad, decimal.Round(veces, 1), motivo);
    }

    private static decimal Mediana(List<decimal> ordenados)
    {
        if (ordenados.Count == 0) return 0m;

        var medio = ordenados.Count / 2;

        return ordenados.Count % 2 == 1
            ? ordenados[medio]
            : (ordenados[medio - 1] + ordenados[medio]) / 2m;
    }
}
