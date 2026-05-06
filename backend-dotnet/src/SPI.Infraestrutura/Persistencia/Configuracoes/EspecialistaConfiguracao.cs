using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class SpecialistConfiguration : IEntityTypeConfiguration<Specialist>
{
    public void Configure(EntityTypeBuilder<Specialist> builder)
    {
        builder.ToTable("specialists");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Especialidade)
            .HasColumnName("especialidade")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.CustoConsulta)
            .HasColumnName("custo_consulta")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id");

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Nome);
        builder.HasIndex(x => x.Especialidade);
        builder.HasIndex(x => x.OrganizationId);
    }
}
