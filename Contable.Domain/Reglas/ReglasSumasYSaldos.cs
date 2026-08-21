namespace Contable.Domain.Reglas;

/// <summary>Movimientos propios de una cuenta, ya separados en periodo y acumulado anterior.</summary>
public record MovimientoDeCuenta(
    int IdCtaContable,
    decimal DebePeriodo,
    decimal HaberPeriodo,
    decimal DebeAcumulado,
    decimal HaberAcumulado);

/// <summary>Una fila del balance de sumas y saldos.</summary>
public record FilaSumasYSaldos
{
    public required int IdCtaContable { get; init; }

    public required string Codigo { get; init; }

    public required string Nombre { get; init; }

    public int? Nivel { get; init; }

    public bool Imputable { get; init; }

    /// <summary>
    /// La cuenta no tiene hijas: su importe es propio y no viene de consolidar nada.
    /// </summary>
    /// <remarks>
    /// No alcanza con mirar <c>Imputable</c> para sumar los totales del balance. El plan real
    /// tiene tres cuentas marcadas como imputables que ademas tienen hijas (ver D28), asi que su
    /// fila consolidada incluye lo de sus hijas y sumarlas a las dos contaria los mismos importes
    /// dos veces.
    /// </remarks>
    public bool EsHoja { get; init; }

    /// <summary>Profundidad dentro del arbol, para sangrar la fila en pantalla.</summary>
    public int Profundidad { get; init; }

    public decimal DebeAcumulado { get; init; }

    public decimal HaberAcumulado { get; init; }

    public decimal DebePeriodo { get; init; }

    public decimal HaberPeriodo { get; init; }

    public decimal DebeTotal => DebeAcumulado + DebePeriodo;

    public decimal HaberTotal => HaberAcumulado + HaberPeriodo;

    /// <summary>Saldo deudor: lo que sobra del debe. Cero si el saldo es acreedor.</summary>
    public decimal SaldoDeudor => DebeTotal > HaberTotal ? DebeTotal - HaberTotal : 0m;

    /// <summary>Saldo acreedor: lo que sobra del haber. Cero si el saldo es deudor.</summary>
    public decimal SaldoAcreedor => HaberTotal > DebeTotal ? HaberTotal - DebeTotal : 0m;

    public bool TieneMovimiento => DebeTotal != 0 || HaberTotal != 0;
}

