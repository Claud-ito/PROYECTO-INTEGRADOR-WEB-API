
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Data;
using PRE_EMERGENCIAS_WEB_API.Dtos.Pisos;
using PRE_EMERGENCIAS_WEB_API.Dtos.Modulos;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Controllers;

[ApiController]
[Route("api/pisos")]
[Produces("application/json")]
[Tags("Pisos")]
public class PisosController(AppDbContext db) : ControllerBase
{
    private const int SqlIndiceUnicoDuplicado = 2601;
    private const int SqlRestriccionUnicaDuplicada = 2627;
    private const int SqlConflictoRestriccion = 547;

    private static readonly Expression<Func<Piso, PisoRespuestaDto>> ARespuesta = p =>
        new PisoRespuestaDto
        {
            PisoId = p.PisoId,
            ModuloId = p.ModuloId,
            ModuloCodigo = p.Modulo.Codigo,
            ModuloNombre = p.Modulo.Nombre,
            Denominacion = p.Denominacion,
            Activo = p.Activo
        };

    // ==========================================
    // GET - LISTAR PISOS
    // ==========================================

    [HttpGet]
    [EndpointSummary("Listar pisos")]
    [EndpointDescription(
        "Devuelve todos los pisos con los datos de su módulo.")]
    [ProducesResponseType<List<PisoRespuestaDto>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PisoRespuestaDto>>> Listar(
        [FromQuery] bool? soloActivos,
        CancellationToken cancellationToken)
    {
        var consulta = db.Pisos.AsNoTracking();

        if (soloActivos == true)
            consulta = consulta.Where(p => p.Activo);

        return await consulta
            .OrderBy(p => p.ModuloId)
            .ThenBy(p => p.Denominacion)
            .Select(ARespuesta)
            .ToListAsync(cancellationToken);
    }

    // ==========================================
    // GET - OBTENER PISO POR ID
    // ==========================================

    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener piso por ID")]
    [ProducesResponseType<PisoRespuestaDto>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PisoRespuestaDto>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var piso = await db.Pisos
            .AsNoTracking()
            .Where(p => p.PisoId == id)
            .Select(ARespuesta)
            .SingleOrDefaultAsync(cancellationToken);

        return piso is null ? PisoNoEncontrado(id) : piso;
    }

    // ==========================================
    // INTEGRANTE 4
    // GET /api/pisos/{id}/modulo
    // ==========================================

    /// <summary>
    /// Obtiene el módulo asociado a un piso.
    /// </summary>
    [HttpGet("{id:int}/modulo")]
    [EndpointSummary("Obtener módulo de un piso")]
    [EndpointDescription(
        "Devuelve el módulo al que pertenece el piso indicado.")]
    [ProducesResponseType<ModuloRespuestaDto>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuloRespuestaDto>> ObtenerModuloDePiso(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var modulo = await db.Pisos
            .AsNoTracking()
            .Where(p => p.PisoId == id)
            .Select(p => new ModuloRespuestaDto(
                p.Modulo.ModuloId,
                p.Modulo.Codigo,
                p.Modulo.Nombre,
                p.Modulo.Activo))
            .SingleOrDefaultAsync(cancellationToken);

        if (modulo is null)
            return PisoNoEncontrado(id);

        return Ok(modulo);
    }

    // ==========================================
    // POST - CREAR PISO
    // ==========================================

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Crear piso")]
    [ProducesResponseType<PisoRespuestaDto>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PisoRespuestaDto>> Crear(
        CrearPisoDto dto,
        CancellationToken cancellationToken)
    {
        var denominacion = dto.Denominacion.Trim();

        var error = await ValidarModuloAsync(
            dto.ModuloId, true, cancellationToken)
            ?? await ValidarDenominacionUnicaAsync(
                dto.ModuloId, denominacion, null, cancellationToken);

        if (error is not null)
            return error;

        var piso = new Piso
        {
            ModuloId = dto.ModuloId,
            Denominacion = denominacion,
            Activo = true
        };

        db.Pisos.Add(piso);

        var errorGuardado = await GuardarAsync(cancellationToken);

        if (errorGuardado is not null)
            return errorGuardado;

        var respuesta = await db.Pisos
            .AsNoTracking()
            .Where(p => p.PisoId == piso.PisoId)
            .Select(ARespuesta)
            .SingleAsync(cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = piso.PisoId },
            respuesta);
    }

    // ==========================================
    // PUT - ACTUALIZAR PISO
    // ==========================================

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("Actualizar piso")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarPisoDto dto,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var piso = await db.Pisos.SingleOrDefaultAsync(
            p => p.PisoId == id, cancellationToken);

        if (piso is null)
            return PisoNoEncontrado(id);

        var denominacion = dto.Denominacion.Trim();
        var cambiaDeModulo = piso.ModuloId != dto.ModuloId;

        var error = await ValidarModuloAsync(
            dto.ModuloId, cambiaDeModulo, cancellationToken)
            ?? await ValidarDenominacionUnicaAsync(
                dto.ModuloId, denominacion, id, cancellationToken);

        if (error is not null)
            return error;

        piso.ModuloId = dto.ModuloId;
        piso.Denominacion = denominacion;
        piso.Activo = dto.Activo!.Value;

        return await GuardarAsync(cancellationToken) ?? NoContent();
    }

    // ==========================================
    // DELETE - ELIMINAR PISO
    // ==========================================

    [HttpDelete("{id:int}")]
    [EndpointSummary("Eliminar piso")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var piso = await db.Pisos.SingleOrDefaultAsync(
            p => p.PisoId == id, cancellationToken);

        if (piso is null)
            return PisoNoEncontrado(id);

        db.Pisos.Remove(piso);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (NumeroErrorSql(ex) == SqlConflictoRestriccion)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El piso tiene referencias.",
                detail: "No se puede eliminar porque otros registros lo utilizan.");
        }

