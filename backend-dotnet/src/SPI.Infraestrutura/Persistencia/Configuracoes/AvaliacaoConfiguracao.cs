using System.Text.Json;
using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
{
    public void Configure(EntityTypeBuilder<Evaluation> builder)
    {
        builder.ToTable("evaluations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(x => x.AvaliadorId)
            .HasColumnName("avaliador_id")
            .IsRequired();

        builder.Property(x => x.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(x => x.FormTemplateId)
            .HasColumnName("form_template_id");

        builder.Property(x => x.Respostas)
            .HasColumnName("respostas")
            .HasConversion(
                value => JsonSerializer.Serialize(value, JsonSerializerOptions.Default),
                value => JsonSerializer.Deserialize<Dictionary<int, int>>(value, JsonSerializerOptions.Default) ?? new Dictionary<int, int>(),
                new ValueComparer<Dictionary<int, int>>(
                    (left, right) => JsonSerializer.Serialize(left, JsonSerializerOptions.Default) == JsonSerializer.Serialize(right, JsonSerializerOptions.Default),
                    value => JsonSerializer.Serialize(value, JsonSerializerOptions.Default).GetHashCode(),
                    value => value.ToDictionary(entry => entry.Key, entry => entry.Value)))
            .IsRequired();

        builder.Property(x => x.ScoreTotal)
            .HasColumnName("score_total")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.PesoTotal)
            .HasColumnName("peso_total")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Classificacao)
            .HasColumnName("classificacao")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DataAvaliacao)
            .HasColumnName("data_avaliacao")
            .IsRequired();

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Avaliacoes)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Avaliador)
            .WithMany(x => x.AvaliacoesRealizadas)
            .HasForeignKey(x => x.AvaliadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FormTemplate)
            .WithMany()
            .HasForeignKey(x => x.FormTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id");

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.AvaliadorId);
        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.FormTemplateId);
    }
}



