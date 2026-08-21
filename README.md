# Módulo Contable SIGEMyT

Migración de la parte contable de `admfincoys` (ASP.NET WebForms + Oracle PL/SQL) a .NET 8 +
Angular 18, integrada al portal SIGEMyT y uniforme con el Módulo de Tesorería.

| | |
|---|---|
| Plan de trabajo | [`PLAN.md`](PLAN.md) |
| Reglas contables extraídas del PL/SQL | [`_analisis/REGLAS.md`](_analisis/REGLAS.md) |
| Fuentes PL/SQL originales | `_analisis/plsql/` (7 packages, 11.449 líneas) |
| Base de trabajo | Stage Morteros, `MUNIMST`, esquema `EJECUCION` |

---

## Estructura

```
api/
├── Contable.sln
├── Contable.API/          Controllers, middleware, DI, health checks
├── Contable.Application/  Servicios, DTOs, AppSettings
├── Contable.Domain/       Entidades, DbContexts, Reglas
└── Contable.Tests/        xUnit + FluentAssertions
web/                       Angular 18 + Material 18 + Bootstrap 5
```

## Cómo se levanta

Desde la raíz de este repositorio:

```bash
dotnet run --project Contable.API/Contable.API.csproj --urls http://localhost:5180
```

En Claude Code alcanza con el perfil `contable-api` de `.claude/launch.json`, que hace exactamente
eso. Las rutas de ese archivo son **relativas a la raíz del repositorio**, para que funcione en
cualquier máquina donde se clone.