        return NoContent();
    }

    // ==========================================
    // MÉTODOS AUXILIARES ORIGINALES
    // ==========================================

    private async Task<ActionResult?> ValidarModuloAsync(
        int moduloId,
        bool exigirActivo,
        CancellationToken cancellationToken)
    {
        var modulo = await db.Modulos
            .AsNoTracking()
            .Where(m => m.ModuloId == moduloId)
            .Select(m => new { m.Activo })
            .SingleOrDefaultAsync(cancellationToken);

        if (modulo is null)
            return ErrorValidacion(
                nameof(CrearPisoDto.ModuloId),
                "El módulo indicado no existe.");

        if (exigirActivo && !modulo.Activo)
            return ErrorValidacion(
                nameof(CrearPisoDto.ModuloId),
                "El módulo indicado está inactivo y no admite nuevos pisos.");

        return null;
    }

    private async Task<ActionResult?> ValidarDenominacionUnicaAsync(
        int moduloId,
        string denominacion,
        int? pisoIdExcluido,
        CancellationToken cancellationToken)
    {
        var duplicado = await db.Pisos.AnyAsync(p =>
            p.ModuloId == moduloId &&
            p.Denominacion == denominacion &&
            (pisoIdExcluido == null || p.PisoId != pisoIdExcluido),
            cancellationToken);

        return duplicado ? DenominacionDuplicada() : null;
    }

    private async Task<ActionResult?> GuardarAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateException ex)
            when (NumeroErrorSql(ex) is
                SqlIndiceUnicoDuplicado or SqlRestriccionUnicaDuplicada)
        {
            return DenominacionDuplicada();
        }
        catch (DbUpdateException ex)
            when (NumeroErrorSql(ex) == SqlConflictoRestriccion)
        {
            return ErrorValidacion(
                nameof(CrearPisoDto.ModuloId),
                "El módulo indicado no existe.");
        }
    }

    private static int? NumeroErrorSql(DbUpdateException ex) =>
        (ex.InnerException as SqlException)?.Number;

    private ActionResult DenominacionDuplicada() =>
        ErrorValidacion(
            nameof(CrearPisoDto.Denominacion),
            "Ya existe un piso con esa denominación en el módulo indicado.");

    private ActionResult IdInvalido() =>
        ErrorValidacion("id", "El identificador debe ser un número positivo.");

    private ActionResult ErrorValidacion(string campo, string mensaje)
    {
        ModelState.AddModelError(campo, mensaje);
        return ValidationProblem(ModelState);
    }

    private ActionResult PisoNoEncontrado(int id) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Piso no encontrado.",
            detail: $"No existe un piso con PisoId {id}.");
}
