using Microsoft.EntityFrameworkCore;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<Piso> Pisos => Set<Piso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        NormalizarUbicaciones();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        NormalizarUbicaciones();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void NormalizarUbicaciones()
    {
        foreach (var entry in ChangeTracker.Entries<Modulo>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.Codigo = entry.Entity.Codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            entry.Entity.Nombre = entry.Entity.Nombre?.Trim() ?? string.Empty;
        }

        foreach (var entry in ChangeTracker.Entries<Piso>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            entry.Entity.Denominacion = entry.Entity.Denominacion?.Trim() ?? string.Empty;
    }
}
