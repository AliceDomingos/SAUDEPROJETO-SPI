using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class FormClassificationRangeConfiguration : IEntityTypeConfiguration<FormClassificationRange>
{
    public void Configure(EntityTypeBuilder<FormClassificationRange> builder)
    {
        builder.ToTable("form_classification_ranges");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FormTemplateId)
            .HasColumnName("form_template_id")
            .IsRequired();

        builder.Property(x => x.ScoreMin)
            .HasColumnName("score_min")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.ScoreMax)
            .HasColumnName("score_max")
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Rotulo)
            .HasColumnName("rotulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(x => x.FormTemplate)
            .WithMany(x => x.ClassificationRanges)
            .HasForeignKey(x => x.FormTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FormTemplateId);
    }
}
