# Despliegue del Modulo Contable SIGEMyT

Son dos piezas independientes: la **API** (.NET 8) y el **front** (Angular 18, archivos estaticos).
Se compilan y se publican por separado, con un script cada una.

Cada una vive en su propio repositorio:

| Pieza | Repositorio | Script | Salida |
|---|---|---|---|
| API | `efinten/modulo-contable-api` | `build-api.cmd` | `dist/` |
| Front | `efinten/modulo-contable-web` | `build-web.cmd` | `publicar/` |

Este documento, el plan, el analisis del PL/SQL y los scripts de datos de prueba viven en el
repositorio de la **API**: son transversales al modulo y no tenia sentido duplicarlos.

---

## 1. Compilar

Hay un script por pieza, cada uno en la raiz de su repositorio. Se pueden correr en cualquier
orden: no dependen entre si.

> **Por que son `.cmd` y no `.ps1`.** La directiva de grupo de las maquinas de la organizacion
> tiene `MachinePolicy = AllSigned`, asi que un `.ps1` sin firmar no corre **ni siquiera** con
> `-ExecutionPolicy Bypass` (la politica de maquina gana sobre todas las demas). Los `.cmd` no
> dependen de la politica de ejecucion de PowerShell. Si en el futuro se firman los scripts, pasar
> a `.ps1` es trivial.

### API

```bat
build-api.cmd
```

