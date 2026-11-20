# Operaciones de Pisos — aporte de Alejandro para el README

Recurso principal de la API. Cada piso pertenece a un módulo. Controlador: `PRE-EMERGENCIAS-WEB-API/Controllers/PisosController.cs`. DTO: `PRE-EMERGENCIAS-WEB-API/Dtos/Pisos/`.

## Endpoints

| Método y ruta | Éxito | Errores |
| --- | --- | --- |
| `GET /api/pisos` | 200 y lista de `PisoRespuestaDto` | 400 si `soloActivos` no es `true`/`false` |
| `GET /api/pisos?soloActivos=true` | 200 y solo pisos con `Activo=true` | 400 si el filtro no es válido |
| `GET /api/pisos/{id}` | 200 y `PisoRespuestaDto` | 400 si `id` ≤ 0; 404 si no existe |
| `POST /api/pisos` | 201, `Location: /api/pisos/{id}` y `PisoRespuestaDto` | 400 por validación, módulo inexistente o inactivo, o duplicado |
| `PUT /api/pisos/{id}` | 204 sin cuerpo | 400 por validación, módulo inexistente, traslado a módulo inactivo o duplicado; 404 si no existe |
| `DELETE /api/pisos/{id}` | 204 sin cuerpo | 400 si otro registro lo referencia; 404 si no existe |

El listado incluye por defecto los pisos inactivos, para poder comprobar una baja lógica. Se ordena por módulo y denominación.

## Cuerpos

`POST /api/pisos` (`CrearPisoDto`):

```json
{ "moduloId": 1, "denominacion": "Piso 2" }
```

`PUT /api/pisos/{id}` (`ActualizarPisoDto`, todos los campos son obligatorios):

```json
{ "moduloId": 1, "denominacion": "Segundo piso", "activo": true }
```

Respuesta (`PisoRespuestaDto`):

```json
{ "pisoId": 5, "moduloId": 1, "moduloCodigo": "M01", "moduloNombre": "Modulo 1 (ejemplo)", "denominacion": "Piso 2", "activo": true }
```

## Reglas aplicadas

- `ModuloId` debe ser positivo y corresponder a un módulo existente. Si no existe, la respuesta es 400 con "El módulo indicado no existe".
- No se crean pisos en un módulo inactivo, ni se trasladan a uno. Un piso que ya está en un módulo inactivo sí puede editarse (por ejemplo, para darlo de baja).
- `Denominacion` es obligatoria, admite como máximo 50 caracteres y se guarda sin espacios al inicio ni al final. No puede repetirse dentro del mismo módulo, sin distinguir mayúsculas ("piso 1" = "Piso 1"). En otro módulo sí se permite.
- En PUT, la búsqueda de duplicados excluye el propio piso.
- Si dos solicitudes simultáneas intentan guardar el mismo duplicado, el índice único de SQL Server lo impide y la API responde 400, no 500.
- Los errores usan `ValidationProblemDetails` (400) o `ProblemDetails` (404), con el detalle en español.

## Baja lógica y eliminación física

- **Baja lógica:** `PUT` con `"activo": false`. El registro se conserva, `GET /api/pisos/{id}` lo muestra con `activo: false` y `?soloActivos=true` lo omite.
- **Eliminación física:** `DELETE` borra un piso sin referencias y devuelve 204; un GET posterior devuelve 404. Si en el futuro otra tabla referencia pisos, DELETE devuelve 400 y se debe usar la baja lógica.

## Pruebas

- Solicitudes listas para Visual Studio o Rider: `docs/pruebas/Pisos.http` (P02–P12, P19, P20 y P21).
- Resultados de la ejecución contra SQL Server: `docs/evidencias/pisos/Resultados_pruebas_Pisos.md`.

## SQL Server en macOS (sin LocalDB)

LocalDB solo existe en Windows. En macOS se puede usar SQL Server en Docker y pasar la conexión por variable de entorno, sin modificar `appsettings.json`:

```bash
docker run -d --name sql-univalle --platform linux/amd64 -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=<CONTRASEÑA_LOCAL>" -p 14333:1433 mcr.microsoft.com/mssql/server:2022-latest
export ConnectionStrings__DefaultConnection="Server=localhost,14333;Database=UnivalleEmergenciasApi_Pruebas;User Id=sa;Password=<CONTRASEÑA_LOCAL>;TrustServerCertificate=True"
dotnet tool restore
dotnet ef database update --project PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj
dotnet run --project PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj -- --seed
dotnet run --project PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj --launch-profile http
```

La contraseña es solo local; no se sube al repositorio.
