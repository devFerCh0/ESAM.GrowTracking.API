using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class RoleConfiguration : AuditableEntityConfiguration<Role>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(r => r.Name).IsUnique();
            builder.Property(r => r.Description).IsRequired(false).HasMaxLength(250);
            builder.Property(r => r.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(r => r.RecordVersion).IsRequired().IsRowVersion();
        }
    }
}