# Aporte para el README: CRUD de Módulos

**Responsable:** Said Montaño Condori (`Claud-ito`). Esta sección cubre únicamente el CRUD del recurso relacionado. El integrante 4 incorpora las consultas relacionadas y prepara el README y PDF finales.

## Contrato HTTP

| Método y ruta | Éxito | Errores esperados |
| --- | --- | --- |
| `GET /api/modulos?soloActivos=true` | `200`, lista de `ModuloRespuestaDto` | `400` si el filtro no es booleano |
| `GET /api/modulos/{id}` | `200`, `ModuloRespuestaDto` | `400` si el ID no es positivo; `404` si no existe |
| `POST /api/modulos` | `201`, DTO y `Location` al GET del nuevo ID | `400` por datos inválidos o código repetido |
| `PUT /api/modulos/{id}` | `204`, sin cuerpo | `400` por datos inválidos o código repetido; `404` si no existe |
| `DELETE /api/modulos/{id}` | `204`, sin cuerpo | `400` si tiene pisos u otras referencias; `404` si no existe |

`CrearModuloDto` recibe `Codigo` y `Nombre`. `ActualizarModuloDto` exige además `Activo`. `ModuloRespuestaDto` devuelve `ModuloId`, `Codigo`, `Nombre` y `Activo`; no expone la entidad EF ni la colección de pisos. Los ID los genera SQL Server.

Se recortan espacios al crear y actualizar. El código se guarda en mayúsculas y debe ser único, incluidos los módulos inactivos. El código admite hasta 20 caracteres y el nombre hasta 100. Se rechazan valores ausentes, vacíos o solo con espacios. El índice único de `Codigo` y una comparación SQL sin distinción de mayúsculas/minúsculas forman parte del modelo que integra el responsable de datos.

La lista muestra activos e inactivos. `soloActivos=true` filtra los inactivos. PUT con `activo=false` da de baja el módulo sin perder referencias históricas. DELETE borra físicamente un módulo sin pisos; si tiene pisos devuelve `400` con una explicación. Una restricción de clave foránea que rechace el borrado por una referencia concurrente también se traduce a `400`.

Las rutas usan `AppDbContext` inyectado y métodos asíncronos de EF Core. El controlador espera `DbSet<Modulo> Modulos`, `DbSet<Piso> Pisos` y las propiedades del modelo acordado en el reparto V2. Al integrar la rama de datos se debe conservar ese contrato o adaptar ambos archivos juntos. El controlador deja la acción `GET /api/modulos/{id}/pisos` al integrante 4.

## Repetir las pruebas

1. Restaurar y arrancar la API integrada con SQL Server y la migración aplicada.
2. Ejecutar `docs/pruebas/Modulos.http`, sustituyendo los ID por los devueltos por POST. El ID generado puede saltar valores; nunca asumir que el siguiente es el anterior más uno.
3. Para una verificación automática, ejecutar:

```powershell
pwsh -File docs/pruebas/VerificarModulos.ps1 -BaseUrl http://localhost:5077 -ModuloConPisosId ID_REAL_CON_PISOS -OutputPath docs/evidencias/modulos/resultado-local.json -DatabaseName NOMBRE_DE_LA_BASE
```

El script crea módulos con códigos únicos, comprueba los códigos HTTP y los elimina si la ejecución termina correctamente. `-ModuloConPisosId` permite comprobar el rechazo por dependencia y debe identificar un módulo que ya tenga pisos. El archivo JSON contiene método, ruta, cuerpo enviado, estado, `Location` y cuerpo realmente recibido. Si se omite ese parámetro, la comprobación de dependencia no se ejecuta.

La rama `feature/modulos` depende de la base del integrante 1 para compilar y ejecutar la API completa. La evidencia JSON incluida se obtuvo en una **copia temporal de integración** con EF Core 10 y LocalDB, porque esa base aún no está integrada en la rama de entrega. Las pruebas y capturas definitivas deben repetirse sobre la versión integrada antes del PDF final; el integrante 4 reúne las capturas de Swagger y las consultas relacionadas.

## Datos confirmados del grupo para la integración

| Integrante | Nombre | Usuario GitHub |
| --- | --- | --- |
| 1 | Diana Valdivia Vaca Diez | `Dianavaldivia001` |
| 2 | Alejandro Rojas Espinoza | `alejandrorojas03010-estrella` |
| 3 | Said Montaño Condori | `Claud-ito` |
| 4 | Jorge Daniel Gutierrez Espinosa | `jorgegut28` |

Repositorio configurado como `origin`: https://github.com/Claud-ito/PROYECTO-INTEGRADOR-WEB-API.git. No se ha hecho push desde esta rama.
