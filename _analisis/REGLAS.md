# Fase 0 — Reglas contables extraídas del PL/SQL

Trazabilidad `procedimiento PL/SQL → clase de Reglas → test` para el port a .NET 8.

- Fuente: `_analisis/plsql/` (7 packages, 11.449 líneas) extraídos de `EJECUCION@MUNIMST`
- Verificado contra el stage de Morteros por consulta directa
- Fecha: 2026-08-20

---

## 0. Resultado principal: cierre y apertura sí tienen fuente

En el plan figuraban como "sin fuente en Morteros". **Era un falso negativo mío.** `all_arguments`
no los mostraba porque en el *spec* del package están declarados dentro de un bloque `/* */`:

```
    /*
      PROCEDURE INSERTAR_ASIENTO_CIERRE(p_ID_VIGENCIA ...);
      PROCEDURE INSERTAR_ASIENTO_APERTURA(p_ID_VIGENCIA ...);
    */
```

El *body* conserva las dos implementaciones completas: cierre en las líneas 1432–1878 y apertura
en 1879–2141 de `PKG_T_ASIENTOS.sql`. Están comentadas en la interfaz pública, no borradas.

**Y ahora sabemos por qué las desactivaron.** El código está atado a la instalación provincial y
en Morteros no puede funcionar:

| Dependencia hardcodeada | Estado en Morteros |
|---|---|
| `ID_CTA_CONTABLE = 11270` (cuenta de balanceo del cierre) | **No existe.** El rango real es 10001–10350 |
| `NOMBRE_UNIDAD_ADM = 'Administracion Central'` | **No existe.** Las UA son Malagueño, Morteros y Trasus |
| `ID_RECURSO_D_H` 3 y 4 | Sin catálogo cargado para verificar |
| `USER_ALTA = 'SUAF'` | Literal del sistema provincial |

El `SELECT ... INTO v_UNIDAD_CENTRAL` lanzaría `NO_DATA_FOUND` y abortaría antes de insertar nada.
No fue un capricho: está roto para este municipio.

**Conclusión: tenemos el algoritmo, y hay que reimplementarlo con los parámetros externalizados,
no portarlo literal.** El riesgo #1 del plan baja de "reconstruir a ciegas" a "portar con
configuración por municipio", que es un trabajo acotado.

---

## 1. Tabla de trazabilidad

