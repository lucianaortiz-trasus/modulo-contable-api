# Plan — Módulo Contable SIGEMyT

Migración de la parte contable de `admfincoys` (ASP.NET WebForms + Oracle PL/SQL) a un módulo
.NET 8 + Angular 18, integrado al portal SIGEMyT y uniforme con el Módulo de Tesorería.

- Legacy de origen: `C:\Proyectos\admfin\admfincoys-main`
- Arquitectura de referencia: `D:\ModuloTesoreria`
- Base de trabajo: stage Morteros, `MUNIMST`, esquema `EJECUCION`
- Fecha del plan: 2026-08-20

---

## 1. Decisiones tomadas

| Tema | Decisión | Consecuencia |
|---|---|---|
| Acceso a datos | **Todo EF Core, sin packages PL/SQL** | Hay que extraer y portar 11.449 líneas de PL/SQL a C#. Es el trabajo central del proyecto. |
| Alcance de reportes | **Libro Diario, Mayores, Sumas y Saldos** | Se suman 3 pantallas y son la base de las estadísticas. |
| Frontend | **Angular 18** | Alineado con Reportes Financieros. Hay que bajar el `styles.scss` de Tesorería de Material 19 a 18. |
| Datos de prueba | **Generar asientos sintéticos en stage** | Único punto del proyecto donde se escribe en stage. Ver §8. |
| Tests | **xUnit + FluentAssertions**, obligatorios | Son la red de seguridad de la decisión "todo EF Core". Ver §6. |

### Por qué los tests dejan de ser opcional

Al reimplementar en C# lo que hoy resuelve la base, cada regla portada es una oportunidad de
divergir del comportamiento actual. Los tests son el mecanismo con el que demostramos que el
asiento que produce el módulo nuevo es idéntico al que produce el viejo. Sin ellos, la decisión
de no usar los packages es una apuesta; con ellos, es una mejora.

---

## 2. Estado del legacy

### Pantallas a migrar (paridad)

| # | Pantalla | Origen | Qué resuelve |
|---|---|---|---|
| 1 | Plan de Cuentas — árbol | `t_cuentas_contablesARBOL.aspx` | Vista jerárquica de 7 niveles + exportación |
| 2 | Plan de Cuentas — ABM | `t_cuentas_contablesABM.aspx` | Alta/edición de cuenta: código por nivel, nombre, activa, imputable, tipo de saldo, naturaleza, recurso debe/haber, auxiliar, asiento OPS, cuenta bancaria, inventariable, proveedor, amortización, ingreso propio |
| 3 | Matriz Contable — grilla | `t_matriz_contableGRILLA.aspx` | Filtro por vigencia/cuenta/OG/recurso, exportar, importar relación |
| 4 | Matriz Contable — ABM | `t_matriz_contableABM.aspx` | Relación cuenta contable ↔ objeto de gasto ↔ recurso |
| 5 | Asientos — grilla | `t_asientosGRILLA.aspx` | Filtros por vigencia, unidad administrativa, rango de fechas, tipo y número; ver detalle; reversar; asiento de cierre; asiento de apertura |
| 6 | Asientos — ABM | `t_asientosABM.aspx` | Cabecera + detalle, validación debe = haber, modo libro banco, imputación por cuenta/proveedor/cta. bancaria/recurso/objeto de gasto |
| 7 | ~~Tipos de Asiento — ABM~~ | `t_asientos_tipoABM.aspx.cs` | **Fuera de alcance (2026-08-21).** Ver la corrección de abajo. |

Volumen del legacy: ~9.600 líneas entre `.aspx`, code-behind, capa BL y capa DA. El grueso es
plomería ADO.NET repetitiva; la lógica real vive en la base.

#### Corrección sobre la pantalla 7 (2026-08-21)

**La descripción original estaba equivocada.** Decía "ABM del catálogo", y no es eso. Hay **tres**
tablas distintas y se confunden por el nombre:

| Tabla | Filas en Morteros | Qué es |
|---|---|---|
| `T_TIPO_ASIENTO` | 7 | El **catálogo**: MANUAL, MANUAL REVERSABLE, DE CIERRE, DE APERTURA… Alimenta el desplegable de asientos, y los ids 6 y 7 están cableados en `appsettings` |
| `T_ASIENTOS_TIPO` | 0 | **Plantillas** de asiento (cabecera) |
| `T_ASIENTOS_TIPO_DET` | 0 | Renglones de la plantilla: cuenta + `PORC_DEBE` / `PORC_HABER` |

