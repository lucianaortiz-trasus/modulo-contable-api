namespace Contable.Domain.Reglas;

/// <summary>Problemas estructurales que puede tener una cuenta dentro del arbol.</summary>
[Flags]
public enum AnomaliaJerarquia
{
    Ninguna = 0,

    /// <summary>La cuenta se declara padre de si misma.</summary>
    SeApuntaASiMisma = 1,

    /// <summary>El padre declarado no existe en el plan.</summary>
    PadreInexistente = 2,

    /// <summary>La cadena de padres vuelve sobre si misma.</summary>
    FormaUnCiclo = 4,

    /// <summary>El nivel no es el del padre mas uno.</summary>
    NivelInconsistente = 8,

    /// <summary>El codigo no cuelga del codigo del padre.</summary>
    CodigoNoCuelgaDelPadre = 16,

    /// <summary>Esta marcada como imputable pero tiene cuentas hijas.</summary>
    ImputableConHijas = 32,
}

/// <summary>Datos minimos que necesita el armador del arbol.</summary>
public interface INodoPlanDeCuentas
{
    int Id { get; }

    int? IdPadre { get; }

    string? Codigo { get; }

    int? Nivel { get; }

    bool EsImputable { get; }
}

/// <summary>Una cuenta dentro del arbol, con sus hijas y sus anomalias.</summary>
public class NodoCuenta<T> where T : INodoPlanDeCuentas
{
    public required T Cuenta { get; init; }

    public List<NodoCuenta<T>> Hijas { get; } = [];

    public AnomaliaJerarquia Anomalias { get; internal set; }

    /// <summary>Profundidad real dentro del arbol armado, que puede no coincidir con Nivel.</summary>
    public int Profundidad { get; internal set; }

    public bool TieneAnomalias => Anomalias != AnomaliaJerarquia.Ninguna;
}

/// <summary>Resultado de armar el arbol.</summary>
public class ResultadoArbol<T> where T : INodoPlanDeCuentas
{
    public List<NodoCuenta<T>> Raices { get; init; } = [];

    /// <summary>Todos los nodos indexados por id, con anomalias ya calculadas.</summary>
    public Dictionary<int, NodoCuenta<T>> PorId { get; init; } = [];

    public List<NodoCuenta<T>> ConAnomalias => [.. PorId.Values.Where(n => n.TieneAnomalias)];
}

/// <summary>
/// Arma el arbol del plan de cuentas a partir de la relacion padre-hijo.
/// </summary>
/// <remarks>
/// El arbol se arma **siempre** por IdPadre y nunca por prefijo de codigo: el nivel 1 guarda "1"
/// mientras el nivel 2 guarda "01.01", asi que el codigo del padre no es prefijo del hijo.
///
/// El armado es iterativo, no recursivo, y esta hecho para aguantar datos sucios. En el plan de
/// Morteros hay una cuenta que se declara padre de si misma (la 10309, "Honorarios Profesionales"):
/// un recorrido recursivo ingenuo entra en bucle infinito, y uno que arranque solo desde las raices
/// pierde esa cuenta y sus dos hermanas sin decir nada. Aca ninguna cuenta se pierde: la que no
/// puede colgarse de su padre se promueve a raiz y queda marcada.
/// </remarks>
public static class ArbolDeCuentas
{
    public static ResultadoArbol<T> Armar<T>(IEnumerable<T> cuentas) where T : INodoPlanDeCuentas
    {
        var nodos = new Dictionary<int, NodoCuenta<T>>();

        foreach (var cuenta in cuentas)
        {
            // Ante ids repetidos gana el primero: duplicar un nodo romperia el indice.
            nodos.TryAdd(cuenta.Id, new NodoCuenta<T> { Cuenta = cuenta });
        }

        var raices = new List<NodoCuenta<T>>();

        foreach (var nodo in nodos.Values)
        {
            var idPadre = nodo.Cuenta.IdPadre;

            if (idPadre is null)
            {
                raices.Add(nodo);
                continue;
            }

            if (idPadre == nodo.Cuenta.Id)
            {
                nodo.Anomalias |= AnomaliaJerarquia.SeApuntaASiMisma;
                raices.Add(nodo);
                continue;
            }

            if (!nodos.TryGetValue(idPadre.Value, out var padre))
            {
                nodo.Anomalias |= AnomaliaJerarquia.PadreInexistente;
                raices.Add(nodo);
                continue;
            }

            if (FormaCiclo(nodos, nodo, padre))
            {
                nodo.Anomalias |= AnomaliaJerarquia.FormaUnCiclo;
                raices.Add(nodo);
                continue;
            }

            padre.Hijas.Add(nodo);
            MarcarIncoherencias(nodo, padre);
        }

        foreach (var nodo in nodos.Values)
        {
            if (nodo.Cuenta.EsImputable && nodo.Hijas.Count > 0)
            {
                nodo.Anomalias |= AnomaliaJerarquia.ImputableConHijas;
            }
        }

        OrdenarPorCodigo(raices);
        AsignarProfundidad(raices);

        return new ResultadoArbol<T> { Raices = raices, PorId = nodos };
    }