| # | Clase de Reglas | Origen PL/SQL | Decisión de port | Test |
|---|---|---|---|---|
| R1 | `ReglasBalanceAsiento` | `t_asientosABM.aspx.cs:645-668` (C#, no PL/SQL) | Portar literal | `ReglasBalanceAsientoTests` |
| R2 | `ReglasNumeracionAsiento` | `F_NUMERO_ASIENTO` | **Corregir** (ver D4) | `ReglasNumeracionAsientoTests` |
| R3 | `ReglasVigenciaDeAsiento` | `PKG_T_ASIENTOS.INSERTAR` | Portar literal | `ReglasVigenciaDeAsientoTests` |
| R4 | `ReglasJurisdiccionPorUnidadAdm` | `PKG_T_ASIENTOS.INSERTAR`, `INSERTAR_REVERSA` | Portar literal | `ReglasJurisdiccionPorUnidadAdmTests` |
| R5 | `ReglasReversa` | `PKG_T_ASIENTOS.INSERTAR_REVERSA` (versión activa, línea 2312) | Portar literal salvo D20 | `ReglasReversaTests` |
| R6 | `ReglasAnulacion` | `PKG_T_ASIENTOS.ELIMINAR` | Portar literal | `ReglasAnulacionTests` |
| R7 | `ReglasAsientoCierre` | `INSERTAR_ASIENTO_CIERRE` (1432) | **Parametrizar** (ver §2) | `ReglasAsientoCierreTests` |
| R8 | `ReglasAsientoApertura` | `INSERTAR_ASIENTO_APERTURA` (1879) | **Parametrizar + corregir D3** | `ReglasAsientoAperturaTests` |
| R9 | `ReglasLibroBanco` | `PKG_T_ASIENTOS_DET.INSERTAR_DET` | Portar literal | `ReglasLibroBancoTests` |
| R10 | `ReglasDetalleAsiento` | `PKG_T_ASIENTOS_DET.INSERTAR_DET` | Portar literal | `ReglasDetalleAsientoTests` |
| R11 | `ReglasRecursoDebeHaber` | `TRAER_RECURSO_DEBE` / `_HABER` | Portar literal (ya degenerado) | `ReglasRecursoDebeHaberTests` |
| R12 | `ReglasCodigoCuenta` | `TRAER_COD_NIVEL_COM`, `INSERTAR` | Portar literal | `ReglasCodigoCuentaTests` |
| R13 | `ReglasProximoCodigo` | `FUNC_PROXIMO_NIVEL1..7` | **Reemplazar** (ver D6) | `ReglasProximoCodigoTests` |
| R14 | `ReglasJerarquiaCuentas` | Estructura de `T_CUENTAS_CONTABLES` | **Nueva** (ver D22) | `ReglasJerarquiaCuentasTests` |
| R15 | `ReglasMatrizContable` | `PKG_T_MATRIZ_CONTABLE.INSERTAR`, `EXISTE_PARTIDA` | **Corregir** (ver D9) | `ReglasMatrizContableTests` |
| R16 | `ReglasImportacionMatriz` | `IMPORTA_RELACION` | **Corregir** (ver D10, D11, D12) | `ReglasImportacionMatrizTests` |
| R17 | `ReglasSaldoCuenta` | Vista `VT_SALDO_CTACONTABLE` | Portar literal + opción anulados | `ReglasSaldoCuentaTests` |
| R18 | `ReglasLibroDiario` | `PRC_LIBRO_DIARIO` | Portar con D13/D14 resueltos | `ReglasLibroDiarioTests` |
| R19 | `ReglasMayores` | `PRC_MAYORES` | Portar con D14 resuelto | `ReglasMayoresTests` |
| R20 | `ReglasSumasYSaldos` | `PRC_SUMAS_Y_SALDOS` | Portar con D13/D14/D15 resueltos | `ReglasSumasYSaldosTests` |
| R22 | `ReglasImportesAtipicos` | Sin origen: agregado del módulo nuevo | **Nueva** (ver §4) | `ReglasImportesAtipicosTests` |
| R23 | `ReglasSugerenciaDeImputacion` | Sin origen: agregado del módulo nuevo | **Nueva** (ver §4) | `ReglasSugerenciaDeImputacionTests` |
| R24 | `ReglasAuditoria` / `ReglasDuplicados` | Sin origen: agregado del módulo nuevo | **Nueva** (ver §4) | `ReglasAuditoriaTests`, `ReglasDuplicadosTests` |

---

## 2. Reglas críticas en detalle

### R7 — Asiento de cierre

Genera **dos asientos separados**, ambos con `ID_TIPO_ASIENTO = 6`, fecha contable `31/12/ejercicio`,
leyenda y observaciones `'ASIENTO DE CIERRE'`, unidad administrativa central:

1. **Cierre de resultado** — cuentas con `ID_NATURALEZA IN (4, 5)`
2. **Cierre patrimonial** — cuentas con `ID_NATURALEZA IN (1, 2)`

Para cada uno, el algoritmo es idéntico:

```
saldos ← VT_SALDO_CTACONTABLE filtrada por naturaleza y vigencia
para cada cuenta con saldo:
    si tipo_saldo = 'D':  detalle con HABER = saldo, recurso ← cuenta.ID_RECURSO_HABER
    si tipo_saldo = 'H':  detalle con DEBE  = saldo, recurso ← cuenta.ID_RECURSO_DEBE
diferencia ← |Σ saldos deudores| − |Σ saldos acreedores|
si Σ acreedor > Σ deudor:  detalle en cuenta de balanceo con HABER = |diferencia|, recurso 3
si no:                     detalle en cuenta de balanceo con DEBE  = |diferencia|, recurso 4
```

Cada cuenta se cancela contra su lado opuesto y el descuadre residual va a una única cuenta de
balanceo. En Morteros las naturalezas están cargadas de forma coherente con el algoritmo:
1 y 2 son patrimoniales (163 cuentas), 4 y 5 son de resultado (165 cuentas).

**Naturaleza 3 (PATRIMONIO NETO, 22 cuentas) queda fuera del bucle de cuentas a cancelar, y es
coherente**: el patrimonio neto es justamente donde cae el balanceo. En el plan de Morteros la
cuenta `10176` — `03.01.04.01` "Resultado del ejercicio" (nivel 4, imputable, naturaleza 3,
tipo de saldo acreedor) es la que cumple el rol de la `11270` provincial. Queda como candidata
recomendada, a confirmar por el contador.

**Parámetros a externalizar** (hoy hardcodeados a la instalación provincial):

| Parámetro | Valor provincial | Qué hay que definir para Morteros |
|---|---|---|
| Cuenta de balanceo | `11270` | Qué cuenta cumple ese rol en el plan de Morteros |
| Unidad administrativa | `'Administracion Central'` por nombre | Resolver por id, no por literal |
| Recurso D/H de balanceo | `3` / `4` | Verificar contra el catálogo |
| Usuario de auditoría | `'SUAF'` | Usar el usuario autenticado |
| Naturalezas de resultado | `(4,5)` | Confirmar |
| Naturalezas patrimoniales | `(1,2)` | Confirmar |

### R8 — Asiento de apertura

Lee el asiento de cierre patrimonial del ejercicio anterior y lo reinyecta invertido, con fecha
contable `01/01/ejercicio`:

```
vigencia_anterior ← vigencia cuyo ejercicio = ejercicio − 1
detalles ← T_ASIENTOS_DET de asientos con ID_TIPO_ASIENTO = 6
           de la vigencia anterior, cuentas con naturaleza IN (1,2)
para cada detalle:
    si tenía DEBE:  nuevo detalle con HABER = DEBE, recurso ← cuenta.ID_RECURSO_HABER
    si no:          nuevo detalle con DEBE  = HABER, recurso ← cuenta.ID_RECURSO_DEBE
```

Contiene el defecto D3, abajo.

### R17 — Saldo de cuenta

Fórmula exacta de `VT_SALDO_CTACONTABLE`, que es la base de cierre, mayores y estadísticas:

```sql
saldo      = ABS(SUM(NVL(debe,0)) − SUM(NVL(haber,0)))
tipo_saldo = 'D' si SIGN(SUM(debe) − SUM(haber)) =  1
             'H' si SIGN(SUM(debe) − SUM(haber)) = -1
group by id_vigencia, id_cta_contable
having  SUM(debe) − SUM(haber) <> 0
```

Tres cosas importantes: **no filtra `ANULADO`**, **no filtra por fecha de corte** y el `tipo_saldo`
que devuelve es el saldo *calculado*, distinto del campo `TIPO_SALDO` de la cuenta —que expresa la
naturaleza esperada. El cierre usa el calculado.

### R2 — Numeración

```sql
numero = NVL(MAX(numero), 0) + 1  WHERE id_jurisdiccion = :jurisdiccion
```

Correlativo **perpetuo por jurisdicción**: no se reinicia por ejercicio ni por vigencia. Lo aplican
`INSERTAR`, `INSERTAR_REVERSA` y `ELIMINAR`.

`BUSCARSIGNUM` —que devuelve `null` con el comentario *"se comenta porque no se coloca al generar
nro asiento"*— está obsoleto y comentado. **No es la regla vigente**; la vigente es `F_NUMERO_ASIENTO`.

### R3 y R4 — Derivaciones silenciosas en el alta

`PKG_T_ASIENTOS.INSERTAR` **ignora dos parámetros que recibe** y los recalcula:

- `ID_VIGENCIA` y `ANIO` se derivan del año de `FEC_CONTABLE`, buscando la vigencia cuyo
  `EJERCICIO` coincida. Lo que mande el llamador se descarta.
- `ID_JURISDICCION` se resuelve desde la unidad administrativa **traduciéndola entre vigencias por
  `NUMERO_UNIDAD_ADM`**: busca la UA de la vigencia derivada que tenga el mismo número que la UA
  recibida.

Esto resuelve el riesgo #3 del plan: la traducción `ID_UNIDAD_ADM → ID_JURISDICCION` es por número
de unidad administrativa contra la vigencia que corresponde a la fecha contable.

### R5 — Reversa

La versión activa (línea 2312) difiere de la comentada (1307) en puntos que importan:

| Aspecto | Versión activa |
|---|---|
| Guard de administración general | **Eliminado.** La versión vieja no hacía nada si la UA no era de grupo sector `1010101` |
| Fecha del contraasiento | Si el **año** contable original < año actual → `31/12/ejercicio`; si no → `SYSDATE` |
| Numeración | `F_NUMERO_ASIENTO(jurisdicción)` |
| Leyenda | `'REVERSA ASIENTO ' || numero_original` |
| Detalle | Invierte debe ↔ haber |
| `ANULADO` | `'S'` en el contraasiento **y** en el asiento original |
| Vínculo | `ID_ASIENTO_REL` → asiento original |

### R6 — Anulación

`ELIMINAR` **no borra nada**. Genera un contraasiento con leyenda y observaciones prefijadas
`'CONTRAASIENTO'`, invierte debe/haber, **conserva la fecha contable original** (a diferencia de la
reversa, que la recalcula) y marca `ANULADO = 'S'` en ambos.

Diferencia funcional real entre reversa y anulación: la fecha. Vale confirmarlo con el usuario
funcional.

### R9 — Libro banco

En `INSERTAR_DET`, si el detalle trae `ID_CUENTA_BANCARIA`, además del detalle se inserta un
movimiento en `EJECUCION.T_LIBRO_BANCO`:

```
id_tipo_ep = 15, id_tipo_doc = 15, numero_ep = '0', anulado = 'N'
concepto   = 'GENERADO A PARTIR DE ASIENTO'
si DEBE es 0 o null →  cuenta_destino = cuenta, monto = HABER, tipo_movimiento = 'H'
si no               →  cuenta_origen  = cuenta, monto = DEBE,  tipo_movimiento = 'D'
```

y el detalle queda vinculado por `ID_LIBRO_BANCO`.

### R12 — Código de cuenta

Formato posicional `NN.NN.NN.NN.NN.NN.NNNN`, dos dígitos por nivel salvo el séptimo que lleva
cuatro. Los siete cuadros de texto del ABM mapean a posiciones fijas:

| Nivel | Posición | Largo total del código |
|---|---|---|
| 1 | `SUBSTR(codigo, 1, 2)` | 2 |
| 2 | `SUBSTR(codigo, 4, 2)` | 5 |
| 3 | `SUBSTR(codigo, 7, 2)` | 8 |
| 4 | `SUBSTR(codigo, 10, 2)` | 11 |
| 5 | `SUBSTR(codigo, 13, 2)` | 14 |
| 6 | `SUBSTR(codigo, 16, 2)` | 17 |
| 7 | `SUBSTR(codigo, 19, 4)` | 22 |

Al insertar, el código se trunca con `SUBSTR(codigo, 0, nivel*2 + nivel - 1)`.

Datos reales de Morteros: 350 cuentas en 5 niveles, con largos 1, 5, 8, 11 y 14. No hay cuentas de
nivel 6 ni 7.

---

## 3. Catálogo de defectos

Ordenados por impacto. Cada uno necesita una decisión explícita de port: replicar por paridad, o
corregir y documentar la desviación.

### Bloqueantes para cierre y apertura

**D1 — Cuenta de balanceo inexistente.** `ID_CTA_CONTABLE = 11270` hardcodeada. En Morteros no
existe (rango 10001–10350). Sin resolver esto, el cierre no puede correr.

**D2 — Unidad administrativa por literal.** `NOMBRE_UNIDAD_ADM = 'Administracion Central'`. En
Morteros no existe → `NO_DATA_FOUND` y aborto.

**D3 — La apertura se escribe como asiento de cierre.** `INSERTAR_ASIENTO_APERTURA` inserta la
cabecera con `ID_TIPO_ASIENTO = 6` (DE CIERRE) en vez de 7 (DE APERTURA), y su propia consulta del
año anterior filtra por `ID_TIPO_ASIENTO = 6`. Consecuencia: una apertura ya generada es
indistinguible de un cierre, y **la apertura del año siguiente la leería como si fuera un cierre,
duplicando los saldos**. Hay que corregirlo y además impedir la doble ejecución.

### Correctitud de reportes

**D13 — Sumas y Saldos multiplica importes.** El join es `cc.Id_Jurisdiccion = ua.Id_Jurisdiccion`
sin filtrar vigencia de la unidad administrativa. En Morteros la jurisdicción 4000 tiene **2
unidades administrativas** (vigencias 199 y 200) y **64 cuentas contables** cuelgan de ella: para
esas 64 cuentas los importes se duplican. Las otras 286 (jurisdicción 2437, una sola UA) salen
bien. El reporte está mal de forma parcial e inconsistente.

**D14 — Los anulados cuentan.** Ni `VT_SALDO_CTACONTABLE`, ni Sumas y Saldos, ni Mayores filtran
`ANULADO`. En Sumas y Saldos el filtro está explícitamente comentado:
`--and (adet.anulado = 'N' or adet.anulado is null)`. Hoy no cambia nada porque los 6 detalles del
stage tienen `ANULADO = 'N'`, pero **en cuanto se use una reversa el efecto aparece**: el original
y su contraasiento quedan ambos en `'S'` y ambos suman. Se cancelan entre sí, así que el saldo neto
sigue bien, pero el libro diario y los totales de movimiento muestran el doble.

**D15 — Filtro por unidad administrativa inoperante.** Sumas y Saldos recibe `p_unidad_adm`, lo usa
solo para el rótulo del encabezado y tiene el filtro real comentado. La pantalla ofrece un filtro
que no filtra.

**D16 — Exclusiones hardcodeadas.** `cc.CODIGO <> '01.01.01.02.36'` y `ID_CTA_CONTABLE <> 11323`.
Ninguno de los dos existe en Morteros, así que hoy son inocuos, pero son reglas de negocio
provinciales embebidas en SQL.

### Plan de cuentas

**D6 — `FUNC_PROXIMO_NIVEL1..7` está rota.** Siete funciones casi idénticas con estos problemas:
- Usa `DECODE(LENGTH(CODIGO), 13, ..., 14, ...)`, longitudes que **no corresponden** a los códigos
  reales (5, 8, 11, 14)
- Usa una excepción `NO_DATA_FOUND` como control de flujo para salir de un `FOR i IN 1..99`
- Si el máximo es 99 y el bucle termina sin excepción, **la función no ejecuta ningún `RETURN`** →
  `ORA-06503: Function returned without value`
- `SUBSTR(MAX(codigo), 5, 16)` sobre códigos de 5 caracteres devuelve null y trunca el resultado

No es lógica portable. Recomiendo **reemplazarla** por un cálculo limpio: tomar el código del
padre, enumerar el segmento de nivel de los hijos existentes, elegir el primer hueco o el máximo
más uno, y completar con ceros a la izquierda. Es la única regla donde propongo no buscar paridad,
porque la paridad sería replicar un error.

**D22 — La jerarquía no se puede derivar del código.** El nivel 1 usa códigos de un carácter
(`"1"` = ACTIVO) mientras el nivel 2 usa `"01.01"`. El código de nivel 1 **no es prefijo** de los
de nivel 2. El árbol hay que armarlo por `ID_CTA_CONTABLE_REL`, nunca por prefijo de código.

**D7 — Alta de cuenta con código duplicado devuelve un id falso.** `INSERTAR` consume
`SQ_T_CUENTAS_CONTABLES.NEXTVAL` y lo asigna al parámetro `OUT` *antes* de verificar si el código
existe. Si existe, setea `p_MSG` y no inserta, pero **el `OUT` ya lleva un id**. Un llamador que no
mire `p_MSG` cree que dio de alta.

**D8 — Truncamiento erróneo en nivel 7.** `SUBSTR(codigo, 0, nivel*2 + nivel - 1)` da 20 para
nivel 7, pero el formato necesita 22 (el séptimo segmento tiene 4 dígitos). Un código de nivel 7
perdería los últimos 2 dígitos. Latente: no hay cuentas de nivel 6 ni 7 en Morteros.

### Matriz contable

**D9 — Control de duplicados asimétrico.** En `INSERTAR`:
- Gasto: `WHERE id_objeto_gasto = X AND id_cta_contable = Y` — verifica el par, **sin vigencia**
- Recurso: `WHERE id_recurso = X` — **sin cuenta contable y sin vigencia**

El control de recurso es global a toda la tabla y a todos los ejercicios. Un recurso mapeado una
vez no puede volver a mapearse nunca, ni a otra cuenta ni en otro ejercicio.

**D10 — La importación solo trae gastos.** `IMPORTA_RELACION` filtra `and r.id_recurso is null`.
Las filas de recurso nunca se importan al ejercicio nuevo. Combinado con D9, **el lado recurso de
la matriz queda congelado de forma permanente después de la primera carga**: no se importa
automáticamente y tampoco se puede cargar a mano. Esto explica por qué en Morteros hay 1 sola fila
de matriz contable.

**D11 — Guard muerto.** `select count(*) + 1 into v_EXISTEN_RECURSOS` — el `+ 1` hace que la
condición `if v_EXISTEN_RECURSOS > 0` sea siempre verdadera. La validación "no existen recursos
para la vigencia" nunca se dispara.

**D12 — Traducción silenciosa a NULL.** La importación traduce objetos de gasto y recursos entre
vigencias por `numero_objeto` / `numero_recurso`. Si la partida no existe en la vigencia nueva, la
subconsulta devuelve null y **se inserta la fila con la partida en NULL**, sin aviso.

**Mensaje engañoso.** `IMPORTA_RELACION` informa *"Se insertaron N registros"* donde N es el
`count(*)` total de la vigencia nueva, no la cantidad realmente insertada.

### Transversales

**D4 — Race condition en la numeración.** `MAX(numero) + 1` sin bloqueo ni secuencia. Dos altas
concurrentes en la misma jurisdicción obtienen el mismo número. En el port hay que resolverlo con
secuencia por jurisdicción o con bloqueo explícito.

**D25 — Los errores se comunican por mensaje, no por excepción.** Todos los procedimientos capturan
`WHEN OTHERS`, hacen `PKG_UTIL.BASE_ROLLBACK` y devuelven el texto en `p_MSG`. El llamador que no
inspeccione `p_MSG` no se entera de que falló. En .NET esto pasa a excepciones tipadas.

**D18 — El detalle ignora su propia fecha.** `INSERTAR_DET` recibe `p_FEC_CONTABLE` pero inserta la
fecha contable **de la cabecera**. El parámetro es decorativo.

**D19 — El alta ignora vigencia y año.** Ver R3.

**D20 — Auditoría con el usuario de Oracle.** `INSERTAR_REVERSA` y `ELIMINAR` graban
`USER_ALTA = user` (el usuario de conexión Oracle, siempre `EJECUCION`) en vez del usuario de
aplicación que reciben por parámetro. La auditoría de reversas y anulaciones es inservible.

**D17 — Secuencia consumida al pedo.** `INSERTAR_DET` toma `SEQ_T_LIBRO_BANCO.NEXTVAL` en todas las
altas, aunque solo lo use cuando hay cuenta bancaria. Cosmético.

**D23 — `T_NATURALEZA_CC` está vacía.** Los ids de naturaleza 1 a 5 se usan sin catálogo. Habrá que
cargarlo o definir un enum en el dominio.

**D24 — Código `AP` duplicado.** `T_TIPO_ASIENTO` tiene `AP` en la fila 3 (AUTOMATICO PURO) y en la
7 (DE APERTURA). Buscar por código es ambiguo; siempre por id.

### Encontrados durante la fase 2

**D26 — `ACTIVA` no usa la misma convención que el resto de las marcas.** En
`T_CUENTAS_CONTABLES`, `ACTIVA` vale `'A'` (activa) o `'I'` (inactiva), mientras `IMPUTABLE`,
`AUXILIAR`, `ASIENTO_OPS` e `INVENTARIABLE` usan `'S'` / `'N'`. Está a la vista en
`t_cuentas_contablesABM.aspx.cs:273` y `:392`. Las 350 cuentas del plan tienen `ACTIVA = 'A'`:
unificar la convención sin migrar los datos deja el plan entero como inactivo. Para peor, `'A'`
también es el valor de `TIPO_SALDO` para acreedor, en la misma tabla y con otro significado.
Queda fijado por `MarcasDeCuentaTests`.

**D27 — El plan de cuentas mezcla jurisdicciones dentro del mismo árbol.** Las 350 cuentas se
reparten en jurisdicción 2437 / vigencia 201 (286 cuentas, el plan actual de Morteros) y
jurisdicción 4000 / vigencia 199 (64 cuentas, de otro municipio). No son dos árboles separados:
`PASIVO` (id 10091) y `PREVISIONES CORRIENTES` (id 10145) son de la 4000 y tienen hijas de la 2437.

Consecuencia práctica: filtrar el plan por la jurisdicción del usuario **parte el árbol** y las
hijas que pierden a su padre aparecen como huérfanas. El módulo lo resuelve trayendo los ancestros
que faltan y marcándolos como cuentas de contexto, en lugar de reportarlas como anomalías del dato:
un corte del propio filtro no es un problema de la base.

### Encontrados durante la fase 3

**D29 — `BUSCAR_DATOS_MATRIZ` produce un producto cartesiano si no se filtra por ejercicio.** El
procedimiento arma el SQL concatenando texto, y las condiciones de join entre las cuatro tablas
del `FROM` (`T_MATRIZ_CONTABLE`, `T_CUENTAS_CONTABLES`, `T_OBJETOS_GASTO`, `T_RECURSOS`) están
**dentro del `IF var_ID_VIGENCIA IS NOT NULL`**. Sin vigencia, las cuatro tablas quedan sin
relacionar: con los volúmenes de Morteros serían del orden de 52 millones de filas. De paso, armar
el SQL por concatenación de parámetros es una práctica a no portar.

**D30 — `ULTIMO_NIVEL` no está cargado igual en todos los ejercicios y no coincide con la
jerarquía.** En `T_OBJETOS_GASTO`, la vigencia 201 tiene 391 partidas con `ULTIMO_NIVEL = 'S'`,
pero la 200 tiene esas mismas 391 en **NULL**. Además el flag no coincide con la estructura: en la
201 hay 46 partidas marcadas `'S'` que tienen hijas y 185 marcadas `'N'` que son hoja. Son dos
criterios distintos de "imputable" y la base no los mantiene sincronizados.

El módulo usa el flag `ULTIMO_NIVEL = 'S'`, que es el criterio explícito de negocio y está cargado
en el ejercicio activo, con un interruptor en la pantalla para ver todas. Los recursos no tienen
ese flag: ahí la hoja se detecta por no tener hijas.

### Encontrados durante la fase 6

**R21 — Las cuentas de resultado no arrastran saldo del ejercicio anterior.** No es un defecto sino
una regla contable que estaba escondida en el `WHERE` del bloque de acumulado de
`PRC_SUMAS_Y_SALDOS`:

```sql
AND (((CC.CODIGO NOT LIKE '04%' AND CC.CODIGO NOT LIKE '05%')
      AND fec_contable >= V_FECHA_INICIO_ACUM
      AND fec_contable <= V_FECHA_CORTE_ACUM))
```

Ingresos y egresos se cancelan en el cierre, así que cada ejercicio arranca de cero: sólo las
cuentas patrimoniales traen saldo de apertura. El original identifica las de resultado por el
prefijo del código, que es el plan provincial; el módulo usa la **naturaleza**, que la
configuración ya define por municipio.

La encontró el test de paridad: sin esta regla, el módulo arrastraba los ingresos de 2025 al
acumulado de 2026 y daba 115 filas distintas del original.

**D31 — La consolidación por prefijo de código deja las cuentas de nivel 1 en cero.** El reporte
acumula los movimientos hacia arriba con `cta.codigo LIKE cc.codigo || '%'`. Como el nivel 1 guarda
`"1"` y el nivel 2 guarda `"01.01"`, el `LIKE` no engancha: **ACTIVO, PASIVO, PATRIMONIO NETO,
INGRESOS y EGRESOS salen todos en cero**, o sea justo los totales que uno mira primero en un
balance. Es la misma raíz que D22. Verificado contra el stage: con 400 asientos cargados, ACTIVO
figura en 0 y debería figurar en 12.926.915,75.

**D32 — Sumar las cuentas imputables no da el total del balance.** Al consolidar hacia arriba, una
cuenta marcada como imputable que además tiene hijas (las tres de D28) queda con su importe **más**
el de sus hijas. Sumar todas las imputables cuenta esos importes dos veces: con los 400 asientos de
prueba el balance descuadraba por 358.931,09. El total se calcula sobre las **hojas** del árbol, no
sobre la marca de imputable.

**D28 — La cuenta 10309 se declara padre de sí misma.** Ya estaba en §3 como caso del árbol, pero
conviene tenerlo en el catálogo: `05.01.03.04.01` "Honorarios Profesionales" tiene
`ID_CTA_CONTABLE_REL = 10309`, o sea su propio id, y sus dos hermanas cuelgan de ella en vez de la
10308. Un recorrido recursivo ingenuo entra en bucle infinito; uno que arranque solo desde las
raíces pierde las tres cuentas sin avisar.

---

## 4. Decisiones tomadas

**Cierre y apertura entran en el alcance, parametrizados.** Se implementan R7 y R8 con la cuenta de
balanceo, la unidad administrativa y las naturalezas como configuración por municipio, y con el
defecto D3 corregido (la apertura pasa a escribirse con `ID_TIPO_ASIENTO = 7` y se rechaza la doble
ejecución).

**Los defectos de reportes se corrigen y la desviación se documenta.** Los tests de paridad de
nivel 3 registran el número viejo y el nuevo lado a lado y afirman la diferencia esperada, en lugar
de exigir igualdad. Aplica a D3, D6, D9, D10, D13 y D14. El criterio es que quede por escrito por
qué difieren, para poder explicárselo a quien haya conciliado contra los números anteriores.

### Configuración por municipio a definir

| Parámetro | Provincial | Propuesto para Morteros | Estado |
|---|---|---|---|
| Cuenta de balanceo del cierre | `11270` | `10176` — `03.01.04.01` "Resultado del ejercicio" | **Confirmada (2026-08-21)** |
| Unidad administrativa del cierre | `'Administracion Central'` (por nombre) | `1614` — numero 1, "Trasus", jurisdicción 2437 | **Resuelta (2026-08-21)** |
| Recurso D/H de balanceo | `3` / `4` | — | A verificar contra el catálogo |
| Naturalezas de resultado | `(4, 5)` | Igual: INGRESOS y EGRESOS | Coherente con el plan de Morteros |
| Naturalezas patrimoniales | `(1, 2)` | Igual: ACTIVO y PASIVO | Coherente |
| Usuario de auditoría | `'SUAF'` | Usuario autenticado del portal | Resuelto |

Ambas entradas quedaron resueltas el 2026-08-21 y verificadas contra la base.

**`CuentaBalanceoCierre = 10176`.** Es `03.01.04.01` "Resultado del ejercicio": activa, imputable,
sin hijas, naturaleza 3 (patrimonio neto) y la única cuenta con ese nombre en todo el plan. Que sea
de patrimonio neto es lo que corresponde: las cuentas de PN quedan fuera del bucle de cancelación
justamente porque ahí es donde cae el balanceo.

**`UnidadAdministrativaCierre = 1614`, no `1619`.** Esta merece explicación, porque la elección
obvia era la equivocada. Las unidades administrativas del stage son:

| UA | nº | Nombre | Jurisdicción | Ejercicio |
|---|---|---|---|---|
| 3000 | 1 | Municipalidad de Malagueño | 4000 | 2024 |
| 5 | 1 | Municipalidad De Morteros | 4000 | 2025 |
| **1614** | **1** | **Trasus** | **2437** | 2026 |
| 1619 | 2 | Municipalidad De Morteros | 2442 | 2026 |

`1619` se llama "Municipalidad De Morteros" y parece la correcta, pero pertenece a la jurisdicción
**2442**, donde no hay **ninguna** cuenta contable ni **ningún** asiento. Toda la contabilidad vive
en las jurisdicciones 2437 y 4000. El asiento de cierre habría quedado sellado con una jurisdicción
que no posee ninguna de las cuentas que el propio asiento cancela.

`1614` es la de numero 1, y el numero es lo que `JurisdiccionDeUnidad` traduce entre ejercicios:
para 2026 resuelve a 2437 y para 2025 a 4000, que es exactamente donde están los asientos de cada
uno. El numero 2 sólo existe en 2026, así que además no serviría para cerrar un ejercicio anterior.

**Limitación conocida:** la configuración es un **id de cuenta único y global**, y los ids de cuenta
están alcanzados por jurisdicción. Hoy funciona porque `10176` vive en 2437 y los asientos de todos
los ejercicios referencian cuentas de 2437 y 4000 indistintamente. Si en un ejercicio futuro se
crea una jurisdicción nueva con su propia cuenta "Resultado del ejercicio", el id configurado
seguiría apuntando a la vieja. Cuando eso pase, hay que resolver la cuenta por código en vez de
por id.

### Reglas sin origen en el PL/SQL (fase 7)

R22, R23 y R24 no portan nada: son lo que el módulo agrega sobre el sistema viejo. Como no hay
contra qué comparar, las decisiones quedan acá.

**R22 — Importes atípicos por mediana y MAD.** Se compara el importe contra la mediana de los
movimientos anteriores de esa cuenta, midiendo la distancia en desviaciones absolutas medianas. La
media y el desvío estándar no sirven: un solo asiento grande mueve la media lo suficiente como para
que después nada parezca inusual, que es justo el caso que hay que detectar. Umbrales: 5 MAD es
"inusual", 10 es "muy atípico", y con menos de 8 antecedentes no se avisa nada — con pocos datos
cualquier importe parece raro.

**R23 — Sugerencia de imputación con motivo.** Tres fuentes en orden de confianza: la cuenta que
indica la matriz contable para esa partida (puntaje 1,0), las cuentas usadas en asientos con
leyenda parecida —Jaccard sobre las palabras significativas, sin acentos, sin palabras vacías y sin
números sueltos, con un parecido mínimo de 0,34—, y las más frecuentes del ejercicio (0,1). **Cada
sugerencia lleva el motivo por el que se propone.** En contabilidad una recomendación que no se
puede justificar no sirve: nadie debería aceptarla a ciegas, y sin el motivo no hay forma de
decidir.

**R24 — Diez controles de auditoría con severidad.** Los controles se agrupan en tres severidades:
`Error` rompe la contabilidad (descuadrado, sin renglones, huérfano, fecha fuera del ejercicio),
`Advertencia` puede ser legítimo pero conviene mirarlo (cuenta no imputable, cuenta inactiva,
posible duplicado, estructura del plan) y `Sugerencia` es un dato a completar (cuenta sin matriz,
partida sin cuenta). La separación es lo que hace que se miren: en una sola lista sin jerarquía,
390 partidas sin cuenta tapan un asiento descuadrado. Cada grupo trae además la explicación de qué
significa y qué hacer, y cada control corta en 200 hallazgos avisando que recortó — una lista de
miles de filas no se revisa, se ignora.

Los duplicados se agrupan por fecha contable, importe total y leyenda normalizada. La
normalización importa: dos cargas del mismo asiento rara vez tienen la leyenda idéntica carácter
por carácter, pero sí las mismas palabras.

---

## 5. Cobertura de tests propuesta

Nivel 1, reglas puras — 20 clases, una por regla de §1.

Casos que salen directamente del análisis y que no son obvios:

- `ReglasNumeracionAsientoTests`: no reinicia entre ejercicios; dos altas concurrentes no colisionan
- `ReglasVigenciaDeAsientoTests`: la vigencia enviada por el llamador se descarta; fecha contable de
  un año sin vigencia definida
- `ReglasReversaTests`: fecha del contraasiento en ejercicio cerrado vs. abierto; el original queda
  anulado; no se reversa dos veces
- `ReglasAnulacionTests`: conserva la fecha contable original, a diferencia de la reversa
- `ReglasAsientoCierreTests`: cuenta con saldo deudor y con saldo acreedor; diferencia hacia cada
  lado; saldo cero excluido; cuenta de balanceo tomada de configuración
- `ReglasAsientoAperturaTests`: **no puede leer aperturas previas como si fueran cierres** (D3);
  doble ejecución rechazada
- `ReglasProximoCodigoTests`: primer hijo, hueco intermedio, nivel lleno (99), padre inexistente,
  nivel 7 con 4 dígitos (D8)
- `ReglasJerarquiaCuentasTests`: el árbol se arma por `ID_CTA_CONTABLE_REL`, no por prefijo (D22)
- `ReglasMatrizContableTests`: mismo recurso en distinto ejercicio **debe** poder cargarse (D9)
- `ReglasImportacionMatrizTests`: importa gastos y recursos (D10); partida sin equivalencia en la
  vigencia nueva se reporta, no se inserta en NULL (D12)
- `ReglasSaldoCuentaTests`: saldo deudor, acreedor, cero excluido, con y sin anulados (D14)
- `ReglasSumasYSaldosTests`: cuenta de jurisdicción con dos UA no duplica importes (D13)

Nivel 3, paridad: los 3 asientos reales del stage más el juego sintético, ejecutando el PL/SQL y el
C# sobre la misma entrada. Para las reglas con desviación deliberada (D3, D6, D9, D10, D13, D14) el
test registra ambos valores y afirma la diferencia esperada, en vez de exigir igualdad.

Las reglas de la fase 7 no tienen nivel 3: no hay PL/SQL contra qué comparar. Los casos que cubren:

- `ReglasImportesAtipicosTests`: menos de 8 antecedentes no avisa; un solo valor grande en el
  historial no desactiva la detección (que es lo que pasaría con media y desvío); historial con
  dispersión cero; importe menor al habitual también se marca
- `ReglasSugerenciaDeImputacionTests`: la matriz gana sobre la leyenda y la leyenda sobre la
  frecuencia; leyendas que sólo comparten palabras vacías no alcanzan el mínimo; acentos y números
  sueltos no cuentan como palabra; nunca se repite una cuenta entre fuentes
- `ReglasDuplicadosTests`: misma fecha, importe y leyenda con distinta puntuación agrupan juntos;
  distinto importe o distinta fecha no agrupan; importe cero se ignora; los grupos más numerosos
  salen primero
- `ReglasAuditoriaTests`: cada tipo de control tiene título, severidad y explicación no vacía
