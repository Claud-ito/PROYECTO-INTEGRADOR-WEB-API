# Resultados de pruebas — CRUD de Pisos

**Ejecutó:** Alejandro Rojas (Integrante 2) · **Fecha:** 19 de noviembre de 2026

**Entorno:** macOS (arm64), SDK .NET 10.0.404, SQL Server 2022 en Docker (`mcr.microsoft.com/mssql/server:2022-latest`), base nueva `UnivalleEmergenciasApi_Pisos`. Se aplicó la migración `20261120022734_InicialUbicaciones` y la carga `--seed` de Diana (M01 y M02 activos, M03 inactivo, cuatro pisos).

Las respuestas se obtuvieron con `curl` contra `http://localhost:5077` y se copian sin editar. Son el registro textual de la ejecución; las capturas de Swagger UI para el PDF se tomarán cuando el grupo integre Swagger UI.

**Resumen:** 31 solicitudes ejecutadas, 31 con el código HTTP esperado. P21 (persistencia tras reinicio) también se comprobó; ver la última sección.

## Resultados

### P02 — Listar pisos

- Solicitud: `GET /api/pisos`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
[
    {
        "pisoId": 2,
        "moduloId": 1,
        "moduloCodigo": "M01",
        "moduloNombre": "Modulo 1 (ejemplo)",
        "denominacion": "Piso 1",
        "activo": true
    },
    {
        "pisoId": 1,
        "moduloId": 1,
        "moduloCodigo": "M01",
        "moduloNombre": "Modulo 1 (ejemplo)",
        "denominacion": "Planta baja",
        "activo": true
    },
    {
        "pisoId": 4,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Piso 1",
        "activo": true
    },
    {
        "pisoId": 3,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Planta baja",
        "activo": true
    }
]

```

### P02b — Listar solo activos

- Solicitud: `GET /api/pisos?soloActivos=true`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
[
    {
        "pisoId": 2,
        "moduloId": 1,
        "moduloCodigo": "M01",
        "moduloNombre": "Modulo 1 (ejemplo)",
        "denominacion": "Piso 1",
        "activo": true
    },
    {
        "pisoId": 1,
        "moduloId": 1,
        "moduloCodigo": "M01",
        "moduloNombre": "Modulo 1 (ejemplo)",
        "denominacion": "Planta baja",
        "activo": true
    },
    {
        "pisoId": 4,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Piso 1",
        "activo": true
    },
    {
        "pisoId": 3,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Planta baja",
        "activo": true
    }
]

```

### P02c — Filtro no válido

- Solicitud: `GET /api/pisos?soloActivos=abc`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "soloActivos": [
            "The value 'abc' is not valid."
        ]
    },
    "traceId": "00-923797e89a8d69d15e8e39b96a82aa04-51b7426ef1baa912-00"
}

```

### P03 — Obtener piso existente

- Solicitud: `GET /api/pisos/1`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
{
    "pisoId": 1,
    "moduloId": 1,
    "moduloCodigo": "M01",
    "moduloNombre": "Modulo 1 (ejemplo)",
    "denominacion": "Planta baja",
    "activo": true
}

```

### P04 — Crear piso válido

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"Piso 2"}`
- HTTP esperado: 201 · HTTP recibido: **201** · OK
- Location: `http://localhost:5077/api/pisos/5`

```json
{
    "pisoId": 5,
    "moduloId": 1,
    "moduloCodigo": "M01",
    "moduloNombre": "Modulo 1 (ejemplo)",
    "denominacion": "Piso 2",
    "activo": true
}

```

### P05 — Seguir el Location de P04

- Solicitud: `GET /api/pisos/5`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
{
    "pisoId": 5,
    "moduloId": 1,
    "moduloCodigo": "M01",
    "moduloNombre": "Modulo 1 (ejemplo)",
    "denominacion": "Piso 2",
    "activo": true
}

```

### P06 — Actualizar piso creado

- Solicitud: `PUT /api/pisos/5`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"Segundo piso","activo":true}`
- HTTP esperado: 204 · HTTP recibido: **204** · OK
- Sin cuerpo de respuesta (204 No Content).

### P06b — GET posterior a la actualización

- Solicitud: `GET /api/pisos/5`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
{
    "pisoId": 5,
    "moduloId": 1,
    "moduloCodigo": "M01",
    "moduloNombre": "Modulo 1 (ejemplo)",
    "denominacion": "Segundo piso",
    "activo": true
}

```

### P07a — Crear piso temporal para borrar

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":2,"denominacion":"Piso temporal"}`
- HTTP esperado: 201 · HTTP recibido: **201** · OK
- Location: `http://localhost:5077/api/pisos/6`

