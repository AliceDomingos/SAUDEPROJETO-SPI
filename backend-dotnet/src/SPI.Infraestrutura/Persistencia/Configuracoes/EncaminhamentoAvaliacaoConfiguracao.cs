using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class EvaluationReferralConfiguration : IEntityTypeConfiguration<EvaluationReferral>
{
    public void Configure(EntityTypeBuilder<EvaluationReferral> builder)
    {
        builder.ToTable("evaluation_referrals");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EvaluationId)
            .HasColumnName("evaluation_id")
            .IsRequired();

        builder.Property(x => x.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(x => x.Encaminhado)
            .HasColumnName("encaminhado")
            .IsRequired();

        builder.Property(x => x.SpecialistId)
            .HasColumnName("specialist_id");

        builder.Property(x => x.SpecialistNome)
            .HasColumnName("specialist_nome")
            .HasMaxLength(200);

        builder.Property(x => x.Especialidade)
            .HasColumnName("especialidade")
            .HasMaxLength(120);

        builder.Property(x => x.CustoEstimado)
            .HasColumnName("custo_estimado")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.Property(x => x.CriadoPorUsuarioId)
            .HasColumnName("criado_por_usuario_id")
            .IsRequired();

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id");

        builder.HasOne(x => x.Evaluation)
            .WithOne(x => x.Referral)
            .HasForeignKey<EvaluationReferral>(x => x.EvaluationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Specialist)
            .WithMany()
            .HasForeignKey(x => x.SpecialistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EvaluationId).IsUnique();
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.SpecialistId);
        builder.HasIndex(x => x.Especialidade);
    }
}