- API: `http://localhost:5180`, Swagger en `/swagger`, health en `/health`
- El front está en [`efinten/modulo-contable-web`](https://github.com/efinten/modulo-contable-web)
  y levanta en `http://localhost:4400`. Se necesitan los dos para trabajar.

```bash
dotnet test
```

## Configuración

`appsettings.json` va **sin secretos** a propósito. Ver
[`api/Contable.API/README-configuracion.md`](api/Contable.API/README-configuracion.md).
El `appsettings.Development.json` está en el `.gitignore`.

## Integración con el portal

El Portal SIGEMyT federa la sesión **por querystring**, con el mismo contrato que usan los doce
módulos de la plataforma:

```
1. El usuario entra al Portal      POST api/Seguridad/ValidarUsuario  → { token, usuario }
2. Clickea el módulo               .../nuevo-contable/?token=…&idJur=…&numJur=…
3. El front llama a su backend     GET  .../nuevo-contable-api/api/Seguridad/Login?atoken=…
4. Ese backend llama al Portal     GET  {Portal:AuthUrl}?atoken=…
5. El Portal responde              200 { token, usuario { … } }  |  401
```

Para el módulo el token es **opaco**: lo reenvía tal cual y no lo desencripta nunca. Lo que devuelve
al front es su propio JWT, con jurisdicción, unidad administrativa y vigencia ya resueltas en los
claims.

> **Esto arrancó implementado mal.** El módulo copió el mecanismo de Tesorería —leer `usuario` y
> `token` de `sessionStorage`— y con eso el Portal real lo rebotaba: no encontraba identidad,
> redirigía al Portal, el Portal devolvía al módulo, y así en un bucle sin ningún mensaje.
> `sessionStorage` quedó como respaldo, pero el camino real es el de arriba.

Dos detalles que importan:

- **El `idJur` del Portal se traduce, no se usa directo.** Llega la jurisdicción del ejercicio donde
  está parado el usuario —en Morteros llega `4000`, que es de 2024/2025— y el módulo la traduce por
  `NUMERO_JURISDICCION` a la vigencia activa (`2437`). Usarla tal cual filtraría todo por un
  ejercicio viejo.
- **El token se borra de la barra de direcciones** apenas se lee. Es una credencial: no tiene por
  qué quedar en el historial ni en lo que el usuario copie para pedir ayuda.

- **La sesión sobrevive al F5.** El token del módulo se guarda en `sessionStorage` bajo
  `contable.token`. Antes vivía solo en memoria y cualquier recarga devolvía al usuario al portal.

Si el ingreso falla, la pantalla muestra **el motivo** y no redirige. La excepción es una sesión
guardada que venció: ahí sí vuelve al portal, y no hace bucle porque el portal manda de nuevo con
`?token=`.

### La trampa de `UseAuth`

`Uses:UseAuth` está en `false` en desarrollo y en `true` en producción. Con `true`, la política de
respaldo exige token en **todo** endpoint sin `[AllowAnonymous]` — y eso crea una clase de bugs que
**no se ven en desarrollo**.

Pasó exactamente eso: `Login/GetDataToken` no está marcado como anónimo, y el interceptor del front
lo excluía de mandar el header "porque es del login". En producción daba 401 y nadie podía entrar.
Ahora el interceptor solo excluye lo que de verdad emite el token, y `GetDataToken` lee los claims
del usuario autenticado en vez de recibir el token por query string — lo que de paso cierra la deuda
de tener la credencial en la URL.

**Si tocás autenticación, probalo con `UseAuth: true`.**

Publicación real en Morteros:

| Pieza | URL |
|---|---|
| Front | `https://www.administracionho.morteros.gob.ar/nuevo-contable` |
| API | `https://www.administracionho.morteros.gob.ar/nuevo-contable-api` |

El front cuelga de un **subdirectorio**, así que la build de producción lleva
`baseHref: "/nuevo-contable/"` en `angular.json`. Es lo único que no se resuelve en tiempo de
ejecución: si la ruta cambia, hay que recompilar. La ruta de la API sí se resuelve en el navegador,
en `services/environment.ts`. Detalle completo en [DESPLIEGUE.md](DESPLIEGUE.md).

## Estado por fase

| Fase | Estado |
|---|---|
| 0 — Análisis del PL/SQL | **Terminada.** 20 reglas trazadas, 25 defectos catalogados |
| 1 — Esqueleto | **Terminada.** Ver abajo |
| 2 — Plan de Cuentas | **Terminada.** Ver abajo |
| 3 — Matriz Contable | **Terminada.** Ver abajo |
| 4 — Asientos | **Terminada.** Ver abajo |
| 5 — Datos de prueba | **Terminada.** Ver abajo |
| 6 — Reportes | **Terminada.** Ver abajo |
| 7 — Inteligencia y estadísticas | **Terminada.** Ver abajo |
| 8 — Cierre | **Terminada.** Ver abajo |
| 9 — Exportación, builds y manual | **Terminada.** Ver abajo |

### Qué quedó funcionando en la fase 1

- Solución .NET 8 de cuatro proyectos, compila sin warnings
- Tres `DbContext` (`EJECUCION`, `PRESUPUESTO`, `PORTAL`) con las cinco entidades contables mapeadas
  columna por columna contra la base real
- Integración con el portal, verificada de punta a punta contra el stage
- Health checks por esquema
- Angular 18 con la paleta de Tesorería (violeta `#553EB0`), tema claro y oscuro, navbar con el
  alcance completo del módulo y las pantallas pendientes apagadas
- Endpoint `GET /api/Diagnostico`: prueba de humo que recorre los tres contextos

### Qué quedó funcionando en la fase 2

- Cuatro clases de reglas de dominio en `Contable.Domain/Reglas/`: `CodigoCuenta`,
  `ReglasProximoCodigo`, `ReglasJerarquiaCuentas` y `ArbolDeCuentas`
- `PlanDeCuentasService` con árbol, búsqueda paginada, ABM y cálculo del próximo código
- `PlanDeCuentasController` con siete endpoints
- Pantalla de Plan de Cuentas: árbol jerárquico con filtro, panel de detalle y ABM, panel de
  anomalías, tema claro y oscuro
- **96 tests en verde** (eran 9 al terminar la fase 1)

Verificado de punta a punta contra el stage: alta, edición y baja desde la pantalla, con la base
volviendo a sus 350 cuentas originales sin residuos.

### Qué quedó funcionando en la fase 3

- Dos clases de reglas nuevas: `ReglasMatrizContable` y `ReglasImportacionMatriz`
- Entidades `TObjetoGasto` y `TRecurso` sobre `PRESUPUESTO`
- `MatrizContableService` con grilla, ABM, selectores de partidas e importación entre ejercicios
- Pantalla de Matriz Contable: grilla paginada, ABM con buscadores incrementales de cuenta y
  partida, e importación con **vista previa antes de confirmar**
- **120 tests en verde**

La importación se verificó de punta a punta contra el stage: dos relaciones de recurso cargadas en
2025 se trasladaron a 2026 con los ids traducidos por número de partida, la segunda corrida no
duplicó nada, y la base volvió a su única fila original.

Los cuatro defectos del PL/SQL que documenta `_analisis/REGLAS.md` quedaron corregidos:

| Defecto | Qué hacía el sistema viejo | Qué hace ahora |
|---|---|---|
| D9 | Control de duplicados asimétrico: gasto por par sin ejercicio, recurso solo por recurso | Control simétrico por (cuenta, partida, ejercicio) |
| D10 | La importación filtraba `id_recurso is null`: solo traía gastos | Importa gasto y recurso |
| D11 | El guard hacía `count(*) + 1` y nunca se disparaba | Guard real, con mensaje propio por tipo de partida |
| D12 | Partida sin equivalente se insertaba en `NULL`, sin avisar | Se informa fila por fila con el motivo, no se inserta |

### Qué quedó funcionando en la fase 4

- Cuatro clases de reglas nuevas: `ReglasBalanceAsiento`, `ReglasAsiento` con
  `ReglasNumeracionAsiento`, `ReglasContrapartida` y `ReglasCierreEjercicio`
- `AsientoService` con grilla, detalle, alta transaccional, reversa, anulación, y cierre y
  apertura de ejercicio con vista previa
- Pantalla de Asientos: grilla con filtros, detalle con totales, ABM cabecera-renglones con
  **indicador de balance en vivo**, y los diálogos de reversa, anulación, cierre y apertura
- **174 tests en verde**

Verificado de punta a punta contra el stage: alta desde la pantalla (asiento N° 4, continuando la
numeración real de la jurisdicción), reversa con su contraasiento invertido, anulación, y la vista
previa del cierre calculada sobre los saldos reales. La base volvió a sus 3 asientos originales.

**El cierre de 2026 no se confirmó**: es el ejercicio activo y cerrarlo habría sido un error. La
vista previa se verificó a mano contra los saldos: la cuenta de egresos queda con saldo deudor de
1.000 y el descuadre patrimonial es de 1.000 al haber, que es lo que el algoritmo manda a la cuenta
de balanceo. Los dos asientos cuadran.

Defectos del PL/SQL corregidos en esta fase:

| Defecto | Qué hacía el sistema viejo | Qué hace ahora |
|---|---|---|
| D1, D2 | Cuenta de balanceo y unidad administrativa escritas a mano, inexistentes en Morteros | Configuración por municipio, con mensaje que nombra la clave que falta |
| D3 | La apertura se grababa con el tipo de cierre y después se releía como cierre | Tipo propio, y la configuración rechaza que sean iguales |
| D4 | `MAX(numero)+1` sin bloqueo: dos altas simultáneas tomaban el mismo número | `FOR UPDATE` sobre el asiento de mayor número antes de leer el máximo |
| Alta en dos pasos | Cabecera y renglones por procedimientos separados, sin garantía de que cuadre | Una sola transacción, y el balance se valida antes de grabar |

### Qué quedó funcionando en la fase 5

Tres scripts versionados en [`_datos-de-prueba/`](_datos-de-prueba/README.md): generar, verificar
y limpiar. El stage tiene ahora **400 asientos sintéticos** (150 en 2025, 250 en 2026) más los 3
reales, todos con el prefijo `[QA]` en la leyenda.

El generador es determinista y toma las cuentas del plan real por naturaleza, así que sirve para
cualquier municipio. Los tres controles de `verificar.sql` dan cero: sin asientos descuadrados, sin
números repetidos por jurisdicción, sin renglones sobre cuentas no imputables.

**Lo que destapó el volumen.** Con 3 asientos todo parecía rápido; con 400 aparecieron dos cosas:

- **La grilla hacía cinco consultas por página.** Contra una base en `us-east-2` cada ida y vuelta
  son ~180 ms, así que una página de 20 filas tardaba 1,10 s. Los totales de cada asiento ahora se
  resuelven con subconsultas escalares dentro de la misma consulta de la página: **dos viajes en
  lugar de cinco, 0,40 s**. Es el piso que permite la latencia.
- **El filtro de anulados se aplicaba en memoria después de paginar**, así que recortaba la página
  pero dejaba el total sin tocar: la grilla decía "1-20 de 253" mostrando menos filas. Ahora el
  filtro va en la consulta.

El cierre de ejercicio, que con los 3 asientos reales generaba renglones contados, ahora produce
asientos de 99 y 58 renglones en 2026 y de 68 y 58 en 2025. Los cuatro cuadran.

### Qué quedó funcionando en la fase 6

- Dos clases de reglas nuevas: `ReglasSumasYSaldos` y `ReglasMayor`
- `ReportesService` con los tres reportes que existen en Morteros
- Pantalla de Reportes con tres solapas, jerarquía sangrada y explicación del descuadre
- **198 tests en verde**
- Un arnés de paridad en [`_analisis/paridad/`](_analisis/paridad/) que compara el PL/SQL original
  contra el módulo, fila por fila

**El resultado de la paridad**, sobre las 350 cuentas del balance de 2026:

```
coinciden exacto   : 286
con diferencias    :  64
   224  D13: el original duplica por el cruce con unidades administrativas
    11  D31: el original deja las cuentas de nivel 1 en cero
     4  D28: la cuenta que se apunta a sí misma queda fuera de su rama
     0  SIN CLASIFICAR
```

Cero diferencias sin explicar. El arnés no busca que los reportes coincidan —no deberían, porque el
viejo tiene defectos— sino que **toda** diferencia corresponda a un defecto documentado. Si aparece
una que no encaja, el script termina con error.

Cuando el balance no cuadra, el reporte explica por qué en lugar de dejar al contador buscando: con
los datos actuales avisa que el ejercicio 2025 no tiene asiento de cierre y que por eso el resultado
del ejercicio anterior no llegó al patrimonio neto.

### Qué quedó funcionando en la fase 7

Dos pantallas nuevas y dos ayudas dentro del ABM de asientos.

**Estadísticas** (`/estadisticas`, `EstadisticasController`) resume el ejercicio en cinco
indicadores y cuatro paneles: actividad por mes contra el mismo corte del ejercicio anterior,
asientos por tipo, cuentas más movidas con su concentración, cuentas imputables que nunca se
usaron, y calidad de la registración —demora de carga por tramos, quién registra, leyendas poco
útiles y porcentaje anulado—. Los cuatro paneles son consultas independientes que salen juntas, así
que la pantalla se dibuja entera y no de a pedazos.

**Auditoría** (`/auditoria`, `AuditoriaController`) corre diez controles y agrupa los hallazgos por
tipo. Cada grupo trae severidad y una explicación de qué significa y qué conviene hacer: un control
sin explicación no se acciona. Los errores se despliegan solos; el resto arranca plegado. Sobre los
datos de stage el ejercicio 2026 da **0 errores, 16 advertencias y 544 sugerencias**, con 7 de 10
controles limpios. Las 16 advertencias son las mismas anomalías de estructura del plan que ya había
detectado la fase 2.

La separación por severidad es deliberada. Un asiento descuadrado rompe la contabilidad; una
partida sin cuenta en la matriz es apenas un dato a completar. Mezclarlos en una sola lista hace
que no se mire ninguno.

**Ayuda durante la carga.** Al escribir la leyenda, el ABM propone cuentas en tres fuentes
ordenadas: la que indica la matriz contable, las que se usaron en asientos con leyenda parecida
(Jaccard sobre las palabras significativas), y las más frecuentes del ejercicio. Cada propuesta
lleva su motivo — una sugerencia contable que no se puede justificar no sirve, porque el contador no
la va a aceptar a ciegas ni debería. Al cargar el importe, si se aparta de lo habitual para esa
cuenta aparece un aviso con el importe típico y cuánto se aparta. Ninguna de las dos bloquea el
guardado.

Los importes atípicos se miden con **mediana y MAD**, no con media y desvío: un solo asiento grande
mueve la media lo suficiente como para que después nada parezca inusual. Hacen falta al menos 8
antecedentes; con menos no se avisa nada.

Desde un hallazgo de auditoría se llega al asiento (`/asientos?numero=N`) o a la cuenta
(`/plan-de-cuentas?buscar=<codigo>`). Los hallazgos sobre partidas presupuestarias no se muestran
como enlace porque no hay pantalla de este módulo a donde llevarlos.

### Qué quedó funcionando en la fase 8

- **`api/llm.md`**, con el mismo formato que el de Tesorería: arquitectura, contratos, mapa de
  datos, las reglas, las trampas de EF Core contra Oracle, el mapa funcional, seguridad y una guía
  de "si tenés que tocar X, andá a Y".
- **`DESPLIEGUE.md`**: cómo se compila cada pieza, qué configuración *no puede quedar como está*,
  cómo se publica el front, y siete pasos de verificación en orden. El último es correr la
  auditoría: si el despliegue quedó apuntando al esquema o al ejercicio equivocado, se nota ahí.
- **Suite completa en verde**: 225 tests.

### Qué quedó funcionando en la fase 9

**Exportación de reportes.** Los tres reportes se descargan con el corte que está en pantalla:

| | Para qué |
|---|---|
| PDF | Apaisado A4, encabezado del corte en cada hoja y numeración. Para imprimir, firmar o adjuntar. |
| Excel | Importes como **números**, no como texto: se suman, ordenan y filtran. Fila de títulos fija y autofiltro puesto. Para trabajar los datos. |

`ExportarService` no sabe nada de contabilidad: recibe una forma neutra (`DocumentoExportable`) y
la escribe en los dos formatos. La traducción de cada reporte a esa forma está en
`reportes-exportables.ts`, en funciones puras. Agregar un cuarto reporte exportable es escribir una
función más, no tocar el exportador.

`jspdf` y `exceljs` se cargan con `import()` dinámico: entre las dos suman más de un mega y casi
nadie exporta. Quedan en chunks aparte y el bundle inicial pasó de 1,32 MB a 1,36 MB.

> **`exceljs` es CommonJS, no ESM.** En el build de desarrollo el bundler sintetiza los nombres y
> `modulo.Workbook` existe; en el de producción el chunk exporta **solo** `default` y
> `modulo.Workbook` queda `undefined`. Hay que tomarlo de `default`. Esto pasó en producción y en
> desarrollo no se veía: **las exportaciones se prueban contra la salida de `build-web.cmd`, no contra `ng serve`.**

**Builds.** `build-api.cmd` y `build-web.cmd`. El de la API corre la suite antes de publicar y
aborta si algo está en rojo —verificado rompiendo un test a propósito—, y saca
`appsettings.Development.json` del paquete porque tiene credenciales.

Son `.cmd` y no `.ps1` porque la directiva de grupo de la máquina está en `AllSigned`: un `.ps1`
sin firmar no corre ni con `-ExecutionPolicy Bypass`.

**Manual de usuario** (`/manual`, nueva opción del menú). Nueve secciones, índice lateral que sigue
al scroll, y hoja de estilos de impresión que oculta el índice y arranca cada sección en hoja
nueva. Está escrito para quien va a usar el módulo, no para quien lo programó: explica cuándo
anular y cuándo reversar, por qué el balance puede no cuadrar, y qué hacer cuando algo no sale.

### Deuda de la fase 6

El mayor **de todas las cuentas** tarda unos diez segundos: arma 155 mayores en una sola respuesta.
El caso normal —una cuenta— responde en menos de dos. Conviene paginarlo o exigir cuenta antes de
ponerlo en producción.

### Fuera del alcance de la fase 4

**El modo libro banco no está conectado.** `PKG_T_ASIENTOS_DET.INSERTAR_DET` inserta además un
movimiento en `EJECUCION.T_LIBRO_BANCO` cuando el renglón trae cuenta bancaria. Es una escritura
dentro del dominio de Tesorería y merece su propia decisión, así que el módulo acepta y guarda
`ID_CUENTA_BANCARIA` en el renglón pero no genera el movimiento de libro banco. La regla está
documentada como R9 en `_analisis/REGLAS.md`.

### Notas técnicas del port

**No usar `Any()` / `AnyAsync()` en consultas EF Core.** El proveedor de Oracle las traduce a una
comparación contra el literal `FALSE`, que Oracle 12.2 no entiende: responde
`ORA-00904: "FALSE": identificador no válido`. Los literales booleanos en SQL recién aparecen en
Oracle 23c. Usar `Take(1).CountAsync() > 0`, que además corta después de la primera fila. Hay un
helper `Existe<T>` en `PlanDeCuentasService`.

**Modo lectura en formularios reactivos.** Poner `readonly` solo en los `input` deja los
`mat-checkbox` y los `mat-select` operables. Se habilita y deshabilita el `FormGroup` completo
desde el código, y se lee con `getRawValue()` porque `value` omite los controles deshabilitados.

**Tampoco usar `Contains()` sobre una subconsulta.** Mismo problema que `Any()`: el proveedor de
Oracle lo traduce con un literal booleano. Para "los que no tienen hijos" va un `LEFT JOIN` con
`IS NULL`, que además no tiene el límite de mil elementos que tiene un `IN`.

**No editar TypeScript ni C# con `perl -pi`.** Perl interpola `${...}`, que es exactamente la
sintaxis de los template literals de TypeScript: una sustitución dejó `` `/` `` donde decía
`` `${this.url}/${id}` `` y rompió el borrado del plan de cuentas sin que el compilador dijera
nada. Para editar código va la herramienta de edición, no la sustitución por texto.

**Cuidado al capturar CSV desde sqlplus.** `set markup csv on quote off` rompe cuando un campo de
texto tiene comas — hay nombres de cuenta como `Electr., Gas y Agua...` que corren todas las
columnas una posición. Durante la paridad eso hizo parecer que el PL/SQL invertía el debe y el
haber, cuando el error era del parseo. Va `quote on` y un parser que respete las comillas. También
conviene `set numwidth 40`, porque sqlplus redondea los números al mostrarlos.

**`toISOString()` no sirve para una fecha local.** Devuelve UTC: en Argentina (UTC-3), después de
las 21:00 da el día siguiente, y el asiento salía con fecha futura. La fecha del día se arma con
`getFullYear()` / `getMonth()` / `getDate()`.

### Parámetros contables

La sección `Contabilidad` de `appsettings.json` externaliza lo que en el PL/SQL provincial estaba
escrito a mano. Los dos parámetros que faltaban quedaron resueltos el **2026-08-21** y verificados
contra la base:

| Clave | Valor | Qué es |
|---|---|---|
| `CuentaBalanceoCierre` | `10176` | `03.01.04.01` "Resultado del ejercicio". Activa, imputable, sin hijas, de patrimonio neto, y la única con ese nombre en el plan |
| `UnidadAdministrativaCierre` | `1614` | Numero 1, "Trasus", jurisdicción 2437 |

**La unidad administrativa tenía trampa.** La que se llama "Municipalidad De Morteros" es la `1619`,
y es la equivocada: pertenece a la jurisdicción **2442**, donde no hay ninguna cuenta contable ni
ningún asiento. Toda la contabilidad vive en las jurisdicciones 2437 y 4000. Con `1619` el cierre se
genera igual y cuadra, pero el asiento queda sellado con una jurisdicción que no posee ninguna de
las cuentas que él mismo cancela — un error que no se nota mirando el asiento.

El criterio correcto no es el nombre sino el **numero**, que es lo que el módulo traduce entre
ejercicios: la UA de numero 1 resuelve a 2437 en 2026 y a 4000 en 2025, que es donde están los
asientos de cada uno.

Con la configuración puesta, la vista previa del cierre de 2026 genera **dos asientos** —el de
resultado y el patrimonial—, ambos cuadrados, con 99 y 58 renglones, y el balanceo cayendo en
`03.01.04.01` por 245.269,28: al Debe en el de resultado y al Haber en el patrimonial. Es el
resultado del ejercicio pasando al patrimonio neto.

Los dos valores son **específicos de Morteros**. Para otro municipio hay que volver a averiguarlos;
copiarlos sería exactamente el error que este módulo vino a corregir del PL/SQL provincial.

## Deuda conocida

- **Angular 18 tiene una vulnerabilidad XSS alta sin corrección dentro de la misma major**
  (`GHSA-jj27-h5hq-8x99`). La versión se eligió para alinear con Reportes Financieros. Conviene
  decidir si se sube de major antes de la puesta en producción.
- ~~`Login/GetDataToken` recibe el token por query string~~ — **resuelto el 2026-08-21.** Ahora lee
  los claims del usuario autenticado y el token viaja en el header `Authorization`. Salió a la luz
  porque era la causa de un 401 en producción.
- **El tema oscuro no está auditado en las pantallas anteriores a la fase 7.** Bootstrap deja un
  gris casi negro heredado en el `body`, así que cualquier texto sin color propio queda invisible
  sobre fondo oscuro. En la fase 7 se corrigió el origen para las tablas (`.table` ahora usa las
  variables del tema en vez de las de Bootstrap) y se arregló el nombre de cuenta en el árbol del
  plan, pero conviene recorrer matriz, asientos y reportes en oscuro antes de producción.
- **`exceljs` arrastra una vulnerabilidad moderada por `uuid`**, transitiva. Solo corre en el
  navegador al exportar a Excel. Evaluar al actualizar dependencias.
- Hasta la fase 7 el `ng build` de producción nunca se había corrido —solo `ng serve`, que no aplica
  los presupuestos de tamaño— y el bundle ya estaba por encima del máximo por defecto. Los límites
  se ajustaron a 1,5 MB de aviso y 2 MB de error, con el bundle actual en 1,32 MB (242 kB
  transferidos). Tesorería los tiene en 5 MB/6 MB, que en la práctica no detectan nada; acá se
  dejaron ajustados a propósito para que una regresión de tamaño se note.
