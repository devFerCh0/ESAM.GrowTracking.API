using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.ModuleId).IsRequired();
            builder.HasIndex(p => p.ModuleId);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(p => p.Name).IsUnique();
            builder.Property(p => p.Description).IsRequired(false).HasMaxLength(250);
            builder.Property(p => p.Code).IsRequired(false).HasMaxLength(50);
            builder.HasIndex(p => p.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
            builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.HasOne(p => p.Module).WithMany(m => m.Permissions).HasForeignKey(p => p.ModuleId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}