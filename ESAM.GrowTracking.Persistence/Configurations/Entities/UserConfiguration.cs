using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserConfiguration : AuditableEntityConfiguration<User>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).IsRequired().ValueGeneratedNever();
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.HasIndex(u => u.Username).IsUnique();
            builder.Property(u => u.NormalizedUserName).IsRequired().HasMaxLength(50);
            builder.HasIndex(u => u.NormalizedUserName).IsUnique();
            builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.NormalizedEmail).IsRequired().HasMaxLength(100);
            builder.HasIndex(u => u.NormalizedEmail).IsUnique();
            builder.Property(u => u.Salt).IsRequired().HasMaxLength(128);
            builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
            builder.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(36).HasDefaultValueSql("CONVERT(varchar(36), NEWID())");
            builder.Property(u => u.TokenVersion).IsRequired().HasDefaultValue(0);
            builder.Property(u => u.IsEmailConfirmed).IsRequired().HasDefaultValue(false);
            builder.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(u => u.LockoutEndAt).IsRequired(false);
            builder.Property(u => u.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(u => u.Person).WithOne(p => p.User).HasForeignKey<User>(u => u.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}