Deja el resultado en `dist\`. El script:

1. Restaura los paquetes NuGet.
2. Compila en `Release`.
3. **Corre la suite de tests.** Si algo esta en rojo, aborta con codigo 1 y no genera el paquete.
4. Publica.
5. **Borra `appsettings.Development.json` del paquete**, porque tiene las cadenas de conexion del
   entorno de stage.

```bat
build-api.cmd C:\publicaciones\contable-api      REM otra carpeta destino
build-api.cmd - --sin-tests                      REM salta la suite (solo para iterar)
```

El guion suelto significa "dejá la salida por defecto". `--sin-tests` **no se usa para un paquete
que se despliega**: el script avisa que el resultado no esta verificado.

### Front

```bat
build-web.cmd
```

Deja el resultado en `publicar\`. Usa `npm ci` (no `npm install`) para instalar exactamente lo que
esta en `package-lock.json`, que es lo que se probo.

```bat
build-web.cmd C:\inetpub\contable                REM otra carpeta destino
build-web.cmd - --sin-instalar                   REM salta npm ci
```

> Si `npm ci` falla con `EPERM ... esbuild.exe`, hay un `ng serve` corriendo que tiene tomado el
> ejecutable. Cerralo y volve a correr.

### Que sale de cada uno

| | Archivos | Tamano | Contenido |
|---|---|---|---|
| `dist\` (repo de la API) | 60 | ~18 MB | DLLs, `Contable.API.exe`, `appsettings.json` |
| `publicar\` (repo del front) | 14 | ~3,2 MB | `index.html`, JS, CSS, favicon |

---

## 2. Configurar la API

En el servidor, `appsettings.json` (o variables de entorno) tiene que tener:

```jsonc
{
  "ConnectionStrings": {
    "Ejecucion":    "user id=EJECUCION;password=...;data source=...",
    "Presupuesto":  "user id=PRESUPUESTO;password=...;data source=...",
    "Portal":       "user id=PORTAL;password=...;data source=..."
  },
  "Jwt": {
    "SecretKey": "<distinta por ambiente, fuera del repositorio>",
    "Issuer":    "https://<host>/",
    "Audience":  "https://<host>/",
    "Minutes":   1440
  },
  "Uses": {
    "UseJwt":     true,
    "UseAuth":    true,     // en produccion SIEMPRE true
    "UseSwagger": false     // en produccion conviene false
  },
  "Cors": {
    "AllowedOrigins": [ "https://<host-del-front>" ]
  },
  "Portal": {
    // OBLIGATORIO: sin esto nadie puede entrar al modulo.
    "AuthUrl": "https://<host-del-portal>/api/Seguridad/Login",
    "TimeoutSegundos": 15
  },
  "Contabilidad": {
    "CuentaBalanceoCierre": 10176,      // 03.01.04.01 "Resultado del ejercicio"
    "UnidadAdministrativaCierre": 1614  // numero 1, "Trasus", jurisdiccion 2437
  }
}
```

### Lo que no puede quedar como esta

| Clave | Estado | Por que importa |
|---|---|---|
| `Uses:UseAuth` | `false` en desarrollo | Con `false`, **todos los endpoints quedan abiertos**. En produccion va en `true` |
| `Uses:UseSwagger` | `true` | Publica el mapa completo de la API. Conviene apagarlo |
| `Jwt:SecretKey` | Valor de desarrollo | Con la clave conocida cualquiera se emite un token valido |
| `Cors:AllowedOrigins` | `localhost:4400` | Tiene que ser la URL real del front |
| `Portal:AuthUrl` | **vacio** | **Sin esto nadie entra al modulo.** Ver la seccion de federacion |
| `Contabilidad:CuentaBalanceoCierre` | `10176` | Ya confirmada. **Es especifica de Morteros**: revisarla para otro municipio |
| `Contabilidad:UnidadAdministrativaCierre` | `1614` | Idem. Ver la advertencia de abajo |

### La federacion con el Portal (Portal:AuthUrl)

Es lo primero que hay que dejar andando: **sin esto la aplicacion no deja entrar a nadie.**

El Portal SIGEMyT manda al usuario al modulo con el token en el querystring, y el modulo se lo
valida al Portal. Es el mismo contrato que usan los otros once modulos de la plataforma:

```
1. El usuario entra al Portal      POST api/Seguridad/ValidarUsuario  -> { token, usuario }
2. Clickea el modulo               .../nuevo-contable/?token=...&idJur=...&numJur=...
3. El front llama a su backend     GET  .../nuevo-contable-api/api/Seguridad/Login?atoken=...
4. Ese backend llama al Portal     GET  {Portal:AuthUrl}?atoken=...
5. El Portal responde              200 { token, usuario { ... } }  |  401
```

Para el modulo el token es **opaco**: lo reenvia tal cual y no lo desencripta nunca. Por eso el
Portal puede cambiar el formato interno sin tocar nada de aca.

`Portal:AuthUrl` es la URL completa de `api/Seguridad/Login` del Portal, **sin** el querystring.

> El servidor donde corre la API tiene que poder **salir** hacia el Portal. Es una llamada de
> servidor a servidor, no del navegador: si hay un firewall de salida, hay que habilitarla.

**Como se diagnostica si falla.** El modulo distingue los tres casos y lo dice en pantalla, en vez
de rebotar al portal —que era lo que generaba un parpadeo eterno sin ningun mensaje—:

| Lo que ve el usuario | Que pasa |
|---|---|
| "El modulo no tiene configurada la direccion del portal" | Falta `Portal:AuthUrl` |
| "El portal no reconocio la sesion" | El Portal devolvio 401, o la API no llega al Portal. Mirar el log de la API |
| "El usuario no existe en el portal" | El Portal valido bien, pero ese usuario no esta en el esquema `PORTAL` de esta base |

### Los dos parametros del cierre

Quedaron resueltos el 2026-08-21 y verificados contra la base de Morteros. **Son especificos de ese
municipio**: para cualquier otro hay que volver a averiguarlos, y no alcanza con copiarlos.

| Clave | Valor | Que es |
|---|---|---|
| `CuentaBalanceoCierre` | `10176` | `03.01.04.01` "Resultado del ejercicio". Activa, imputable, sin hijas, de patrimonio neto |
| `UnidadAdministrativaCierre` | `1614` | Numero 1, "Trasus", jurisdiccion 2437 |

> **Cuidado con la unidad administrativa.** La eleccion obvia es la que se llama "Municipalidad De
> Morteros" (`1619`), y es la equivocada: pertenece a la jurisdiccion **2442**, donde no hay
> ninguna cuenta contable ni ningun asiento. Toda la contabilidad esta en las jurisdicciones 2437 y
> 4000. Con `1619` el asiento de cierre se genera igual y cuadra, pero queda sellado con una
> jurisdiccion que no posee ninguna de las cuentas que el mismo asiento cancela.
>
> El criterio correcto no es el nombre sino el **numero**: es lo que el modulo traduce entre
> ejercicios. La UA de numero 1 resuelve a 2437 en 2026 y a 4000 en 2025, que es donde estan los
> asientos de cada uno. La de numero 2 solo existe en 2026, asi que ademas no sirve para cerrar un
> ejercicio anterior.

Con cualquiera de los dos en `0`, el modulo funciona entero salvo el cierre y la apertura, que
avisan que falta configurar en vez de generar un asiento contra una cuenta equivocada.

**Como verificar el valor en un municipio nuevo**, sin escribir nada: entrar a **Asientos** y
apretar **Cierre**. La vista previa muestra los asientos que se generarian; si los totales cuadran
y el balanceo cae en la cuenta de resultado esperada, la configuracion esta bien. Recien ahi
confirmar.

---

## 3. Publicar el front

### Las URLs

| Pieza | URL |
|---|---|
| Front | `https://www.administracionho.morteros.gob.ar/nuevo-contable` |
| API | `https://www.administracionho.morteros.gob.ar/nuevo-contable-api` |

