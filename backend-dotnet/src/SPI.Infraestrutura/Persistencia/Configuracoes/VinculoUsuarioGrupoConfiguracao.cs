using SPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SPI.Infrastructure.Data.Persistence.Configurations;

public sealed class UserGroupMembershipConfiguration : IEntityTypeConfiguration<UserGroupMembership>
{
    public void Configure(EntityTypeBuilder<UserGroupMembership> builder)
    {
        builder.ToTable("user_group_memberships");

        builder.HasKey(x => new { x.UserId, x.GroupId });

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.GroupId).HasColumnName("group_id");

        builder.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.GroupMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Group)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.GroupId);
    }
}



