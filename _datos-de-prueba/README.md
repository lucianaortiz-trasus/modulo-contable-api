# Datos de prueba

Tres scripts para poblar el stage con asientos sintéticos, verificarlos y borrarlos. Van juntos:
nunca se genera sin tener a mano el que limpia.

| Script | Qué hace | Escribe |
|---|---|---|
| `generar.sql` | Genera los asientos | Sí |
| `verificar.sql` | Muestra qué hay y controla la consistencia | No |
| `limpiar.sql` | Borra todo lo generado | Sí |

```bash
sqlplus <usuario>/<clave>@<cadena-de-conexion> @generar.sql
```

Se conecta con el usuario del esquema `EJECUCION`. Las credenciales no van escritas acá ni en
ningún archivo del repositorio: se piden al área de sistemas.

## Reglas duras

- Se escribe **únicamente** en `EJECUCION.T_ASIENTOS` y `EJECUCION.T_ASIENTOS_DET`.
- No se toca el plan de cuentas, la matriz contable, las vigencias, las jurisdicciones, las
  unidades administrativas ni ningún otro esquema.
- Todo asiento generado lleva el prefijo **`[QA]`** al principio de la leyenda. Eso es lo único
  que hace falta para reconocerlo, y es por lo que `limpiar.sql` los borra.
- `generar.sql` se niega a correr si ya hay asientos con ese prefijo: primero hay que limpiar.
- Ante cualquier error hace `ROLLBACK` y deja la base como estaba.

## Cómo ver qué haría sin escribir nada

En `generar.sql`, poner `v_simular := TRUE`. Calcula todo, informa el resumen y deshace.

## Qué genera

Con los parámetros que vienen por defecto:

| Ejercicio | Asientos | Vigencia | Jurisdicción |
|---|---|---|---|
| 2025 | 150 | 200 | 4000 |
| 2026 | 250 | 201 | 2437 |

400 asientos y 818 renglones, repartidos entre todos los meses de 2025 y los transcurridos de
2026. La numeración continúa el máximo real de cada jurisdicción, no arranca de cero.

Tres patrones contables, que son los mismos que tienen los asientos reales del stage:

1. **Devengamiento de gasto** — una cuenta de egresos al debe contra una de pasivo al haber
2. **Pago a proveedor** — pasivo al debe contra una cuenta bancaria al haber
3. **Recaudación** — cuenta bancaria al debe contra una de ingresos al haber

Uno de cada siete reparte el gasto en dos cuentas, para que el juego tenga asientos de más de dos
renglones.

## Dos decisiones que conviene conocer

**Las cuentas no están escritas a mano.** El script las toma del plan real filtrando por
naturaleza e imputable. Sirve para cualquier municipio, no solo para Morteros: si el plan cambia,
los asientos se acomodan solos.

**El generador es determinista.** No usa `DBMS_RANDOM`, sino aritmética sobre el índice del bucle.
Dos corridas con los mismos parámetros producen exactamente los mismos asientos, que es lo que uno
quiere de un juego de datos de prueba: si algo falla, se puede reproducir.

## Un detalle sobre la jurisdicción

Los asientos de 2025 quedan en jurisdicción **4000**, porque esa es la única unidad administrativa
de la vigencia 200. No es una elección del script: es la regla real del sistema, la misma que
aplica el alta del módulo.

Consecuencia práctica: un usuario de la jurisdicción 2437 **no ve los asientos de 2025 en la
grilla**. Sirven igual para los reportes, que toman el ejercicio como parámetro, y para probar el
encadenamiento de cierre y apertura.

## Controles que corre `verificar.sql`

Los tres tienen que dar cero:

- Asientos descuadrados
- Números repetidos dentro de una misma jurisdicción
- Renglones imputados a cuentas no imputables
