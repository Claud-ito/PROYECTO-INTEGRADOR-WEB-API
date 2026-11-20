
using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Data;

var seedRequested = args.Contains("--seed", StringComparer.OrdinalIgnoreCase);
var builder = WebApplication.CreateBuilder(
    args.Where(arg => !arg.Equals("--seed", StringComparison.OrdinalIgnoreCase)).ToArray());

// ==========================================
// CONFIGURACIÓN ORIGINAL DEL PROYECTO
// ==========================================

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(connection));

// ==========================================
// INTEGRANTE 4 - SWAGGER UI
// ==========================================

// No se modifican los servicios de los otros integrantes.
// Swagger UI utilizará el documento OpenAPI existente.

var app = builder.Build();

// ==========================================
// CARGA INICIAL DE DATOS (INTEGRANTE 1)
// ==========================================

// No se aplican migraciones ni se cargan datos
// al iniciar normalmente.
if (seedRequested)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(db);

    app.Logger.LogInformation(
        "Carga inicial finalizada; los datos existentes se conservaron.");

    return;
}

// ==========================================
// INTEGRANTE 4 - DOCUMENTACIÓN SWAGGER
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "UNIVALLE Emergencias API v1");

        options.RoutePrefix = "swagger";
        options.DocumentTitle = "UNIVALLE Emergencias - Swagger UI";
    });
}

// ==========================================
// CONFIGURACIÓN ORIGINAL
// ==========================================

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