`t_asientos_tipoABM.aspx.cs` es el ABM de las **plantillas**, no del catálogo: arma un asiento
modelo de exactamente dos cuentas, una al Debe al 100 % y otra al Haber al 100 %, sobre cuentas
imputables. Le falta el `.aspx`, así que la pantalla nunca pudo renderizarse — y eso explica que
las dos tablas tengan cero filas: **la funcionalidad nunca se usó en Morteros**.

Del catálogo, en cambio, solo se usan dos tipos: MANUAL (325 asientos) y MANUAL REVERSABLE (80).

**Decisión del usuario:** no se construye ninguna de las dos y se saca la opción del menú. El
catálogo ya se consulta desde el desplegable de Asientos y desde Inicio, y las plantillas quedarían
solapadas con la sugerencia de cuentas por leyenda de la fase 7, que resuelve el mismo problema
—no repetir la carga de cuentas habituales— sin una pantalla más que mantener.

### Pantallas nuevas (reportes)

| # | Pantalla | Procedimiento de origen |
|---|---|---|
| 8 | Libro Diario | `PKG_REPORTES_CONTABILIDAD.PRC_LIBRO_DIARIO` |
| 9 | Mayores | `PKG_REPORTES_CONTABILIDAD.PRC_MAYORES` |
| 10 | Sumas y Saldos | `PKG_REPORTES_CONTABILIDAD.PRC_SUMAS_Y_SALDOS` |

### Pantallas nuevas (inteligencia y estadísticas)

| # | Pantalla | Contenido |
|---|---|---|
| 11 | Dashboard contable | Indicadores de cabecera y accesos rápidos |
| 12 | Estadísticas | Cuatro paneles analíticos, ver §7 |
| 13 | Auditoría contable | Detección de anomalías sobre el histórico, ver §7 |

---

## 3. Hallazgos sobre la base de Morteros

Verificado por conexión directa con sqlplus contra `MUNIMST`.

### 3.1 Los packages de Morteros son un subconjunto del código legacy

El código de `admfincoys` invoca procedimientos que **no existen** en esta instalación:

| Package | Falta en Morteros | Impacto |
|---|---|---|
| `PKG_T_ASIENTOS` | `EDITAR`, `BUSCARSIGNUM`, `INSERTAR_ASIENTO_CIERRE`, `CONSULTAR_S_ASIENTO_CIERRE` | Los botones de asiento de cierre y apertura hoy fallarían |
| `PKG_T_ASIENTOS_DET` | `BUSCARPORPK`, `EDITAR`, `ELIMINAR` | Solo hay alta y búsqueda |
| `PKG_T_TIPO_ASIENTO` | `INSERTAR` | No se pueden crear tipos nuevos |
| `PKG_REPORTES_CONTABILIDAD` | `PRC_SUMAS_MAYORES`, `PRC_AUXILIARES`, `PRC_COMPOSICIONES`, `PRC_CONTROL_ASIENTOS_*`, `PRC_TCC` | Solo 3 de los 8 reportes contables existen |

Esto **valida la decisión de no depender de los packages**: la superficie disponible cambia entre
instalaciones, y un módulo que dependa de ella no es portable a otro municipio.

Contrapartida menor de lo que parecía: la fase 0 encontró que el *body* del package conserva las
implementaciones de cierre, apertura, numeración y edición. Están declaradas dentro de un bloque
`/* */` en el spec, y por eso `all_arguments` no las ve. La lógica existe y está documentada en
`_analisis/REGLAS.md`; lo que falta es parametrizar los valores atados a la instalación provincial.

### 3.2 Estructura real de las tablas

`EJECUCION.T_ASIENTOS` (25 columnas) — cabecera. PK `ID_ASIENTO`. Obligatorios: `NUMERO`, `ANIO`,
`ID_VIGENCIA`, `ID_TIPO_ASIENTO`, `FEC_CONTABLE`. Trazabilidad al origen por `ID_DEVENGADO`,
`ID_DIETDAC`, `ID_ANTICIPO`, `ID_TRANSFERENCIA`, `ID_MOV_TESO`, `ID_LIBRO_BANCO`, `ID_BIEN`,
`ID_APORTE`, `ID_SELLO`, `ID_RUR`. Reversa por `ID_ASIENTO_REL`.

