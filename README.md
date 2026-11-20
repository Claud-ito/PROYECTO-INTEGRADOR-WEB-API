# UNIVALLE Emergencias — Web API

Catálogo de ubicaciones: un módulo contiene varios pisos. ASP.NET Core Web API, .NET 10 y SQL Server.

## Equipo y estado

| Persona | Cuenta | Responsabilidad |
| --- | --- | --- |
| Diana Valdivia | dianavaldivia001 | Modelo, SQL Server, contexto, migraciones y carga inicial |
| Alejandro | Por completar | DTO, CRUD y pruebas de Pisos |
| Said | Por completar | DTO, CRUD y pruebas de Módulos |

Repositorio: https://github.com/Claud-ito/PROYECTO-INTEGRADOR-WEB-API

Esta rama entrega la infraestructura de Diana. Los controladores, DTO y Swagger UI todavía deben integrarse. OpenAPI está disponible en Development en http://localhost:5077/openapi/v1.json; inicialmente no muestra operaciones porque no existen controladores. No confundir OpenAPI con Swagger UI.

El reparto V2 recibido contempla cuatro integrantes; el equipo confirmó tres. Queda pendiente asignar las consultas relacionadas, Swagger UI, el PDF y el ZIP final.

## Inicio en Windows

Requisitos: SDK .NET 10, SQL Server LocalDB o una instancia SQL Server accesible y acceso a NuGet para la primera restauración.

Desde PowerShell, en la carpeta que contiene este README:

```powershell
.\Iniciar-Diana.ps1
```

El script restaura herramientas y paquetes, compila, aplica las migraciones incluidas, solicita la carga inicial controlada y arranca la API. Se detiene si falla un paso. No genera migraciones nuevas ni elimina la base. Si PowerShell bloquea scripts, utilizar los comandos individuales de [la guía](docs/Configuracion_SQLServer.md).

La conexión de ejemplo está en `PRE-EMERGENCIAS-WEB-API/appsettings.json`; usa `(localdb)\\MSSQLLocalDB` y la base `UnivalleEmergenciasApi_Pruebas`, sin contraseña. Para otro servidor, ver la guía.

## Versiones

- Proyecto: net10.0.
- EF Core SQL Server y Design: 10.0.13.
- Herramienta local dotnet-ef: 10.0.13.
- Microsoft.AspNetCore.OpenApi: 10.0.11. Se ajustó la referencia inicial 10.0.15 a la versión disponible para verificar esta entrega; se mantiene .NET 10.
- SDK utilizado para verificar: 10.0.400.

## Modelo y reglas

`Models/Modulo.cs`: ModuloId, Codigo (20), Nombre (100), Activo y Pisos.

`Models/Piso.cs`: PisoId, ModuloId, Denominacion (50), Activo y Modulo.

Identificadores autogenerados; FK obligatoria; código único; denominación única dentro de cada módulo, sin distinguir mayúsculas. No hay borrado en cascada. Activo inicia en true y permite guardar false. Se recortan los textos y se guarda Codigo en mayúsculas. SQL también rechaza cadenas vacías o compuestas solo por espacios ordinarios.

Los controladores pendientes deben validar DTO y reglas de negocio para devolver 400/404: longitudes, identificadores positivos, duplicados, referencias inexistentes, módulos inactivos y DELETE con dependencias. Las restricciones SQL complementan estas validaciones; por sí solas no producen respuestas HTTP 400.

## Datos de ejemplo

Carga explícita con `dotnet run --project PRE-EMERGENCIAS-WEB-API -- --seed`, después de aplicar migraciones. Crea M01 y M02 activos con dos pisos cada uno y M03 inactivo. Son ejemplos ficticios.

Si ya hay cualquier módulo, no agrega ni modifica ubicaciones. La carga es transaccional y no se ejecuta al iniciar normalmente. Para repetir la prueba de persistencia iniciar sin `--seed`.

## Verificación

```powershell
dotnet run --project tests/Datos.Verificacion
```

Comprueba el modelo, índices, longitudes, FK, SQL generado, snapshot, normalización y Activo. Intercepta el guardado: no conecta a SQL Server ni demuestra persistencia real.

El estado de las pruebas ejecutadas y las pendientes está en [docs/Validacion_Diana.md](docs/Validacion_Diana.md). La instalación sobre SQL Server nuevo y P21 por API siguen pendientes mientras LocalDB no esté accesible y no se integren los controladores.
