using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class RolePermissionConfiguration : AuditableEntityConfiguration<RolePermission>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            builder.Property(rp => rp.HasAccess).IsRequired().HasDefaultValue(false);
            builder.Property(rp => rp.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}