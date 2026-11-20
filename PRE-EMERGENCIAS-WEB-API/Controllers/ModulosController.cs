using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Data;
using PRE_EMERGENCIAS_WEB_API.Dtos.Modulos;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Controllers;

[ApiController]
[Route("api/modulos")]
public sealed class ModulosController(AppDbContext db) : ControllerBase
{
    /// <summary>Lista todos los módulos; soloActivos=true oculta los inactivos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ModuloRespuestaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ModuloRespuestaDto>>> GetAll(
        [FromQuery] bool soloActivos = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.Modulos.AsNoTracking();
        if (soloActivos)
        {
            query = query.Where(modulo => modulo.Activo);
        }

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

    /// <summary>Obtiene un módulo por ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ModuloRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuloRespuestaDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
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
            return Problem(statusCode: StatusCodes.Status404NotFound,
                title: "El módulo solicitado no existe.");
        }

        return Ok(modulo);
    }

    /// <summary>Crea un módulo activo y devuelve su ubicación.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ModuloRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ModuloRespuestaDto>> Create(
        CrearModuloDto dto,
        CancellationToken cancellationToken)
    {
        var codigo = dto.Codigo.Trim().ToUpperInvariant();
        var nombre = dto.Nombre.Trim();

        if (codigo.Length is 0 or > 20)
        {
            ModelState.AddModelError(nameof(dto.Codigo), "El código debe tener entre 1 y 20 caracteres.");
        }

        if (nombre.Length is 0 or > 100)
        {
            ModelState.AddModelError(nameof(dto.Nombre), "El nombre debe tener entre 1 y 100 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (await db.Modulos.AnyAsync(modulo => modulo.Codigo == codigo, cancellationToken))
        {
            ModelState.AddModelError(nameof(dto.Codigo), "Ya existe un módulo con ese código.");
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
            ModelState.AddModelError(nameof(dto.Codigo), "Ya existe un módulo con ese código.");
            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = nuevo.ModuloId },
            ToResponse(nuevo));
    }

    private static ModuloRespuestaDto ToResponse(Modulo modulo) =>
        new(modulo.ModuloId, modulo.Codigo, modulo.Nombre, modulo.Activo);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.GetBaseException() is Microsoft.Data.SqlClient.SqlException sql
        && sql.Number is 2601 or 2627;
}
