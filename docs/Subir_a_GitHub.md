# Subir el trabajo de Diana

El repositorio local conserva los commits originales del grupo. Los cambios de Diana quedan en feature/diana-datos-sqlserver, separados de main.

## Publicar la rama

1. Abrir PRE-EMERGENCIAS-WEB-API.slnx en Visual Studio.
2. Abrir Ver > Terminal y confirmar que está en la carpeta que contiene README.md.
3. Ejecutar:

```powershell
git status
git log -5 --oneline
git push -u origin feature/diana-datos-sqlserver
```

Si GitHub solicita inicio de sesión, usar la cuenta dianavaldivia001. Si rechaza la escritura, el dueño debe confirmar que esa cuenta es colaboradora y que se aceptó la invitación; también puede habilitar el acceso de la conexión de GitHub utilizada en ChatGPT. No cambiar el remoto a un repositorio de otra persona ni usar push --force.

## Abrir la solicitud de revisión

Después de que push finalice correctamente, abrir:

https://github.com/Claud-ito/PROYECTO-INTEGRADOR-WEB-API/compare/main...feature/diana-datos-sqlserver

Seleccionar Create pull request y usar como título:

`feat: datos SQL Server y modelo de ubicaciones (Diana)`

Descripción sugerida:

> Agrega el modelo Modulo–Piso, contexto EF Core y conexión SQL Server, índices únicos, relación sin cascada, migración inicial y carga explícita de prueba. Incluye guía de instalación y verificador del modelo.
>
> Validación: compilación sin errores ni advertencias, 11 comprobaciones del modelo correctas y arranque de la API confirmado. Pendientes: aplicar la migración y ejecutar seed sobre SQL Server accesible, y demostrar persistencia por API después de integrar el CRUD. LocalDB no fue accesible desde el entorno de ejecución.

El líder puede revisar y fusionar conservando los commits y sus autores. No se debe presentar el proyecto completo como terminado: los controladores y las evidencias SQL todavía están pendientes.
