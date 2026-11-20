# Verificación del aporte de Diana

Fecha local: 19 de noviembre de 2026. Base: main, commit 78be53fbc538f28363ee8634c3f6124ab7d622e0.

## Ejecutado

- Restauración del proyecto y del verificador desde los paquetes disponibles en el equipo.
- Compilación correcta, sin advertencias ni errores.
- Migración InicialUbicaciones generada con EF Core 10.0.13, diseñador y snapshot incluidos.
- Generación de SQL Server idempotente sin conectar a la base.
- Once comprobaciones correctas en tests/Datos.Verificacion: longitudes, índices únicos, relación obligatoria sin cascada, coincidencia de snapshot, tablas y CHECK en SQL, normalización síncrona/asíncrona y estado Activo.
- Inicio de la API confirmado por el servidor en http://localhost:5077, entorno Development.

## Ejecución posterior de Diana, confirmada por sus capturas

- dotnet tool restore restauró correctamente dotnet-ef 10.0.13.
- SQL Server creó UnivalleEmergenciasApi_Pruebas y aplicó 20261120022734_InicialUbicaciones hasta finalizar con Done. Se observaron tablas Modulos y Pisos, FK e índices únicos: P22 confirmada.
- La carga inicial insertó módulos y pisos y finalizó correctamente.
- La API arrancó en http://localhost:5077, Development.
- NuGet informó dos advertencias sobre System.Security.Cryptography.Pkcs 9.0.11. Se agregó una referencia explícita a 10.0.13, versión corregida para GHSA-846f-f764-76mx según https://github.com/advisories/GHSA-846f-f764-76mx. Volver a restaurar para comprobar la auditoría con acceso a NuGet.

## Limitaciones iniciales y pendientes restantes

- El entorno no pudo acceder a la configuración de LocalDB. database update falló con error 50: Cannot create an automatic instance. No se acreditan tablas creadas, carga del seed ni persistencia real.
- La llamada HTTP al documento OpenAPI fue bloqueada por los permisos de sockets del entorno. El inicio del servidor sí fue observado; no se acredita un HTTP 200.
- dotnet tool restore no pudo descargar dotnet-ef desde NuGet en este entorno. Se usó la herramienta global 10.0.13 ya instalada para generar y comprobar la migración. El manifiesto fija esa misma versión para los compañeros.
- P22 quedó confirmada por la ejecución posterior de Diana. P21 sigue pendiente: necesita el CRUD de Pisos que implementará Alejandro y la interfaz de prueba del grupo.
- Swagger UI, DTO, CRUD, consultas relacionadas, PDF y ZIP académico final no forman parte de este aporte de datos.

No se incluyen capturas o resultados ficticios. La guía Configuracion_SQLServer.md permite completar las pruebas pendientes y el README distingue el modelo verificado de la persistencia todavía pendiente.