> **Atención:** la tabla tiene `ID_JURISDICCION`, no `ID_UNIDAD_ADM`, pero el package recibe
> `P_ID_UNIDAD_ADM` y hace la traducción internamente. Al portar a EF Core hay que resolver ese
> mapeo de forma explícita; es una fuente de error silencioso.

`EJECUCION.T_ASIENTOS_DET` (30 columnas) — detalle. `DEBE` y `HABER` son `NUMBER(18,2)`.
Nota: el ABM legacy captura el importe con `CantidadDecimales="5"` mientras la base guarda 2
decimales. Hay truncamiento; definir el redondeo de forma explícita.

`EJECUCION.T_CUENTAS_CONTABLES` (24 columnas) — plan de cuentas. Jerarquía por
`ID_CTA_CONTABLE_REL` + `NIVEL`. `TIPO_SALDO` y `IMPUTABLE` son `VARCHAR2(1)`.

`EJECUCION.T_MATRIZ_CONTABLE` (9 columnas) — todas nullable, incluida la PK `ID_MATRIZ_CBLE`.

`EJECUCION.T_TIPO_ASIENTO` — 7 tipos cargados: `AM` MANUAL, `AMR` MANUAL REVERSABLE, `AP`
AUTOMATICO PURO, `TCC1` AUTOMATICO INDIRECTO, `TCC2` AUTOMATICO DIRECTO, `AC` DE CIERRE, `AP`
DE APERTURA. El código `AP` está duplicado en dos filas.

### 3.3 Volumen de datos en stage

| Tabla | Filas |
|---|---|
| `T_CUENTAS_CONTABLES` | 350 (niveles 1→5: 5, 7, 40, 115, 183) |
| `T_ASIENTOS` | 3 (vigencia 201, marzo a mayo 2026) |
| `T_ASIENTOS_DET` | 6 |
| `T_MATRIZ_CONTABLE` | 1 |
| `T_TIPO_ASIENTO` | 7 |

Solo el plan de cuentas sirve para desarrollar contra datos reales. Todo lo demás requiere el
juego de datos sintéticos de §8.

---

## 4. Arquitectura objetivo

Calcada de Tesorería, con nombres propios.

```
D:\Claude\Modulo-Contable-Sigemyt\
├── api\
│   ├── Contable.sln
│   ├── Contable.API\           → Controllers, middleware, DI, Program.cs
│   ├── Contable.Application\   → Services, DTOs, validaciones, AutoMapper, Reglas
│   ├── Contable.Domain\        → Entidades, enums, DbContexts, mapeos EF Core
│   └── Contable.Tests\         → xUnit + FluentAssertions
└── web\                        → Angular 18 + Material 18 + Bootstrap 5
```

### Contextos EF Core

| DbContext | Esquema | Entidades |
|---|---|---|
| `EjecucionContext` | `EJECUCION` | `TAsiento`, `TAsientoDetalle`, `TCuentaContable`, `TMatrizContable`, `TTipoAsiento` |
| `PresupuestoContext` | `PRESUPUESTO` | `TVigencias`, `TJurisdicciones`, `TUnidadesAdministrativa`, objetos de gasto, recursos |
| `PortalContext` | `PORTAL` | Usuarios, personas, roles |
| `CajaContext` | `CAJA` | Proveedores, cuentas bancarias (para los popups de imputación) |

### Convenciones heredadas de Tesorería

- `OperationResponse<T>` como contrato único de salida
- `BaseController.Return` mapea `Code` a `200` / `400` / `404`
- `BaseService` con claims: `GetUserCache`, `GetJurisdiccionCache`, `GetUnidadAdministrativa`, `GetIdVigencia`
- `GlobalExceptionHandlingMiddleware`
- Reglas de negocio compartidas en `Contable.Domain/Reglas/`, nunca duplicadas en servicios

### Integración con el portal

Idéntica a Tesorería, sin inventar nada:

1. El portal deja `usuario` y `token` en `sessionStorage` (mismo origen)
2. El módulo los lee al arrancar en `app.component.ts`
3. Pide su propio JWT a `Login/CrearTokenAdmin`
4. Obtiene los claims con `Login/GetDataToken`
5. Sin token, redirige a `https://www.sigemyt.com/portal/#!/` (o `homologacion.sigemyt.com` según hostname)

