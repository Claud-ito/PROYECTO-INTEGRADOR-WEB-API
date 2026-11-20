# Configuración SQL Server — Diana

## 1. Abrir el proyecto

En Visual Studio abrir `PRE-EMERGENCIAS-WEB-API.slnx`. El proyecto web está dentro de `PRE-EMERGENCIAS-WEB-API/`. No utiliza MAUI.

## 2. Verificar requisitos

```powershell
dotnet --list-sdks
SqlLocalDB info
SqlLocalDB start MSSQLLocalDB
```

Si la instancia no existe, crearla una sola vez con `SqlLocalDB create MSSQLLocalDB` y volver a iniciarla. Si LocalDB devuelve un error de acceso/configuración, ejecutar estas instrucciones desde una terminal normal del usuario o configurar una instancia SQL Server disponible. No borrar ni reinstalar bases para resolverlo.

## 3. Conexión

La clave que lee Program.cs es `ConnectionStrings:DefaultConnection`.

Conexión de ejemplo incluida:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\\\MSSQLLocalDB;Database=UnivalleEmergenciasApi_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=10"
  }
}
```

Para una instancia diferente se puede configurar una variable en la terminal antes de arrancar:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.\SQLEXPRESS;Database=UnivalleEmergenciasApi_Pruebas;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=10'
```

En PowerShell la barra de la instancia es una sola. JSON utiliza dos. No subir contraseñas ni conexiones personales al repositorio. Para SQL Server remoto usar la conexión proporcionada por su administrador; LocalDB requiere Windows.

## 4. Restaurar y aplicar migraciones

Ejecutar desde la raíz del repositorio:

```powershell
dotnet tool restore
dotnet restore PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj
dotnet build PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj --no-restore
dotnet ef database update --project PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj
dotnet run --project PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj -- --seed
dotnet run --project PRE-EMERGENCIAS-WEB-API/PRE-EMERGENCIAS-WEB-API.csproj --launch-profile http
```

Detenerse si un comando falla. No ejecutar migrations add para instalar una copia: la migración, diseñador y snapshot ya están incluidos.

La URL de desarrollo real es http://localhost:5077. El documento OpenAPI está en http://localhost:5077/openapi/v1.json. La raíz puede devolver 404 hasta integrar los controladores; Swagger UI es trabajo pendiente del grupo.

## 5. Datos y cambios del modelo

La carga explícita crea dos módulos activos y uno inactivo con cuatro pisos. Si existen módulos, conserva todos los datos y no completa registros faltantes. Esto impide reponer ubicaciones borradas o reescribir modificaciones durante las pruebas.

El inicio normal no ejecuta seed, EnsureDeleted, EnsureCreated ni migraciones automáticas. Las nuevas migraciones se coordinan con Diana para evitar snapshots incompatibles.

## 6. Evidencias pendientes

P22: usar una base de pruebas con nombre nuevo cambiando únicamente Database en la conexión; aplicar las migraciones y capturar el comando exitoso y las tablas.

P21, al integrar el CRUD y Swagger UI:
1. Crear por POST un piso llamado Persistencia-Diana con el ID real de un módulo activo.
2. Capturar 201, Location y el identificador.
3. Consultarlo por ID y capturar 200.
4. Detener completamente la API con Ctrl+C.
5. Ejecutar únicamente el último comando de arranque, sin seed, con la misma conexión.
6. Consultar el mismo ID y capturar 200 con los mismos datos.

Estas evidencias deben ejecutarse realmente. La comprobación del modelo sin base no las sustituye.

## Referencias

- https://learn.microsoft.com/en-us/ef/core/providers/sql-server/
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
