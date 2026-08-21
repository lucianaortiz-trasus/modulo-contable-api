@echo off
setlocal enabledelayedexpansion

REM ============================================================================
REM  Compila y publica la API del modulo contable lista para desplegar.
REM
REM  Deja el resultado en dist\api. Antes de publicar corre la suite de tests:
REM  si algo esta en rojo, no se genera el paquete. Un modulo contable que
REM  compila pero tiene una regla rota no sirve de nada.
REM
REM  Uso:
REM     build-api.cmd
REM     build-api.cmd C:\publicaciones\contable-api
REM     build-api.cmd - --sin-tests        (el guion deja la salida por defecto)
REM
REM  Esta en .cmd y no en .ps1 a proposito: la directiva de grupo de las
REM  maquinas de la organizacion esta en AllSigned, asi que un .ps1 sin firmar
REM  no corre ni con -ExecutionPolicy Bypass. Los .cmd no dependen de eso.
REM ============================================================================

set "RAIZ=%~dp0"
if "%RAIZ:~-1%"=="\" set "RAIZ=%RAIZ:~0,-1%"

set "SOLUCION=%RAIZ%\Contable.sln"
set "PROYECTO=%RAIZ%\Contable.API\Contable.API.csproj"

set "SALIDA=%~1"
if "%SALIDA%"=="" set "SALIDA=%RAIZ%\dist"
if "%SALIDA%"=="-" set "SALIDA=%RAIZ%\dist"

set "CORRER_TESTS=1"
if /i "%~2"=="--sin-tests" set "CORRER_TESTS=0"
if /i "%~1"=="--sin-tests" set "CORRER_TESTS=0" & set "SALIDA=%RAIZ%\dist"

echo.
echo === API del Modulo Contable SIGEMyT ===
echo Solucion : %SOLUCION%
echo Salida   : %SALIDA%
echo.

if not exist "%SOLUCION%" (
    echo ERROR: no se encontro la solucion en %SOLUCION%
    exit /b 1
)

echo [1/4] Restaurando paquetes...
call dotnet restore "%SOLUCION%"
if errorlevel 1 goto :fallo_restore

echo.
echo [2/4] Compilando en Release...
call dotnet build "%SOLUCION%" --configuration Release --no-restore
if errorlevel 1 goto :fallo_build

echo.
if "%CORRER_TESTS%"=="0" (
    echo [3/4] Tests OMITIDOS por parametro.
    echo       El paquete no esta verificado: no lo subas a produccion asi.
) else (
    echo [3/4] Corriendo la suite de tests...
    call dotnet test "%SOLUCION%" --configuration Release --no-build --verbosity quiet
    REM !errorlevel! y no %errorlevel%: dentro de un bloque entre parentesis,
    REM %errorlevel% se resuelve al parsear el bloque y siempre da el valor
    REM anterior. Con %errorlevel% aca, un test en rojo se publicaba igual.
    if !errorlevel! neq 0 goto :fallo_tests
)

echo.
echo [4/4] Publicando...

REM Se borra la carpeta antes de publicar: si no, quedan DLLs de una version
REM anterior conviviendo con las nuevas y el proceso levanta una mezcla.
if exist "%SALIDA%" rmdir /s /q "%SALIDA%"

call dotnet publish "%PROYECTO%" --configuration Release --no-build --output "%SALIDA%"
if errorlevel 1 goto :fallo_publish

REM appsettings.Development.json tiene las cadenas de conexion del stage.
REM No va al paquete.
if exist "%SALIDA%\appsettings.Development.json" (
    del /q "%SALIDA%\appsettings.Development.json"
    echo       Se quito appsettings.Development.json del paquete ^(tiene credenciales^).
)

echo.
echo LISTO.
echo   Carpeta : %SALIDA%
echo.
echo Antes de desplegar, revisa en el servidor:
echo   - ConnectionStrings ^(Ejecucion, Presupuesto, Portal^)
echo   - Jwt:SecretKey, Issuer y Audience
echo   - Uses:UseAuth en true y Uses:UseSwagger en false
echo   - Cors:AllowedOrigins con la URL real del front
echo   - Contabilidad: CuentaBalanceoCierre y UnidadAdministrativaCierre son de MORTEROS
echo     ^(10176 y 1614^). Para otro municipio hay que volver a averiguarlos.
echo.
exit /b 0

:fallo_restore
echo.
echo ERROR: fallo el restore de paquetes.
exit /b 1

:fallo_build
echo.
echo ERROR: fallo la compilacion.
exit /b 1

:fallo_tests
echo.
echo ERROR: hay tests en rojo. No se publica.
exit /b 1

:fallo_publish
echo.
echo ERROR: fallo el publish.
exit /b 1