API publicada bajo `/apicontable/api/`, siguiendo el patrón de `/apitesoreria/api/`.

### Estilos

Se copia `styles.scss` de Tesorería (779 líneas, ~60 variables CSS, tema claro y oscuro) y se
adapta a Material 18. La paleta queda igual: violeta `#553EB0` como primario, verde `#2bab2d`
para confirmar, `#5c5e5e` para cancelar, encabezados `#5c5e5e` sobre blanco. Se reutilizan el
navbar, el spinner y el `theme-toggle`.

---

## 5. Reglas de negocio a portar

El corazón del proyecto. Fuente: los `.sql` extraídos en `_analisis/plsql/` más el código C#
legacy. Cada regla va a `Contable.Domain/Reglas/` con su test.

| Clase de reglas | Qué define | Origen |
|---|---|---|
| `ReglasBalanceAsiento` | Suma de debe = suma de haber. En el legacy: `t_asientosABM.aspx.cs:645-668` | C# legacy |
| `ReglasNumeracionAsiento` | Número correlativo por vigencia y año | `PKG_T_ASIENTOS` (falta `BUSCARSIGNUM` en Morteros) |
| `ReglasImputacion` | Qué auxiliares exige cada cuenta: proveedor, cuenta bancaria, recurso, objeto de gasto, según `AUXILIAR` y flags | `PKG_T_ASIENTOS_DET` + `t_asientosABM.aspx.cs` |
| `ReglasRecursoDebeHaber` | Recurso aplicable según `TIPO_SALDO` y lado del asiento | `TRAER_RECURSO_DEBE` / `TRAER_RECURSO_HABER` |
| `ReglasReversa` | Contraasiento: qué se invierte, qué se hereda, cómo se vincula por `ID_ASIENTO_REL` | `INSERTAR_REVERSA` |
| `ReglasAsientoCierre` | Cierre de ejercicio: qué cuentas se cancelan y contra qué | `INSERTAR_ASIENTO_CIERRE` (comentado en el spec) — **parametrizar** |
| `ReglasAsientoApertura` | Apertura del ejercicio siguiente desde los saldos de cierre | `INSERTAR_ASIENTO_APERTURA` (comentado en el spec) — **parametrizar y corregir** |
| `ReglasCodigoCuenta` | Composición del código de 7 niveles, cálculo del próximo código libre | `FUNC_PROXIMO_NIVEL1..7`, `TRAER_COD_NIVEL_COM`, `PRC_GENERARCODIGOS` |
| `ReglasJerarquiaCuentas` | Consistencia padre/hijo, nivel, imputable solo en hojas | `PKG_T_CUENTAS_CONTABLES` |
| `ReglasMatrizContable` | Unicidad de la relación, validez de la partida, importación entre ejercicios | `EXISTE_PARTIDA`, `IMPORTA_RELACION` |
| `ReglasSaldoCuenta` | Cálculo de saldo por naturaleza deudora o acreedora | `PRC_MAYORES`, `PRC_SUMAS_Y_SALDOS` |
| `ReglasLibroBanco` | Comportamiento del modo libro banco en el ABM | `t_asientosABM.aspx.cs` |

El detalle completo de cada regla, con los defectos detectados y la decisión de port, está en
[`_analisis/REGLAS.md`](_analisis/REGLAS.md), entregable de la fase 0.

---

## 6. Estrategia de tests

Stack idéntico al de Tesorería: **xUnit 2.9 + FluentAssertions 6.12 + EF Core InMemory**,
proyecto `Contable.Tests`, ejecutable con `dotnet test`.

### Tres niveles

**Nivel 1 — Reglas de dominio (unitarios puros, sin base).**
Una clase de test por clase de `Reglas`. Es el grueso de la suite y donde vive el valor.

