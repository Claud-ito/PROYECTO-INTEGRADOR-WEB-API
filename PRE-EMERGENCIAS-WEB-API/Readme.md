# UNIVALLE Emergencias — Web API

Proyecto integrador de Programación Web III desarrollado con **ASP.NET Core Web API (.NET 10), Entity Framework Core y SQL Server**.

El sistema permite administrar un catálogo de ubicaciones de emergencia de UNIVALLE, organizado mediante dos entidades relacionadas: **Módulos y Pisos**.

Un módulo puede contener varios pisos y cada piso pertenece a un único módulo.

## 1. Integrantes del equipo

| Integrante | Nombre | GitHub | Responsabilidad |
|---|---|---|---|
| 1 | Diana Valdivia Vaca Diez | Dianavaldivia001 | Base de datos, entidades, DbContext, migraciones y carga inicial |
| 2 | Alejandro Rojas Espinoza | alejandrorojas03010-estrella | CRUD de Pisos, DTO, validaciones y pruebas |
| 3 | Said Montaño Condori | Claud-ito | CRUD de Módulos, DTO, validaciones y pruebas |
| 4 | Jorge Daniel Gutierrez Espinosa | jorgegut28 | Consultas relacionadas, Swagger UI, documentación e integración final |

**Repositorio:** https://github.com/Claud-ito/PROYECTO-INTEGRADOR-WEB-API

## 2. Tecnologías utilizadas

- C# y ASP.NET Core Web API (.NET 10).
- Entity Framework Core 10.
- SQL Server / SQL Server LocalDB.
- Microsoft.AspNetCore.OpenApi.
- Swagger UI mediante Swashbuckle.AspNetCore.SwaggerUI.
- Visual Studio.
- Git y GitHub.
- Archivos `.http` para pruebas de solicitudes.

## 3. Modelo de datos

### Entidad Modulo

| Campo | Descripción |
|---|---|
| ModuloId | Identificador único autogenerado |
| Codigo | Código único, máximo 20 caracteres |
| Nombre | Nombre del módulo, máximo 100 caracteres |
| Activo | Estado lógico del módulo |
| Pisos | Colección de pisos relacionados |

### Entidad Piso

| Campo | Descripción |
|---|---|
| PisoId | Identificador único autogenerado |
| ModuloId | Clave foránea hacia Modulo |
| Denominacion | Nombre del piso, máximo 50 caracteres |
| Activo | Estado lógico del piso |
| Modulo | Módulo al que pertenece |

### Reglas de negocio

- Cada piso debe pertenecer a un módulo existente.
- El código de un módulo debe ser único.
- No pueden existir dos pisos con la misma denominación dentro de un mismo módulo.
- Se utilizan validaciones para impedir registros con datos inválidos.
- Los códigos de módulos se normalizan a mayúsculas.
- Los textos se almacenan sin espacios sobrantes en los extremos.
- Los nuevos pisos deben pertenecer a módulos activos.
- La propiedad `Activo` permite realizar bajas lógicas.
- No se permite eliminar físicamente un módulo que tenga pisos asociados.
- La relación entre Módulos y Pisos está configurada sin eliminación en cascada.

## 4. Endpoints de la API

### CRUD de Pisos — Integrante 2

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/pisos` | Listar pisos |
| GET | `/api/pisos/{id}` | Obtener piso por ID |
| POST | `/api/pisos` | Registrar piso |
| PUT | `/api/pisos/{id}` | Actualizar piso |
| DELETE | `/api/pisos/{id}` | Eliminar piso |

El listado permite utilizar `?soloActivos=true` para obtener únicamente los pisos activos.

### CRUD de Módulos — Integrante 3

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/modulos` | Listar módulos |
| GET | `/api/modulos/{id}` | Obtener módulo por ID |
| POST | `/api/modulos` | Registrar módulo |
| PUT | `/api/modulos/{id}` | Actualizar módulo |
| DELETE | `/api/modulos/{id}` | Eliminar módulo |

El listado también permite utilizar `?soloActivos=true`.

### Consultas relacionadas — Integrante 4

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/pisos/{id}/modulo` | Obtener el módulo al que pertenece un piso |
| GET | `/api/modulos/{id}/pisos` | Obtener los pisos asociados a un módulo |

**GET /api/pisos/{id}/modulo**

Devuelve un `ModuloRespuestaDto` con los campos `moduloId`, `codigo`, `nombre` y `activo`.

Respuestas esperadas:

- `200 OK`: el piso existe y se devuelve su módulo.
- `400 Bad Request`: identificador no positivo.
- `404 Not Found`: el piso no existe.

**GET /api/modulos/{id}/pisos**

Devuelve una lista de `PisoRespuestaDto`, con información del piso y de su módulo.

Respuestas esperadas:

- `200 OK`: módulo existente con sus pisos.
- `200 OK` con `[]`: módulo existente sin pisos.
- `400 Bad Request`: identificador no positivo o filtro booleano inválido.
- `404 Not Found`: módulo inexistente.

Admite el filtro opcional `?soloActivos=true` para devolver únicamente los pisos activos.

Las consultas utilizan Entity Framework Core, operaciones asíncronas y `AsNoTracking()`.

## 5. Instalación y configuración

### Requisitos

- SDK de .NET 10.
- Visual Studio compatible con .NET 10.
- SQL Server o SQL Server LocalDB.
- Acceso a NuGet para restaurar los paquetes.

### Clonar el repositorio

```bash
git clone https://github.com/Claud-ito/PROYECTO-INTEGRADOR-WEB-API.git
cd PROYECTO-INTEGRADOR-WEB-API
```

Abrir la solución del proyecto en Visual Studio.

### Configurar la base de datos

La cadena de conexión se encuentra en:

`PRE-EMERGENCIAS-WEB-API/appsettings.json`

Configuración de ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=UnivalleEmergenciasApi_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=10"
  }
}
```