Las dos estan en el **mismo origen**, asi que en la practica no hay CORS entre ellas. La entrada de
`Cors:AllowedOrigins` igual queda cargada, porque el navegador si hace preflight cuando el pedido
lleva el header `Authorization`.

El front se sirve desde un **subdirectorio**, no desde la raiz del dominio. Eso tiene dos
consecuencias que hay que respetar o la aplicacion no carga.

### a) El `base href`

Angular necesita saber que cuelga de `/nuevo-contable/`. Ya esta puesto en `angular.json`, en la
configuracion de produccion:

```json
"production": {
  "baseHref": "/nuevo-contable/"
}
```

El `index.html` publicado tiene que decir `<base href="/nuevo-contable/">` y todas las referencias
a los `.js` y `.css` tienen que ser **relativas**, sin barra inicial. Se comprueba en un segundo:

```bat
findstr /C:"<base href" publicar\index.html
```

> Si el front se mueve a otra ruta, hay que cambiar `baseHref` y **recompilar**. Es lo unico que no
> se resuelve en tiempo de ejecucion.

### b) `index.html` para cualquier ruta que no sea un archivo

La aplicacion usa rutas de Angular. Si alguien entra directo a `/nuevo-contable/reportes`, el
servidor tiene que devolver `index.html` y dejar que el router resuelva. Sin esto todo funciona
mientras se navegue por el menu, y da 404 al recargar la pagina.

### La ruta de la API

El front la resuelve en tiempo de ejecucion, en `web/src/app/services/environment.ts`:

```ts
const RUTA_DE_LA_API = 'nuevo-contable-api';

if (window.location.hostname.includes('localhost')) {
  environment.urlService = 'http://localhost:5180/api/';
} else {
  environment.urlService = `${window.location.origin}/${RUTA_DE_LA_API}/api/`;
}
```

Los controllers cuelgan de `api/`, asi que la URL completa de un endpoint queda:

```
https://www.administracionho.morteros.gob.ar/nuevo-contable-api/api/Reportes/SumasYSaldos
```

### nginx

