using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Data;

var seedRequested = args.Contains("--seed", StringComparer.OrdinalIgnoreCase);
var builder = WebApplication.CreateBuilder(
    args.Where(arg => !arg.Equals("--seed", StringComparison.OrdinalIgnoreCase)).ToArray());

builder.Services.AddControllers();
builder.Services.AddOpenApi();
var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

var app = builder.Build();

// No se aplican migraciones ni se cargan datos al iniciar normalmente.
if (seedRequested)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
    app.Logger.LogInformation("Carga inicial finalizada; los datos existentes se conservaron.");
    return;
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
