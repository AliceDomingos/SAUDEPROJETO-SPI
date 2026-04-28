using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(x => x.DataNascimento)
            .HasColumnName("data_nascimento")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Sexo)
            .HasColumnName("sexo")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Idade)
            .HasColumnName("idade");

        builder.Property(x => x.AvaliadorId)
            .HasColumnName("avaliador_id");

        builder.Property(x => x.NomeResponsavel)
            .HasColumnName("nome_responsavel")
            .HasMaxLength(200);

        builder.Property(x => x.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(200);

        builder.Property(x => x.Cep)
            .HasColumnName("cep")
            .HasMaxLength(8);

        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasMaxLength(2);

        builder.Property(x => x.Cidade)
            .HasColumnName("cidade")
            .HasMaxLength(120);

        builder.Property(x => x.Bairro)
            .HasColumnName("bairro")
            .HasMaxLength(120);

        builder.Property(x => x.Rua)
            .HasColumnName("rua")
            .HasMaxLength(200);

        builder.Property(x => x.Numero)
            .HasColumnName("numero")
            .HasMaxLength(30);

        builder.Property(x => x.Complemento)
            .HasColumnName("complemento")
            .HasMaxLength(200);

        builder.Property(x => x.Observacoes)
            .HasColumnName("observacoes")
            .HasMaxLength(2000);

        builder.Property(x => x.GroupId)
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.HasOne(x => x.Avaliador)
            .WithMany(x => x.PacientesCriados)
            .HasForeignKey(x => x.AvaliadorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Group)
            .WithMany(x => x.Patients)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.OrganizationId)
            .HasColumnName("organization_id");

        builder.HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.GroupId);
        builder.HasIndex(x => x.Cpf).IsUnique();
    }
}



