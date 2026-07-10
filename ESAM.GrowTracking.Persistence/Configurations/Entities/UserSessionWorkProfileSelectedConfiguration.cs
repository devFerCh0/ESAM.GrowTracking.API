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
            builder.HasKey(uswps => uswps.Id);
            builder.Property(uswps => uswps.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(uswps => uswps.UserSessionId).IsRequired();
            builder.Property(uswps => uswps.UserId).IsRequired();
            builder.Property(uswps => uswps.WorkProfileId).IsRequired();
            builder.Property(uswps => uswps.IsActive).IsRequired();
            builder.Property(uswps => uswps.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.HasIndex(uswps => new { uswps.UserSessionId, uswps.IsActive });
            builder.HasOne(uswps => uswps.UserSession).WithMany(us => us.UserSessionWorkProfilesSelected).HasForeignKey(uswps => uswps.UserSessionId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(uswps => uswps.UserWorkProfile).WithMany(uwp => uwp.UserSessionWorkProfilesSelected).HasForeignKey(uswps => new { uswps.UserId, uswps.WorkProfileId })
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }

    //internal sealed class UserSessionWorkProfileSelectedConfiguration : IEntityTypeConfiguration<UserSessionWorkProfileSelected>
    //{
    //    public void Configure(EntityTypeBuilder<UserSessionWorkProfileSelected> builder)
    //    {
    //        builder.ToTable("UserSessionWorkProfilesSelected");
    //        builder.HasKey(uswps => uswps.UserSessionId);
    //        builder.Property(uswps => uswps.UserSessionId).IsRequired().ValueGeneratedNever();
    //        builder.Property(uswps => uswps.UserId).IsRequired();
    //        builder.Property(uswps => uswps.WorkProfileId).IsRequired();
    //        builder.HasOne(uswps => uswps.UserSession).WithOne(us => us.UserSessionWorkProfileSelected)
    //            .HasForeignKey<UserSessionWorkProfileSelected>(uswps => uswps.UserSessionId).IsRequired().OnDelete(DeleteBehavior.Restrict);           
    //        builder.HasOne(uswps => uswps.UserWorkProfile).WithMany(uwp => uwp.UserSessionWorkProfilesSelected)
    //            .HasForeignKey(uswps => new { uswps.UserId, uswps.WorkProfileId }).IsRequired().OnDelete(DeleteBehavior.Restrict);
    //    }
    //}
}