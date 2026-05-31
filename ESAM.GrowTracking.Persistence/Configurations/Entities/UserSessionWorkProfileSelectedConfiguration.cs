using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserSessionWorkProfileSelectedConfiguration : IEntityTypeConfiguration<UserSessionWorkProfileSelected>
    {
        public void Configure(EntityTypeBuilder<UserSessionWorkProfileSelected> builder)
        {
            builder.ToTable("UserSessionWorkProfilesSelected");
            builder.HasKey(uswps => uswps.UserSessionId);
            builder.Property(uswps => uswps.UserSessionId).IsRequired().ValueGeneratedNever();
            builder.Property(uswps => uswps.UserId).IsRequired();
            builder.Property(uswps => uswps.WorkProfileId).IsRequired();
            builder.HasOne(uswps => uswps.UserSession).WithOne(us => us.UserSessionWorkProfileSelected)
                .HasForeignKey<UserSessionWorkProfileSelected>(uswps => uswps.UserSessionId).IsRequired().OnDelete(DeleteBehavior.Restrict);           
            builder.HasOne(uswps => uswps.UserWorkProfile).WithMany(uwp => uwp.UserSessionWorkProfilesSelected)
                .HasForeignKey(uswps => new { uswps.UserId, uswps.WorkProfileId }).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}