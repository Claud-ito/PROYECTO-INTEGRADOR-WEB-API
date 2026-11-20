using System.Data;
using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        // Carga inicial explicita y atomica. Una base con ubicaciones se conserva completa.
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        if (await db.Modulos.AnyAsync(cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        db.Modulos.AddRange(
            new Modulo
            {
                Codigo = "M01", Nombre = "Modulo 1 (ejemplo)",
                Pisos = new List<Piso>
                {
                    new() { Denominacion = "Planta baja" },
                    new() { Denominacion = "Piso 1" }
                }
            },
            new Modulo
            {
                Codigo = "M02", Nombre = "Modulo 2 (ejemplo)",
                Pisos = new List<Piso>
                {
                    new() { Denominacion = "Planta baja" },
                    new() { Denominacion = "Piso 1" }
                }
            },
            new Modulo { Codigo = "M03", Nombre = "Modulo inactivo (ejemplo)", Activo = false });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
