using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRE_EMERGENCIAS_WEB_API.Models;

namespace PRE_EMERGENCIAS_WEB_API.Data.Configurations;

public class PisoConfiguration : IEntityTypeConfiguration<Piso>
{
    public void Configure(EntityTypeBuilder<Piso> builder)
    {
        builder.ToTable("Pisos", table =>
            table.HasCheckConstraint("CK_Pisos_Denominacion_NoVacia",
                "LEN(LTRIM(RTRIM([Denominacion]))) > 0"));
        builder.HasKey(p => p.PisoId);
        builder.Property(p => p.Denominacion).IsRequired().HasMaxLength(50)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.Property(p => p.Activo).HasDefaultValue(true).HasSentinel(true);
        builder.HasIndex(p => new { p.ModuloId, p.Denominacion }).IsUnique();
        builder.HasOne(p => p.Modulo).WithMany(m => m.Pisos)
            .HasForeignKey(p => p.ModuloId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