- `ReglasBalanceAsientoTests` — cuadra, no cuadra, redondeo a 2 decimales, asiento de una sola línea, importes en cero, línea con debe y haber simultáneos
- `ReglasCodigoCuentaTests` — próximo código por nivel, nivel lleno, código con huecos, composición y descomposición del código de 7 niveles
- `ReglasImputacionTests` — cada combinación de cuenta con y sin auxiliar exigido
- `ReglasReversaTests` — el contraasiento invierte debe y haber, hereda la imputación, no se reversa dos veces
- `ReglasSaldoCuentaTests` — saldo deudor, acreedor, saldo cero, cuenta sin movimientos
- `ReglasMatrizContableTests` — alta duplicada, partida inexistente, importación con y sin colisiones
- `ReglasJerarquiaCuentasTests` — imputable en cuenta con hijos, nivel inconsistente, ciclo padre-hijo

**Nivel 2 — Servicios de aplicación (EF Core InMemory).**
Casos de uso completos con contexto sembrado: alta de asiento válido e inválido, búsqueda con
filtros y paginación, ABM de plan de cuentas, armado del árbol jerárquico, generación de los tres
reportes.

**Nivel 3 — Paridad contra el legacy (los más importantes).**
Tests que fijan el comportamiento actual como especificación. Se toman los 3 asientos reales del
stage más el juego sintético, se ejecuta el PL/SQL y el C# nuevo sobre la misma entrada y se
comparan los resultados fila por fila. Estos tests son los que justifican haber salido de los
packages: convierten "creemos que hace lo mismo" en "está demostrado que hace lo mismo".

Para Libro Diario, Mayores y Sumas y Saldos —que sí existen en Morteros— la paridad se puede
verificar de forma automática contra el procedimiento original.

### Regla de trabajo

Ninguna clase de `Reglas` se da por terminada sin su test. `dotnet test` en verde antes de cada
merge, igual que en Tesorería.

---

## 7. Inteligencia y estadísticas

Todo determinístico y auditable. En un módulo contable, un indicador que no se puede explicar no
sirve: cada número tiene que poder abrirse hasta el asiento que lo origina.

### Dashboard contable (pantalla 11)

Asientos del ejercicio, monto acumulado, asientos del mes, último asiento registrado, cuentas
imputables activas, cobertura de la matriz contable, alertas abiertas.

### Estadísticas (pantalla 12), cuatro paneles

1. **Actividad** — asientos por mes, por tipo y por unidad administrativa; evolución del monto
   registrado; comparación interanual
2. **Cuentas** — ranking de cuentas por volumen movido, distribución debe/haber, cuentas
   imputables sin movimiento en el ejercicio, concentración del movimiento
3. **Saldos** — evolución del saldo por cuenta y por capítulo, sobre `ReglasSaldoCuenta`, con
   drill-down a Mayores y de ahí al asiento
4. **Calidad de registración** — desfase entre fecha contable y fecha de alta, asientos cargados
   fuera de período, distribución de la carga por usuario

### Auditoría contable (pantalla 13)

Detección de anomalías sobre el histórico, cada hallazgo con enlace directo al asiento:

- Asientos descuadrados (debe ≠ haber) que hayan quedado persistidos
- Asientos sin detalle, o con detalle sin cabecera
- Movimientos sobre cuentas no imputables o inactivas
- Asientos con fecha contable fuera de la vigencia declarada
- Posibles duplicados por leyenda, importe y fecha
- Cuentas del plan sin relación en la matriz contable
- Objetos de gasto y recursos sin cuenta contable asociada
- Importes atípicos respecto del histórico de esa cuenta

### Asistencia durante la carga

Donde la inteligencia se paga sola, porque evita el error en lugar de detectarlo después:

- **Sugerencia de imputación**: al elegir objeto de gasto o recurso, proponer la cuenta contable
  según `T_MATRIZ_CONTABLE` y el histórico de asientos con leyenda parecida
- **Autobalanceo**: proponer el importe de la contrapartida que cuadra el asiento
- **Validación en vivo**: avisar antes de guardar, no después
- **Plantillas**: repetir un asiento frecuente desde el histórico

> Queda abierto si además se suma asistencia por LLM para interpretar la leyenda en texto libre y
> proponer la imputación completa. Es viable y encaja con el resto, pero es una decisión aparte
> porque introduce una dependencia externa en un flujo contable. Lo dejo planteado, no incluido.

---

## 8. Datos de prueba en stage

Único punto del proyecto donde se escribe en la base. Reglas duras:

- Solo en el stage de Morteros, solo en `EJECUCION`, solo `T_ASIENTOS` y `T_ASIENTOS_DET`
- Los asientos sintéticos van con una leyenda con prefijo identificable, para poder borrarlos
- Script de alta y script de limpieza, ambos versionados, entregados juntos
- No se toca `T_CUENTAS_CONTABLES` (350 filas reales), ni `T_MATRIZ_CONTABLE`, ni ningún otro esquema
- **Antes de insertar la primera fila te muestro exactamente qué se va a insertar y espero tu OK**

Objetivo: unos cientos de asientos repartidos en 2 o 3 ejercicios, con distribución realista por
tipo, cuenta y mes, suficiente para que las estadísticas muestren algo y para detectar problemas
de performance en las grillas paginadas.

---

## 9. Riesgos

| # | Riesgo | Mitigación |
|---|---|---|
| 1 | **Cierre y apertura atados a la instalación provincial.** La lógica existe pero depende de una cuenta de balanceo (`11270`) y una unidad administrativa (`'Administracion Central'`) que no existen en Morteros. Por eso están desactivados. | Externalizar esos valores a configuración por municipio. Requiere que el contador defina la cuenta de balanceo de Morteros. Ver `_analisis/REGLAS.md` §2 y §4. |
| 2 | **Divergencia con el legacy al reimplementar en C#.** | Tests de paridad de Nivel 3 sobre datos reales antes de dar por buena cada regla. |
| 3 | **Mapeo `ID_UNIDAD_ADM` ↔ `ID_JURISDICCION`.** El package hace una traducción implícita. | Resolverlo explícito y con test propio. Documentado en §3.2. |
| 4 | **Truncamiento de decimales.** El ABM captura 5 decimales, la base guarda 2. | Definir el redondeo de forma explícita y cubrirlo con test. |
| 5 | **Stage casi vacío.** | Juego de datos sintéticos, §8. |
| 6 | **Código `AP` duplicado en `T_TIPO_ASIENTO`.** | Verificar cuál usa cada flujo antes de tocarlo. No es un dato a corregir sin consultar. |
| 7 | **Angular 18 con estilos escritos para Material 19.** | Adaptar el `styles.scss` en la fase 1 y validar visualmente contra Tesorería antes de construir pantallas. |

### Fuera de este plan

El `appsettings.json` de Tesorería tiene credenciales reales versionadas: claves AWS, contraseña
SMTP y el secreto JWT. No es parte de este trabajo, pero si vamos a calcar esa configuración
conviene no arrastrar el problema al módulo nuevo. Lo menciono para que lo tengas presente.

---

## 10. Fases

Cada fase deja algo verificable. No se avanza a la siguiente con tests en rojo.

### Fase 0 — Análisis del PL/SQL
Extraer y leer los packages, documentar cada regla en una tabla de trazabilidad
`procedimiento PL/SQL → clase de Reglas → test`. Resolver el riesgo #1.
**Entregable:** documento de reglas con la traza completa.
**Estado:** parcialmente hecho, los `.sql` ya están en `_analisis/plsql/`.

### Fase 1 — Esqueleto
Solución .NET 8 de 4 proyectos, DbContexts y entidades EF Core, integración con el portal,
proyecto Angular 18 con estilos adaptados y navbar. Health check y un endpoint de humo.
**Entregable:** el módulo arranca, entra desde el portal, muestra el navbar con los colores correctos.
**Estado: terminada.** Compila sin warnings, 9 tests en verde, verificado contra el stage de
Morteros: `GET /api/Diagnostico` devuelve 350 cuentas y 7 tipos de asiento reales, y el handoff
del portal resuelve la identidad completa. Ver `README.md`.

### Fase 2 — Plan de Cuentas
Reglas de código y jerarquía con sus tests, servicio, API, árbol y ABM. Es la mejor pantalla para
empezar: tiene 350 filas reales, no depende de nada más y ejercita la jerarquía.
**Entregable:** pantallas 1 y 2 funcionando contra datos reales.
**Estado: terminada.** 96 tests en verde, ABM completo verificado contra el stage. Aparecieron tres
defectos nuevos del dato, documentados como D26 a D28 en `_analisis/REGLAS.md`: la convención
`'A'`/`'I'` de la columna `ACTIVA`, la mezcla de jurisdicciones dentro del mismo árbol, y la cuenta
que se declara padre de sí misma.