```nginx
# Front, servido desde un subdirectorio
location /nuevo-contable/ {
    alias /var/www/contable/;
    try_files $uri $uri/ /nuevo-contable/index.html;   # (b)
}

# API
location /nuevo-contable-api/ {
    proxy_pass http://127.0.0.1:5180/;
    proxy_set_header Host              $host;
    proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

> Dos detalles de nginx que suelen morder: con `alias` la ruta **termina en barra** en las dos
> partes, y el `try_files` de respaldo tiene que incluir el prefijo (`/nuevo-contable/index.html`),
> no `/index.html`. Y en `proxy_pass` la barra final es la que hace que `/nuevo-contable-api/api/X`
> le llegue a la API como `/api/X`; sin ella llega con el prefijo y da 404 en todos los endpoints.

### IIS

Con `URL Rewrite` + `Application Request Routing`:

- Una aplicacion en `/nuevo-contable` apuntando a la carpeta `publicar`, con una regla que
  reescriba a `index.html` todo lo que no sea archivo ni directorio.
- Una aplicacion o `rewrite` en `/nuevo-contable-api` que mande al puerto de la API.

---

## 4. Verificar despues de desplegar

En orden. Si uno falla, no tiene sentido seguir con el siguiente.

| # | Que | Como | Esperado |
|---|---|---|---|
| 1 | La API levanta | `GET https://www.administracionho.morteros.gob.ar/nuevo-contable-api/health` | `Healthy`, con los tres chequeos en verde |
| 2 | Llega a las tres bases | El mismo `/health` | Si alguno falla, revisar la cadena de conexion de ese esquema |
| 3 | El front carga | Abrir `.../nuevo-contable` | Se ve el menu lateral. Si la pagina queda en blanco, mirar la consola: casi siempre es el `base href` o el `try_files` |
| 4 | Las rutas internas resuelven | Entrar directo a `.../nuevo-contable/reportes` y recargar con F5 | Carga la pantalla. Un 404 aca es el punto (b) del servidor |
| 5 | El front llega a la API | Entrar a **Inicio** | Muestra ejercicio activo y cantidad de cuentas; si da error, revisar la barra final del `proxy_pass` y CORS |
| 6 | La federacion funciona | Entrar **desde el portal**, no escribiendo la URL a mano | El nombre del usuario aparece arriba a la derecha, y el `?token=` desaparece de la barra de direcciones |
| 7 | Los datos son los correctos | **Reportes -> Sumas y Saldos** | El balance cuadra, o explica por que no |
| 8 | La consistencia esta sana | **Auditoria** | Cero hallazgos de severidad *Error* |
| 9 | El cierre esta bien configurado | **Asientos -> Cierre** (solo la vista previa, **sin confirmar**) | Dos asientos cuadrados y el balanceo en `03.01.04.01` |

El paso 8 es el mas util de todos: si el despliegue quedo apuntando al esquema equivocado o a un
ejercicio que no corresponde, la auditoria lo muestra enseguida.

---

## 5. Datos de prueba

El entorno de stage tiene **400 asientos sinteticos** con la leyenda prefijada `[QA]`, en 2025 y
2026, generados para poder probar reportes y estadisticas con volumen.

| Script | Que hace |
|---|---|
| `_datos-de-prueba/generar.sql` | Genera los asientos `[QA]` |
| `_datos-de-prueba/verificar.sql` | Cuenta lo que hay, para confirmar antes de limpiar |
| `_datos-de-prueba/limpiar.sql` | Borra **solo** lo que tiene el prefijo `[QA]` |

> **Antes de pasar un ambiente a produccion, corre `limpiar.sql`.** Los asientos `[QA]` no deben
> quedar en una base real: son 400 asientos que suman en los reportes.
>
> Los scripts tocan unicamente `EJECUCION.T_ASIENTOS` y `EJECUCION.T_ASIENTOS_DET`, y solo filas con
> el prefijo. No tocan el plan de cuentas, la matriz, ni nada de `PRESUPUESTO`.

---

## 6. Volver atras

La API es autocontenida: para volver a la version anterior alcanza con reemplazar la carpeta
publicada y reiniciar el servicio. El front, lo mismo con los archivos estaticos.

**El modulo no corre migraciones ni modifica el esquema de la base**, asi que un rollback de
aplicacion no deja datos a medias. Lo unico que persiste son los asientos cargados, y eso es
justamente lo que tiene que persistir.
