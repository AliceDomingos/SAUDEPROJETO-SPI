using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class FormQuestionOptionConfiguration : IEntityTypeConfiguration<FormQuestionOption>
{
    public void Configure(EntityTypeBuilder<FormQuestionOption> builder)
    {
        builder.ToTable("form_question_options");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FormQuestionId)
            .HasColumnName("form_question_id")
            .IsRequired();

        builder.Property(x => x.Valor)
            .HasColumnName("valor")
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(x => x.FormQuestion)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.FormQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.FormQuestionId);
    }
}
