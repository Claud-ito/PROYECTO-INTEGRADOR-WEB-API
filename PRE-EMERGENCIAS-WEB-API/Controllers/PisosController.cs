using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Data;
using PRE_EMERGENCIAS_WEB_API.Dtos.Pisos;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Controllers;

/// <summary>CRUD del recurso principal: pisos de cada módulo.</summary>
[ApiController]
[Route("api/pisos")]
[Produces("application/json")]
[Tags("Pisos")]
public class PisosController(AppDbContext db) : ControllerBase
{
    // Errores de SQL Server: 2601/2627 = índice único, 547 = restricción FK o CHECK.
    private const int SqlIndiceUnicoDuplicado = 2601;
    private const int SqlRestriccionUnicaDuplicada = 2627;
    private const int SqlConflictoRestriccion = 547;

    private static readonly Expression<Func<Piso, PisoRespuestaDto>> ARespuesta = p => new PisoRespuestaDto
    {
        PisoId = p.PisoId,
        ModuloId = p.ModuloId,
        ModuloCodigo = p.Modulo.Codigo,
        ModuloNombre = p.Modulo.Nombre,
        Denominacion = p.Denominacion,
        Activo = p.Activo
    };

    /// <summary>Lista los pisos. Por defecto incluye los inactivos para poder verificar las bajas.</summary>
    /// <param name="soloActivos">true para devolver únicamente pisos activos.</param>
    [HttpGet]
    [EndpointSummary("Listar pisos")]
    [EndpointDescription("Devuelve todos los pisos con los datos de su módulo. Con soloActivos=true omite los pisos dados de baja (Activo=false).")]
    [ProducesResponseType<List<PisoRespuestaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PisoRespuestaDto>>> Listar([FromQuery] bool? soloActivos,
        CancellationToken cancellationToken)
    {
        var consulta = db.Pisos.AsNoTracking();
        if (soloActivos == true)
            consulta = consulta.Where(p => p.Activo);

        return await consulta
            .OrderBy(p => p.ModuloId).ThenBy(p => p.Denominacion)
            .Select(ARespuesta)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Obtiene un piso por su identificador.</summary>
    [HttpGet("{id:int}")]
    [EndpointSummary("Obtener piso por ID")]
    [ProducesResponseType<PisoRespuestaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PisoRespuestaDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var piso = await db.Pisos.AsNoTracking()
            .Where(p => p.PisoId == id)
            .Select(ARespuesta)
            .SingleOrDefaultAsync(cancellationToken);

        return piso is null ? PisoNoEncontrado(id) : piso;
    }

    /// <summary>Registra un piso en un módulo activo.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Crear piso")]
    [EndpointDescription("Crea un piso y devuelve 201 con el encabezado Location apuntando a GET /api/pisos/{id}. El módulo debe existir y estar activo, y la denominación no puede repetirse dentro del mismo módulo (sin distinguir mayúsculas).")]
    [ProducesResponseType<PisoRespuestaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PisoRespuestaDto>> Crear(CrearPisoDto dto, CancellationToken cancellationToken)
    {
        var denominacion = dto.Denominacion.Trim();

        var error = await ValidarModuloAsync(dto.ModuloId, exigirActivo: true, cancellationToken)
                    ?? await ValidarDenominacionUnicaAsync(dto.ModuloId, denominacion, pisoIdExcluido: null,
                        cancellationToken);
        if (error is not null)
            return error;

        var piso = new Piso { ModuloId = dto.ModuloId, Denominacion = denominacion, Activo = true };
        db.Pisos.Add(piso);

        var errorGuardado = await GuardarAsync(cancellationToken);
        if (errorGuardado is not null)
            return errorGuardado;

        var respuesta = await db.Pisos.AsNoTracking()
            .Where(p => p.PisoId == piso.PisoId)
            .Select(ARespuesta)
            .SingleAsync(cancellationToken);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = piso.PisoId }, respuesta);
    }

    /// <summary>Modifica todos los campos editables de un piso.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("Actualizar piso")]
    [EndpointDescription("Reemplaza ModuloId, Denominacion y Activo. Activo=false registra una baja lógica que conserva el registro. No se puede trasladar un piso a un módulo inactivo.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, ActualizarPisoDto dto, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var piso = await db.Pisos.SingleOrDefaultAsync(p => p.PisoId == id, cancellationToken);
        if (piso is null)
            return PisoNoEncontrado(id);

        var denominacion = dto.Denominacion.Trim();
        var cambiaDeModulo = piso.ModuloId != dto.ModuloId;

        // Solo se exige módulo activo al trasladar: un piso que ya está en un módulo
        // inactivo puede seguir editándose (por ejemplo, para darlo de baja).
        var error = await ValidarModuloAsync(dto.ModuloId, exigirActivo: cambiaDeModulo, cancellationToken)
                    ?? await ValidarDenominacionUnicaAsync(dto.ModuloId, denominacion, pisoIdExcluido: id,
                        cancellationToken);
        if (error is not null)
            return error;

        piso.ModuloId = dto.ModuloId;
        piso.Denominacion = denominacion;
        piso.Activo = dto.Activo!.Value;

        return await GuardarAsync(cancellationToken) ?? NoContent();
    }

    /// <summary>Elimina físicamente un piso sin referencias.</summary>
    [HttpDelete("{id:int}")]
    [EndpointSummary("Eliminar piso")]
    [EndpointDescription("Eliminación física. Si otro registro hace referencia al piso devuelve 400; en ese caso debe usarse la baja lógica con PUT y Activo=false.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return IdInvalido();

        var piso = await db.Pisos.SingleOrDefaultAsync(p => p.PisoId == id, cancellationToken);
        if (piso is null)
            return PisoNoEncontrado(id);

        db.Pisos.Remove(piso);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (NumeroErrorSql(ex) == SqlConflictoRestriccion)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "El piso tiene referencias.",
                detail: "No se puede eliminar porque otros registros lo utilizan. Use PUT con Activo=false para darlo de baja.");
        }

        return NoContent();
    }

    private async Task<ActionResult?> ValidarModuloAsync(int moduloId, bool exigirActivo,
        CancellationToken cancellationToken)
    {
        var modulo = await db.Modulos.AsNoTracking()
            .Where(m => m.ModuloId == moduloId)
            .Select(m => new { m.Activo })
            .SingleOrDefaultAsync(cancellationToken);

        if (modulo is null)
            return ErrorValidacion(nameof(CrearPisoDto.ModuloId), "El módulo indicado no existe.");

        if (exigirActivo && !modulo.Activo)
            return ErrorValidacion(nameof(CrearPisoDto.ModuloId),
                "El módulo indicado está inactivo y no admite nuevos pisos.");

        return null;
    }

    private async Task<ActionResult?> ValidarDenominacionUnicaAsync(int moduloId, string denominacion,
        int? pisoIdExcluido, CancellationToken cancellationToken)
    {
        // La columna usa una intercalación sin distinción de mayúsculas, así que la
        // comparación se resuelve en SQL Server con el mismo criterio que el índice único.
        var duplicado = await db.Pisos.AnyAsync(p =>
            p.ModuloId == moduloId &&
            p.Denominacion == denominacion &&
            (pisoIdExcluido == null || p.PisoId != pisoIdExcluido), cancellationToken);

        return duplicado ? DenominacionDuplicada() : null;
    }

    private async Task<ActionResult?> GuardarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateException ex) when (NumeroErrorSql(ex) is SqlIndiceUnicoDuplicado or SqlRestriccionUnicaDuplicada)
        {
            // Otra solicitud guardó la misma denominación entre la validación y el guardado.
            return DenominacionDuplicada();
        }
        catch (DbUpdateException ex) when (NumeroErrorSql(ex) == SqlConflictoRestriccion)
        {
            // El módulo se eliminó entre la validación y el guardado.
            return ErrorValidacion(nameof(CrearPisoDto.ModuloId), "El módulo indicado no existe.");
        }
    }

    private static int? NumeroErrorSql(DbUpdateException ex) => (ex.InnerException as SqlException)?.Number;

    private ActionResult DenominacionDuplicada() => ErrorValidacion(nameof(CrearPisoDto.Denominacion),
        "Ya existe un piso con esa denominación en el módulo indicado.");

    private ActionResult IdInvalido() => ErrorValidacion("id", "El identificador debe ser un número positivo.");

    private ActionResult ErrorValidacion(string campo, string mensaje)
    {
        ModelState.AddModelError(campo, mensaje);
        return ValidationProblem(ModelState);
    }

    private ActionResult PisoNoEncontrado(int id) => Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Piso no encontrado.",
        detail: $"No existe un piso con PisoId {id}.");
}
