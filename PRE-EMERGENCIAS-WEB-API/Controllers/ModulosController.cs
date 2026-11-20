
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Data;
using PRE_EMERGENCIAS_WEB_API.Dtos.Modulos;
using PRE_EMERGENCIAS_WEB_API.Dtos.Pisos;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Controllers;

[ApiController]
[Route("api/modulos")]
public sealed class ModulosController(AppDbContext db) : ControllerBase
{
    // ==========================================
    // GET - LISTAR MÓDULOS
    // ==========================================

    [HttpGet]
    [ProducesResponseType(
        typeof(List<ModuloRespuestaDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ModuloRespuestaDto>>> GetAll(
        [FromQuery] bool soloActivos = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.Modulos.AsNoTracking();

        if (soloActivos)
            query = query.Where(modulo => modulo.Activo);

        var modulos = await query
            .OrderBy(modulo => modulo.Codigo)
            .Select(modulo => new ModuloRespuestaDto(
                modulo.ModuloId,
                modulo.Codigo,
                modulo.Nombre,
                modulo.Activo))
            .ToListAsync(cancellationToken);

        return Ok(modulos);
    }

    // ==========================================
    // GET - OBTENER MÓDULO POR ID
    // ==========================================

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ModuloRespuestaDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuloRespuestaDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El ID del módulo debe ser positivo.");
        }

        var modulo = await db.Modulos
            .AsNoTracking()
            .Where(item => item.ModuloId == id)
            .Select(item => new ModuloRespuestaDto(
                item.ModuloId,
                item.Codigo,
                item.Nombre,
                item.Activo))
            .SingleOrDefaultAsync(cancellationToken);

        if (modulo is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.");
        }

        return Ok(modulo);
    }

    // ==========================================
    // INTEGRANTE 4
    // GET /api/modulos/{id}/pisos
    // ==========================================

    /// <summary>
    /// Obtiene los pisos pertenecientes a un módulo.
    /// </summary>
    [HttpGet("{id:int}/pisos")]
    [EndpointSummary("Obtener pisos de un módulo")]
    [EndpointDescription(
        "Devuelve los pisos asociados al módulo. " +
        "Si el módulo existe pero no tiene pisos, devuelve una lista vacía.")]
    [ProducesResponseType<List<PisoRespuestaDto>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PisoRespuestaDto>>> ObtenerPisosDeModulo(
        int id,
        [FromQuery] bool soloActivos = false,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El ID del módulo debe ser positivo.");
        }

        var moduloExiste = await db.Modulos
            .AsNoTracking()
            .AnyAsync(m => m.ModuloId == id, cancellationToken);

        if (!moduloExiste)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.",
                detail: $"No existe un módulo con ModuloId {id}.");
        }

        var consulta = db.Pisos
            .AsNoTracking()
            .Where(p => p.ModuloId == id);

        if (soloActivos)
            consulta = consulta.Where(p => p.Activo);

        var pisos = await consulta
            .OrderBy(p => p.Denominacion)
            .Select(p => new PisoRespuestaDto
            {
                PisoId = p.PisoId,
                ModuloId = p.ModuloId,
                ModuloCodigo = p.Modulo.Codigo,
                ModuloNombre = p.Modulo.Nombre,
                Denominacion = p.Denominacion,
                Activo = p.Activo
            })
            .ToListAsync(cancellationToken);

        return Ok(pisos);
    }

    // ==========================================
    // POST - CREAR MÓDULO
    // ==========================================

    [HttpPost]
    [ProducesResponseType(
        typeof(ModuloRespuestaDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ModuloRespuestaDto>> Create(
        CrearModuloDto dto,
        CancellationToken cancellationToken)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        var nombre = dto.Nombre.Trim();

        if (codigo.Length is 0 or > 20)
        {
            ModelState.AddModelError(
                nameof(dto.Codigo),
                "El código debe tener entre 1 y 20 caracteres.");
        }

        if (nombre.Length is 0 or > 100)
        {
            ModelState.AddModelError(
                nameof(dto.Nombre),
                "El nombre debe tener entre 1 y 100 caracteres.");
        }

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (await db.Modulos.AnyAsync(
            modulo => modulo.Codigo == codigo, cancellationToken))
        {
            ModelState.AddModelError(
                nameof(dto.Codigo),
                "Ya existe un módulo con ese código.");

            return ValidationProblem(ModelState);
        }

        var nuevo = new Modulo
        {
            Codigo = codigo,
            Nombre = nombre,
            Activo = true
        };

        db.Modulos.Add(nuevo);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(
                nameof(dto.Codigo),
                "Ya existe un módulo con ese código.");

            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = nuevo.ModuloId },
            ToResponse(nuevo));
    }

    // ==========================================
    // PUT - ACTUALIZAR MÓDULO
    // ==========================================

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        ActualizarModuloDto dto,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El ID del módulo debe ser positivo.");
        }

        var modulo = await db.Modulos.SingleOrDefaultAsync(
            item => item.ModuloId == id, cancellationToken);

        if (modulo is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.");
        }

        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        var nombre = dto.Nombre.Trim();

        if (codigo.Length is 0 or > 20)
        {
            ModelState.AddModelError(
                nameof(dto.Codigo),
                "El código debe tener entre 1 y 20 caracteres.");
        }

        if (nombre.Length is 0 or > 100)
        {
            ModelState.AddModelError(
                nameof(dto.Nombre),
                "El nombre debe tener entre 1 y 100 caracteres.");
        }

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (await db.Modulos.AnyAsync(
            item => item.Codigo == codigo && item.ModuloId != id,
            cancellationToken))
        {
            ModelState.AddModelError(
                nameof(dto.Codigo),
                "Ya existe otro módulo con ese código.");

            return ValidationProblem(ModelState);
        }

        modulo.Codigo = codigo;
        modulo.Nombre = nombre;
        modulo.Activo = dto.Activo!.Value;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(
                nameof(dto.Codigo),
                "Ya existe otro módulo con ese código.");

            return ValidationProblem(ModelState);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.");
        }

        return NoContent();
    }

    // ==========================================
    // DELETE - ELIMINAR MÓDULO
    // ==========================================

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El ID del módulo debe ser positivo.");
        }

        var modulo = await db.Modulos.SingleOrDefaultAsync(
            item => item.ModuloId == id, cancellationToken);

        if (modulo is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.");
        }

        if (await db.Pisos.AnyAsync(
            piso => piso.ModuloId == id, cancellationToken))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El módulo tiene pisos asociados y no puede eliminarse.",
                detail: "Para conservar su historial, use PUT con activo=false.");
        }

        db.Modulos.Remove(modulo);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El módulo tiene registros relacionados y no puede eliminarse.",
                detail: "Para conservar su historial, use PUT con activo=false.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.");
        }

        return NoContent();
    }

    // ==========================================
    // MÉTODOS AUXILIARES ORIGINALES
    // ==========================================

    private static ModuloRespuestaDto ToResponse(Modulo modulo) =>
        new(
            modulo.ModuloId,
            modulo.Codigo,
            modulo.Nombre,
            modulo.Activo);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.GetBaseException()
            is Microsoft.Data.SqlClient.SqlException sql
        && sql.Number is 2601 or 2627;

    private static bool IsForeignKeyViolation(DbUpdateException exception) =>
        exception.GetBaseException()
            is Microsoft.Data.SqlClient.SqlException sql
        && sql.Number == 547;
}