    /// <summary>
    /// Sube por la cadena de padres desde el candidato. Si vuelve a pasar por el nodo que se quiere
    /// colgar, la relacion cerraria un ciclo.
    /// </summary>
    private static bool FormaCiclo<T>(Dictionary<int, NodoCuenta<T>> nodos, NodoCuenta<T> nodo, NodoCuenta<T> padre)
        where T : INodoPlanDeCuentas
    {
        var visitados = new HashSet<int> { nodo.Cuenta.Id };
        var actual = padre;

        while (true)
        {
            if (!visitados.Add(actual.Cuenta.Id)) return true;

            var idAbuelo = actual.Cuenta.IdPadre;

            if (idAbuelo is null || idAbuelo == actual.Cuenta.Id) return false;
            if (!nodos.TryGetValue(idAbuelo.Value, out var abuelo)) return false;

            actual = abuelo;
        }
    }

    private static void MarcarIncoherencias<T>(NodoCuenta<T> nodo, NodoCuenta<T> padre) where T : INodoPlanDeCuentas
    {
        if (nodo.Cuenta.Nivel is not null && padre.Cuenta.Nivel is not null
            && nodo.Cuenta.Nivel != padre.Cuenta.Nivel + 1)
        {
            nodo.Anomalias |= AnomaliaJerarquia.NivelInconsistente;
        }

        if (!CodigoCuenta.CuelgaDe(nodo.Cuenta.Codigo, padre.Cuenta.Codigo))
        {
            nodo.Anomalias |= AnomaliaJerarquia.CodigoNoCuelgaDelPadre;
        }
    }

    private static void OrdenarPorCodigo<T>(List<NodoCuenta<T>> nivel) where T : INodoPlanDeCuentas
    {
        nivel.Sort((a, b) => string.CompareOrdinal(
            CodigoCuenta.Normalizar(a.Cuenta.Codigo),
            CodigoCuenta.Normalizar(b.Cuenta.Codigo)));

        foreach (var nodo in nivel) OrdenarPorCodigo(nodo.Hijas);
    }

    /// <summary>Recorrido iterativo: con datos sucios la recursion no es segura.</summary>
    private static void AsignarProfundidad<T>(List<NodoCuenta<T>> raices) where T : INodoPlanDeCuentas
    {
        var pila = new Stack<(NodoCuenta<T> Nodo, int Profundidad)>();

        foreach (var raiz in raices) pila.Push((raiz, 1));

        while (pila.Count > 0)
        {
            var (nodo, profundidad) = pila.Pop();
            nodo.Profundidad = profundidad;

            foreach (var hija in nodo.Hijas) pila.Push((hija, profundidad + 1));
        }
    }
}