/// <summary>
/// Balance de sumas y saldos.
/// </summary>
/// <remarks>
/// Portado de `PKG_REPORTES_CONTABILIDAD.PRC_SUMAS_Y_SALDOS`. La estructura del reporte se
/// conserva —una fila por cuenta, con las sumas del periodo, las del ejercicio anterior y el saldo
/// resultante— pero se corrigen tres cosas que en el original daban numeros equivocados. Estan en
/// _analisis/REGLAS.md como D13, D14 y el que aparecio al portar:
///
/// <b>La consolidacion es por la relacion padre-hijo, no por prefijo de codigo.</b> El original
/// acumulaba con <c>cta.codigo LIKE cc.codigo || '%'</c>. Como el nivel 1 guarda "1" y el nivel 2
/// guarda "01.01", el LIKE no engancha nada y <b>las cinco cuentas de nivel 1 salen en cero</b>:
/// ACTIVO, PASIVO, PATRIMONIO NETO, INGRESOS y EGRESOS, o sea justo los totales que uno mira
/// primero en un balance. Verificado contra el stage.
/// </remarks>
public static class ReglasSumasYSaldos
{
    /// <summary>
    /// Arma el balance consolidando los movimientos hacia arriba del arbol.
    /// </summary>
    /// <param name="cuentas">Plan de cuentas del alcance del reporte.</param>
    /// <param name="movimientos">Movimientos propios de cada cuenta.</param>
    /// <param name="nivelMaximo">Ultimo nivel a mostrar. Null trae todos.</param>
    /// <param name="soloConMovimiento">Descarta las cuentas que quedaron en cero.</param>
    public static List<FilaSumasYSaldos> Armar<T>(
        IEnumerable<T> cuentas,
        IEnumerable<MovimientoDeCuenta> movimientos,
        int? nivelMaximo = null,
        bool soloConMovimiento = false)
        where T : INodoPlanDeCuentas, ICuentaConNombre
    {
        var arbol = ArbolDeCuentas.Armar(cuentas);
        var porCuenta = movimientos.ToDictionary(m => m.IdCtaContable);

        // Acumulados de cada nodo, incluyendo lo de sus descendientes.
        var totales = new Dictionary<int, decimal[]>();

        foreach (var id in arbol.PorId.Keys)
        {
            var propio = porCuenta.GetValueOrDefault(id);

            totales[id] = propio is null
                ? [0m, 0m, 0m, 0m]
                : [propio.DebePeriodo, propio.HaberPeriodo, propio.DebeAcumulado, propio.HaberAcumulado];
        }

        // Recorrido de abajo hacia arriba: cada nodo suma lo de sus hijas. Se hace con una pila y
        // no con recursion porque el plan real tiene una cuenta que se declara padre de si misma.
        foreach (var raiz in arbol.Raices)
        {
            var orden = new List<NodoCuenta<T>>();
            var pila = new Stack<NodoCuenta<T>>();
            pila.Push(raiz);

            while (pila.Count > 0)
            {
                var nodo = pila.Pop();
                orden.Add(nodo);

                foreach (var hija in nodo.Hijas) pila.Push(hija);
            }

            // Al recorrer al reves quedan primero las hojas.
            for (var i = orden.Count - 1; i >= 0; i--)
            {
                var nodo = orden[i];
                var idPadre = nodo.Cuenta.IdPadre;

                if (idPadre is null || idPadre == nodo.Cuenta.Id) continue;
                if (!totales.ContainsKey(idPadre.Value)) continue;

                for (var j = 0; j < 4; j++)
                {
                    totales[idPadre.Value][j] += totales[nodo.Cuenta.Id][j];
                }
            }
        }

        var filas = new List<FilaSumasYSaldos>();

        foreach (var nodo in arbol.PorId.Values)
        {
            var nivel = nodo.Cuenta.Nivel ?? nodo.Profundidad;

            if (nivelMaximo is not null && nivel > nivelMaximo) continue;

            var t = totales[nodo.Cuenta.Id];

            var fila = new FilaSumasYSaldos
            {
                IdCtaContable = nodo.Cuenta.Id,
                Codigo = nodo.Cuenta.Codigo ?? string.Empty,
                Nombre = nodo.Cuenta.Nombre ?? string.Empty,
                Nivel = nodo.Cuenta.Nivel,
                Imputable = nodo.Cuenta.EsImputable,
                EsHoja = nodo.Hijas.Count == 0,
                Profundidad = nodo.Profundidad,
                DebePeriodo = ReglasBalanceAsiento.Redondear(t[0]),
                HaberPeriodo = ReglasBalanceAsiento.Redondear(t[1]),
                DebeAcumulado = ReglasBalanceAsiento.Redondear(t[2]),
                HaberAcumulado = ReglasBalanceAsiento.Redondear(t[3]),
            };

            if (soloConMovimiento && !fila.TieneMovimiento) continue;

            filas.Add(fila);
        }

        return [.. filas.OrderBy(f => CodigoCuenta.Normalizar(f.Codigo), StringComparer.Ordinal)];
    }

    /// <summary>
    /// Totales del balance. En un balance bien armado, la suma de los saldos deudores tiene que
    /// igualar la de los acreedores.
    /// </summary>
    public static (decimal Deudor, decimal Acreedor, bool Cuadra) Totales(IEnumerable<FilaSumasYSaldos> filas)
    {
        // Solo las hojas. Sumar las cuentas de agrupacion contaria cada importe una vez por
        // nivel del arbol, y usar Imputable en lugar de EsHoja tampoco alcanza: hay tres cuentas
        // marcadas como imputables que tienen hijas, y su fila ya trae los importes de ellas.
        var hojas = filas.Where(f => f.EsHoja).ToList();

        var deudor = ReglasBalanceAsiento.Redondear(hojas.Sum(f => f.SaldoDeudor));
        var acreedor = ReglasBalanceAsiento.Redondear(hojas.Sum(f => f.SaldoAcreedor));

        return (deudor, acreedor, deudor == acreedor);
    }
}

/// <summary>Nombre de la cuenta, que el armador del balance necesita ademas de la estructura.</summary>
public interface ICuentaConNombre
{
    string? Nombre { get; }
}