Modificar la conexión si se utiliza otra instancia de SQL Server.

### Aplicar migraciones

Desde la raíz del repositorio:

```powershell
dotnet tool restore
dotnet ef database update --project PRE-EMERGENCIAS-WEB-API
```

No es necesario generar una nueva migración si se utiliza el modelo existente sin cambios.

### Cargar datos de ejemplo

```powershell
dotnet run --project PRE-EMERGENCIAS-WEB-API -- --seed
```

La carga inicial crea módulos y pisos de ejemplo. Si ya existen módulos, el proceso no vuelve a insertar los datos iniciales.

También se dispone del script `Iniciar-Diana.ps1` y de la guía `docs/Configuracion_SQLServer.md` para preparar el entorno Windows.

## 6. Ejecución de la API

Desde la raíz del repositorio:

```powershell
dotnet run --project PRE-EMERGENCIAS-WEB-API --launch-profile https
```

Direcciones configuradas para el entorno de desarrollo:

- Swagger UI: https://localhost:7298/swagger/index.html
- OpenAPI JSON: https://localhost:7298/openapi/v1.json
- API HTTP: http://localhost:5077

Se recomienda utilizar HTTPS para acceder a Swagger UI y evitar problemas de redirección entre HTTP y HTTPS.

Los puertos pueden cambiar según la configuración local.

## 7. Swagger UI

Swagger UI permite visualizar la documentación de la API y ejecutar solicitudes directamente desde el navegador.

Se configura en `Program.cs` y utiliza el documento generado por `Microsoft.AspNetCore.OpenApi`.

La interfaz muestra las operaciones de Pisos, Módulos y las consultas relacionadas una vez integradas.

Swagger UI está habilitado en el entorno `Development`.

## 8. Pruebas y validación

Los archivos de pruebas se encuentran en `docs/pruebas/`.

Entre ellos:

- `Pisos.http`: solicitudes del CRUD de Pisos.
- `Modulos.http`: solicitudes del CRUD de Módulos.
- `Relaciones.http`: solicitudes de las consultas relacionadas.

Las pruebas de relaciones contemplan los siguientes escenarios:

| Código | Prueba | Resultado esperado |
|---|---|---|
| P13 | Obtener módulo de un piso existente | 200 |
| P13b | Piso inexistente | 404 |
| P13c | ID de piso inválido | 400 |
| P14 | Obtener pisos de módulo existente | 200 |
| P15 | Módulo sin pisos | 200 y `[]` |
| P16 | Módulo inexistente | 404 |
| P16b | ID de módulo inválido | 400 |
| Extra | Filtrar pisos activos | 200 |

### Estado de verificación

- Infraestructura y modelo: existen comprobaciones documentadas en `docs/Validacion_Diana.md`.
- CRUD de Pisos: el integrante 2 documentó 31 solicitudes con los códigos HTTP esperados, incluida una prueba de persistencia tras reiniciar.
- CRUD de Módulos: dispone de pruebas y evidencias documentadas por el integrante 3.
- Swagger UI: se comprobó que abre y muestra los endpoints en el entorno de desarrollo.
- Consultas relacionadas: código implementado y pruebas HTTP preparadas; queda pendiente ejecutar los casos contra una base de datos disponible y guardar las evidencias finales.

No se consideran aprobadas las pruebas relacionadas mientras no se hayan ejecutado realmente.

## 9. Evidencias

Las evidencias se organizan dentro de `docs/evidencias/`.

Las carpetas `relaciones` y `swagger` están destinadas a guardar capturas de:

- Interfaz Swagger UI con los endpoints.
- Respuesta exitosa de módulo asociado a un piso.
- Listado de pisos de un módulo.
- Respuesta vacía de un módulo sin pisos.
- Respuestas de error `404 Not Found`.

Estas evidencias se incorporarán al informe PDF final una vez completadas las pruebas.

## 10. Entrega final

La entrega del proyecto contempla:

- Código fuente completo.
- Entidades, DTO y controladores.
- Configuración de SQL Server y migraciones.
- Swagger UI y documentación OpenAPI.
- Archivos de pruebas HTTP.
- README actualizado.
- Evidencias de ejecución.
- Informe PDF con capturas.
- Archivo ZIP del proyecto final.

## 11. Repositorio

Código fuente disponible en:

https://github.com/Claud-ito/PROYECTO-INTEGRADOR-WEB-API

El trabajo se desarrolla mediante ramas Git por integrante, integrando los cambios en la rama principal después de su revisión.