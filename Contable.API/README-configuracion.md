# Configuracion

`appsettings.json` va sin secretos a proposito. Las cadenas de conexion y la clave del JWT se
resuelven por entorno:

- En desarrollo, desde `appsettings.Development.json`, que esta en el `.gitignore`.
- En homologacion y produccion, desde variables de entorno.

Las variables usan `__` como separador de nivel:

```
ConnectionStrings__Ejecucion
ConnectionStrings__Presupuesto
ConnectionStrings__Portal
Jwt__SecretKey
```

## Parametros contables

La seccion `Contabilidad` externaliza los valores que en el PL/SQL provincial estaban escritos a
mano y que cambian de un municipio a otro. Ver `_analisis/REGLAS.md` seccion 2.

`CuentaBalanceoCierre` y `UnidadAdministrativaCierre` arrancan en 0 porque todavia no estan
confirmados para Morteros. El cierre de ejercicio valida que esten cargados antes de correr: con
0 falla con un mensaje claro en vez de generar un asiento mal imputado.

Candidata propuesta para Morteros, a confirmar por el contador:
`10176` — `03.01.04.01` "Resultado del ejercicio".
