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
}