```json
{
    "pisoId": 6,
    "moduloId": 2,
    "moduloCodigo": "M02",
    "moduloNombre": "Modulo 2 (ejemplo)",
    "denominacion": "Piso temporal",
    "activo": true
}

```

### P07 — Eliminar piso sin referencias

- Solicitud: `DELETE /api/pisos/6`
- HTTP esperado: 204 · HTTP recibido: **204** · OK
- Sin cuerpo de respuesta (204 No Content).

### P07b — GET posterior a la eliminación

- Solicitud: `GET /api/pisos/6`
- HTTP esperado: 404 · HTTP recibido: **404** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "Piso no encontrado.",
    "status": 404,
    "detail": "No existe un piso con PisoId 6.",
    "traceId": "00-2ebaf1e502ff67766ef0b7b67a04d554-dc337e3edf1c308f-00"
}

```

### P08 — Denominación vacía

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1,"denominacion":""}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "La denominación es obligatoria."
        ]
    },
    "traceId": "00-aae1ce7f85ea79b4a76197c512e1899a-bab22d2cac404b0e-00"
}

```

### P08b — Denominación solo con espacios

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"   "}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "La denominación es obligatoria."
        ]
    },
    "traceId": "00-9f557ecfa3340bc9339c416b753b640f-cc1fdc37d3282860-00"
}

```

### P08c — Denominación ausente

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "La denominación es obligatoria."
        ]
    },
    "traceId": "00-1bec3e36deccc1e01728bf926393d101-31feb156b809d86b-00"
}

```

### P08d — Denominación de más de 50 caracteres

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "La denominación admite como máximo 50 caracteres."
        ]
    },
    "traceId": "00-4721c97ba1a00a256c02998a9fe4eea1-6efa064b39752b7e-00"
}

```

### P08e — ModuloId no positivo

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":0,"denominacion":"Piso X"}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "ModuloId": [
            "El ModuloId debe ser un número positivo."
        ]
    },
    "traceId": "00-0631839535c563cf5ae903bcbe244e19-eaf30eca2cfd736a-00"
}

```

### P09 — ModuloId inexistente

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":999,"denominacion":"Piso X"}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "ModuloId": [
            "El módulo indicado no existe."
        ]
    },
    "traceId": "00-7a75381a3ac3e24334ba031654152ca5-9d958c45136e4951-00"
}

```

### P10 — Denominación duplicada en el mismo módulo

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"Piso 1"}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "Ya existe un piso con esa denominación en el módulo indicado."
        ]
    },
    "traceId": "00-0dd5fe8ad8135fd8cb510397f1db2c9d-5f1dd0907a2f8697-00"
}

```

### P10b — Duplicado con otras mayúsculas y espacios

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"  PISO 1 "}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "Ya existe un piso con esa denominación en el módulo indicado."
        ]
    },
    "traceId": "00-5551b4d034e872947faadde2092d0896-aecb686d45133daa-00"
}

```

### P11 — Misma denominación en otro módulo activo

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":2,"denominacion":"Segundo piso"}`
- HTTP esperado: 201 · HTTP recibido: **201** · OK
- Location: `http://localhost:5077/api/pisos/7`

```json
{
    "pisoId": 7,
    "moduloId": 2,
    "moduloCodigo": "M02",
    "moduloNombre": "Modulo 2 (ejemplo)",
    "denominacion": "Segundo piso",
    "activo": true
}

```

### P12a — GET de piso inexistente

- Solicitud: `GET /api/pisos/99999`
- HTTP esperado: 404 · HTTP recibido: **404** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "Piso no encontrado.",
    "status": 404,
    "detail": "No existe un piso con PisoId 99999.",
    "traceId": "00-d72b504482aea99ba3ba5680abcad56d-d537ed7ac45e3543-00"
}

```

### P12b — PUT de piso inexistente

- Solicitud: `PUT /api/pisos/99999`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"Nada","activo":true}`
- HTTP esperado: 404 · HTTP recibido: **404** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "Piso no encontrado.",
    "status": 404,
    "detail": "No existe un piso con PisoId 99999.",
    "traceId": "00-ccd8748f53b84d20d3e29e8184d1cc3f-43619f983a4792bd-00"
}

```

### P12c — DELETE de piso inexistente

- Solicitud: `DELETE /api/pisos/99999`
- HTTP esperado: 404 · HTTP recibido: **404** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "Piso no encontrado.",
    "status": 404,
    "detail": "No existe un piso con PisoId 99999.",
    "traceId": "00-10aa31444911cc5cb8f9493ed695ac1e-fda55dcaf4003d5b-00"
}

```

