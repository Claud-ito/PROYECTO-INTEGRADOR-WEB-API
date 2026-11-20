using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Data.Configurations;

public class ModuloConfiguration : IEntityTypeConfiguration<Modulo>
{
    public void Configure(EntityTypeBuilder<Modulo> builder)
    {
        builder.ToTable("Modulos", table =>
        {
            table.HasCheckConstraint("CK_Modulos_Codigo_NoVacio", "LEN(LTRIM(RTRIM([Codigo]))) > 0");
            table.HasCheckConstraint("CK_Modulos_Nombre_NoVacio", "LEN(LTRIM(RTRIM([Nombre]))) > 0");
        });
        builder.HasKey(m => m.ModuloId);
        builder.Property(m => m.Codigo).IsRequired().HasMaxLength(20)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Activo).HasDefaultValue(true).HasSentinel(true);
        builder.HasIndex(m => m.Codigo).IsUnique();
    }
}