### Fase 3 — Matriz Contable
Reglas, tests, servicio, API, grilla y ABM, importación entre ejercicios y exportación.
**Entregable:** pantallas 3 y 4.
**Estado: terminada.** 120 tests en verde. Los cuatro defectos de la matriz (D9 a D12) quedaron
corregidos y verificados contra el stage, incluida la importación que ahora sí traslada el lado
recurso. Se sumaron dos defectos nuevos al catálogo: D29 (producto cartesiano en
`BUSCAR_DATOS_MATRIZ` sin filtro de ejercicio) y D30 (`ULTIMO_NIVEL` desparejo entre ejercicios).

### Fase 4 — Asientos
La fase más pesada. Reglas de balance, numeración, imputación y reversa con sus tests; servicio
transaccional; grilla con filtros y paginación; ABM cabecera-detalle; reversa. Cierre y apertura
según lo que resuelva la fase 0.
**Entregable:** pantallas 5, 6 y 7, con tests de paridad contra los asientos reales.
**Estado: terminada.** 174 tests en verde. Alta transaccional, reversa, anulación, y cierre y
apertura parametrizados, todo verificado contra el stage. Quedan dos cosas fuera: el modo libro
banco, que escribe en la tabla de Tesorería y necesita su propia decisión, y la pantalla de tipos
de asiento, que se resolvió como catálogo de solo lectura en el selector del ABM.

### Fase 5 — Datos de prueba
Scripts de alta y limpieza, con tu OK previo. Verificación de performance de las grillas.
**Entregable:** stage con volumen suficiente y script de limpieza probado.
**Estado: terminada.** 400 asientos sintéticos con prefijo `[QA]` en 2025 y 2026, generados tras
mostrar la simulación y con tu OK. Los tres scripts están en `_datos-de-prueba/`. El volumen
destapó dos problemas de la grilla de asientos, ya corregidos: cinco consultas por página en vez
de dos, y el filtro de anulados aplicado después de paginar.

### Fase 6 — Reportes
Libro Diario, Mayores y Sumas y Saldos, con paridad automática contra el PL/SQL original.
**Entregable:** pantallas 8, 9 y 10.
**Estado: terminada.** 198 tests en verde y paridad cerrada contra el PL/SQL: 286 filas idénticas y
64 diferencias, todas clasificadas en defectos documentados, cero sin explicar. Aparecieron una
regla contable que faltaba (R21: las cuentas de resultado no arrastran saldo del ejercicio anterior)
y dos defectos nuevos, D31 y D32.

### Fase 7 — Inteligencia y estadísticas
Dashboard, los cuatro paneles, auditoría contable y la asistencia durante la carga.
**Entregable:** pantallas 11, 12 y 13.
**Estado: terminada.** 225 tests en verde. Tres clases de reglas nuevas —importes atípicos por
mediana y MAD, sugerencia de imputación con motivo, y el catálogo de diez controles de auditoría—,
dos servicios, dos controllers y dos pantallas. La ayuda de carga quedó dentro del ABM de asientos:
sugiere cuentas desde la leyenda y avisa cuando un importe se aparta de lo habitual para su cuenta.
Ninguna de las dos bloquea: proponen y explican.

### Fase 8 — Cierre
Suite completa en verde, `llm.md` del módulo con el mismo formato que el de Tesorería,
documentación de despliegue.
**Estado: terminada.** 225 tests en verde, `api/llm.md` con las 12 secciones del formato de
Tesorería, `DESPLIEGUE.md` con la configuración a revisar y los 7 pasos de verificación posterior,
y los dos scripts de build (`build-api.cmd` y `build-web.cmd`), ambos corridos de punta a punta.

### Fase 9 — Exportación, builds y manual
Pedido posterior al plan original: reportes descargables, un build por pieza y manual de usuario
dentro de la aplicación.
**Estado: terminada.**
- Los tres reportes se descargan en **PDF apaisado** (A4, 841,89 × 595,28 pt) y en **Excel** con
  los importes como números. Las dos librerías se cargan bajo demanda, así que el bundle inicial
  casi no crece.
- `build-api.cmd` y `build-web.cmd`, con la suite de tests como condición para publicar.
- Pantalla **Manual de usuario** con nueve secciones, índice que sigue al scroll y hoja de estilos
  de impresión.
