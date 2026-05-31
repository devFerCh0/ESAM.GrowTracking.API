using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserRoleCampusConfiguration : AuditableEntityConfiguration<UserRoleCampus>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserRoleCampus> builder)
        {
            builder.ToTable("UserRoleCampuses");
            builder.HasKey(urc => new { urc.UserId, urc.RoleId, urc.CampusId });
            builder.Property(urc => urc.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(urc => urc.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(urc => urc.User).WithMany(u => u.UserRoleCampuses).HasForeignKey(urc => urc.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(urc => urc.Role).WithMany(r => r.UserRoleCampuses).HasForeignKey(urc => urc.RoleId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(urc => urc.Campus).WithMany(c => c.UserRoleCampuses).HasForeignKey(urc => urc.CampusId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}