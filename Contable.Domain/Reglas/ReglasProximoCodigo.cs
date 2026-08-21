namespace Contable.Domain.Reglas;

/// <summary>Resultado del calculo del proximo codigo disponible.</summary>
/// <param name="Codigo">Codigo libre, o null si el nivel esta agotado.</param>
/// <param name="Nivel">Nivel que le corresponde a la cuenta nueva.</param>
/// <param name="Segmento">Valor numerico del ultimo segmento.</param>
/// <param name="HijosIgnorados">
/// Hijas cuyo codigo no cuelga del padre y que por lo tanto no participaron del calculo.
/// </param>
public record ProximoCodigo(string? Codigo, int Nivel, int? Segmento, int HijosIgnorados)
{
    public bool HayDisponible => Codigo is not null;
}

/// <summary>
/// Calcula el proximo codigo libre bajo una cuenta padre.
/// </summary>
/// <remarks>
/// Reemplaza a FUNC_PROXIMO_NIVEL1..7 del PL/SQL, que estaban rotas: usaban largos de codigo
/// que no existen (13 y 14), se apoyaban en una excepcion NO_DATA_FOUND como control de flujo y
/// tenian una rama que terminaba sin RETURN, con lo que Oracle levantaba ORA-06503. Es la unica
/// regla del modulo que se reemplaza en lugar de portarse: buscar paridad aca seria replicar un
/// error. Ver _analisis/REGLAS.md, defecto D6.
///
/// El criterio es el primer hueco disponible, no el maximo mas uno. Asi se reutilizan los codigos
/// que quedaron libres al borrar una cuenta, que es lo que el usuario espera al ver el arbol.
/// </remarks>
public static class ReglasProximoCodigo
{
    /// <summary>
    /// Devuelve el primer codigo libre bajo el padre indicado.
    /// </summary>
    /// <param name="codigoPadre">Codigo de la cuenta padre. Null o vacio para calcular una raiz.</param>
    /// <param name="nivelPadre">Nivel de la cuenta padre. 0 para calcular una raiz.</param>
    /// <param name="codigosHijos">Codigos de las cuentas que ya cuelgan del padre.</param>
    public static ProximoCodigo Calcular(string? codigoPadre, int nivelPadre, IEnumerable<string?> codigosHijos)
    {
        var nivelHijo = nivelPadre + 1;

        if (!CodigoCuenta.EsNivelValido(nivelHijo))
        {
            throw new InvalidOperationException(
                $"El plan de cuentas llega hasta el nivel {CodigoCuenta.NivelMaximo}: no se puede abrir un nivel {nivelHijo}.");
        }

        var hijos = codigosHijos?.ToArray() ?? [];

        // Una hija cuyo codigo no cuelga del padre no dice nada sobre que segmentos estan usados
        // bajo ese padre. En el plan de Morteros hay catorce filas asi, producto de relaciones mal
        // cargadas; se dejan afuera del calculo y se informan.
        var relevantes = nivelHijo == 1
            ? hijos
            : hijos.Where(c => CodigoCuenta.CuelgaDe(c, codigoPadre)).ToArray();

        var ignorados = hijos.Length - relevantes.Length;

        var usados = relevantes
            .Select(c => CodigoCuenta.SegmentoDeNivel(c, nivelHijo))
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToHashSet();

        var maximo = CodigoCuenta.ValorMaximo(nivelHijo);

        for (var candidato = 1; candidato <= maximo; candidato++)
        {
            if (usados.Contains(candidato)) continue;

            return new ProximoCodigo(
                CodigoCuenta.ComponerDesdePadre(codigoPadre, nivelHijo, candidato),
                nivelHijo,
                candidato,
                ignorados);
        }

        // Nivel agotado. El PL/SQL viejo, en este caso, salia de la funcion sin devolver nada.
        return new ProximoCodigo(null, nivelHijo, null, ignorados);
    }
}
