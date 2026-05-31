using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class WorkProfilePermissionConfiguration : AuditableEntityConfiguration<WorkProfilePermission>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<WorkProfilePermission> builder)
        {
            builder.ToTable("WorkProfilePermissions");
            builder.HasKey(wpp => new { wpp.WorkProfileId, wpp.PermissionId });
            builder.Property(wpp => wpp.HasAccess).IsRequired().HasDefaultValue(false);
            builder.Property(wpp => wpp.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(wpp => wpp.WorkProfile).WithMany(wp => wp.WorkProfilePermissions).HasForeignKey(wpp => wpp.WorkProfileId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(wpp => wpp.Permission).WithMany(p => p.WorkProfilePermissions).HasForeignKey(wpp => wpp.PermissionId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}