### P12d — ID no positivo

- Solicitud: `GET /api/pisos/0`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "id": [
            "El identificador debe ser un número positivo."
        ]
    },
    "traceId": "00-dfba3154c920a80981b3d73d882e5b0d-ed8e926a68f36105-00"
}

```

### P19 — Crear piso en módulo inactivo (M03)

- Solicitud: `POST /api/pisos`
- Cuerpo enviado: `{"moduloId":3,"denominacion":"Piso 1"}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "ModuloId": [
            "El módulo indicado está inactivo y no admite nuevos pisos."
        ]
    },
    "traceId": "00-1e9f90a507144c0155542f21f04d64b7-2356dfb911418734-00"
}

```

### P19b — Trasladar piso a módulo inactivo

- Solicitud: `PUT /api/pisos/5`
- Cuerpo enviado: `{"moduloId":3,"denominacion":"Segundo piso","activo":true}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "ModuloId": [
            "El módulo indicado está inactivo y no admite nuevos pisos."
        ]
    },
    "traceId": "00-855db18241e16d5f6dcc41406ccc215e-a43054bd4e70aa48-00"
}

```

### P19c — PUT duplicando otro piso del módulo

- Solicitud: `PUT /api/pisos/5`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"piso 1","activo":true}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Denominacion": [
            "Ya existe un piso con esa denominación en el módulo indicado."
        ]
    },
    "traceId": "00-43bc019d98d434c4f9cb74bbf9342c26-973d6fe93ce2723e-00"
}

```

### P19d — PUT sin el campo Activo

- Solicitud: `PUT /api/pisos/5`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"Segundo piso"}`
- HTTP esperado: 400 · HTTP recibido: **400** · OK

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "Activo": [
            "El campo Activo es obligatorio."
        ]
    },
    "traceId": "00-0b8b4b78a1781272967840b99a7d961c-93698071ea080ee4-00"
}

```

### P20 — Baja lógica con Activo=false

- Solicitud: `PUT /api/pisos/5`
- Cuerpo enviado: `{"moduloId":1,"denominacion":"Segundo piso","activo":false}`
- HTTP esperado: 204 · HTTP recibido: **204** · OK
- Sin cuerpo de respuesta (204 No Content).

### P20b — GET posterior muestra Activo=false

- Solicitud: `GET /api/pisos/5`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
{
    "pisoId": 5,
    "moduloId": 1,
    "moduloCodigo": "M01",
    "moduloNombre": "Modulo 1 (ejemplo)",
    "denominacion": "Segundo piso",
    "activo": false
}

```

### P20c — Listado con soloActivos=true omite la baja

- Solicitud: `GET /api/pisos?soloActivos=true`
- HTTP esperado: 200 · HTTP recibido: **200** · OK

```json
[
    {
        "pisoId": 2,
        "moduloId": 1,
        "moduloCodigo": "M01",
        "moduloNombre": "Modulo 1 (ejemplo)",
        "denominacion": "Piso 1",
        "activo": true
    },
    {
        "pisoId": 1,
        "moduloId": 1,
        "moduloCodigo": "M01",
        "moduloNombre": "Modulo 1 (ejemplo)",
        "denominacion": "Planta baja",
        "activo": true
    },
    {
        "pisoId": 4,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Piso 1",
        "activo": true
    },
    {
        "pisoId": 3,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Planta baja",
        "activo": true
    },
    {
        "pisoId": 7,
        "moduloId": 2,
        "moduloCodigo": "M02",
        "moduloNombre": "Modulo 2 (ejemplo)",
        "denominacion": "Segundo piso",
        "activo": true
    }
]

```

## P21 — Persistencia tras reinicio (apoyo a Diana)

1. `POST /api/pisos` con `{"moduloId":1,"denominacion":"Persistencia-20261119-A"}` → **201 Created**, `Location: http://localhost:5077/api/pisos/8`.
2. Se detuvo por completo el proceso de la API (sin procesos activos y sin respuesta en el puerto 5077).
3. Se inició otra vez con la misma conexión, sin `--seed` ni cambios en la base.
4. `GET /api/pisos/8` a las 23:00:29 → **200 OK**:

```json
{"pisoId":8,"moduloId":1,"moduloCodigo":"M01","moduloNombre":"Modulo 1 (ejemplo)","denominacion":"Persistencia-20261119-A","activo":true}
```

La evidencia oficial de P21 para el PDF la captura Diana desde Swagger UI con su propia base.
