using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PRE_EMERGENCIAS_WEB_API.Data;
using PRE_EMERGENCIAS_WEB_API.Models;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=VerificacionSinConexion;Trusted_Connection=True")
    .AddInterceptors(new SinEscritura()).Options;
await using var db = new AppDbContext(options);
void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    Console.WriteLine("OK: " + message);
}
var modulo = db.Model.FindEntityType(typeof(Modulo))!;
var piso = db.Model.FindEntityType(typeof(Piso))!;
Check(modulo.FindProperty(nameof(Modulo.Codigo))!.GetMaxLength() == 20, "Codigo: maximo 20");
Check(modulo.FindProperty(nameof(Modulo.Nombre))!.GetMaxLength() == 100, "Nombre: maximo 100");
Check(piso.FindProperty(nameof(Piso.Denominacion))!.GetMaxLength() == 50, "Denominacion: maximo 50");
Check(modulo.GetIndexes().Any(i => i.IsUnique && i.Properties.Single().Name == "Codigo"), "Codigo unico");
Check(piso.GetIndexes().Any(i => i.IsUnique &&
    i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ModuloId", "Denominacion" })), "Unicidad por modulo");
Check(piso.GetForeignKeys().Single().IsRequired &&
    piso.GetForeignKeys().Single().DeleteBehavior == DeleteBehavior.Restrict, "Relacion obligatoria sin cascada");
Check(!db.Database.HasPendingModelChanges(), "Migracion y snapshot coinciden con el modelo");
var sql = db.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
Check(sql.Contains("CREATE TABLE [Modulos]") && sql.Contains("CREATE TABLE [Pisos]") &&
    sql.Contains("CK_Modulos_Codigo_NoVacio") && sql.Contains("CK_Pisos_Denominacion_NoVacia"),
    "SQL Server: tablas y restricciones generadas");
var m = new Modulo { Codigo = "  m90 ", Nombre = " Ejemplo ", Activo = false };
var p = new Piso { Modulo = m, Denominacion = " Piso 9 " };
db.Add(p);
await db.SaveChangesAsync();
Check(m.Codigo == "M90" && m.Nombre == "Ejemplo" && p.Denominacion == "Piso 9",
    "Normalizacion al guardar async");
Check(!m.Activo && new Modulo().Activo && new Piso().Activo, "Activo=false preservado y true inicial");
m.Codigo = " m91 ";
db.SaveChanges();
Check(m.Codigo == "M91", "Normalizacion al guardar sincrono");
Console.WriteLine("Verificacion del modelo completada. No se conecto a SQL Server.");

sealed class SinEscritura : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
        InterceptionResult<int> result) => InterceptionResult<int>.SuppressWithResult(0);
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(0));
}
