using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class FormQuestionConfiguration : IEntityTypeConfiguration<FormQuestion>
{
    public void Configure(EntityTypeBuilder<FormQuestion> builder)
    {
        builder.ToTable("form_questions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FormTemplateId)
            .HasColumnName("form_template_id")
            .IsRequired();

        builder.Property(x => x.Texto)
            .HasColumnName("texto")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Peso)
            .HasColumnName("peso")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Ordem)
            .HasColumnName("ordem")
            .IsRequired();

        builder.Property(x => x.Ativa)
            .HasColumnName("ativa")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(x => x.FormTemplateId);
    }
}



