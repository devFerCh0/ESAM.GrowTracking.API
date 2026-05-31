using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserWorkProfileConfiguration : AuditableEntityConfiguration<UserWorkProfile>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserWorkProfile> builder)
        {
            builder.ToTable("UserWorkProfiles");
            builder.HasKey(uwp => new { uwp.UserId, uwp.WorkProfileId });
            builder.Property(uwp => uwp.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(uwp => uwp.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(uwp => uwp.User).WithMany(u => u.UserWorkProfiles).HasForeignKey(uwp => uwp.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uwp => uwp.WorkProfile).WithMany(wp => wp.UserWorkProfiles).HasForeignKey(uwp => uwp.WorkProfileId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}