using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserDeviceConfiguration : AuditableEntityConfiguration<UserDevice>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserDevice> builder)
        {
            builder.ToTable("UserDevices");
            builder.HasKey(ud => ud.Id);
            builder.Property(ud => ud.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(ud => ud.UserId).IsRequired();
            builder.HasIndex(ud => ud.UserId);
            builder.Property(ud => ud.DeviceIdentifier).IsRequired().HasMaxLength(256);
            builder.HasIndex(ud => ud.DeviceIdentifier);
            builder.Property(ud => ud.DeviceName).IsRequired().HasMaxLength(100);
            builder.Property(ud => ud.ApiClientType).HasConversion<byte>().IsRequired();
            builder.HasIndex(ud => ud.ApiClientType);
            builder.Property(ud => ud.IsTrusted).IsRequired().HasDefaultValue(false);
            builder.Property(ud => ud.LastSeenAt).IsRequired(false);
            builder.Property(ud => ud.LastIp).IsRequired(false).HasMaxLength(50);
            builder.Property(ud => ud.LastUserAgent).IsRequired(false).HasMaxLength(512);
            builder.Property(ud => ud.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(ud => ud.FailedLoginCount).IsRequired().HasDefaultValue(0);
            builder.Property(ud => ud.LastFailedLoginAt).IsRequired(false);
            builder.Property(ud => ud.LockoutEndAt).IsRequired(false);
            builder.Property(ud => ud.LastLoginAt).IsRequired(false);
            builder.Property(ud => ud.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(ud => new { ud.UserId, ud.IsDeleted });
            builder.HasOne(ud => ud.User).WithMany(u => u.UserDevices).HasForeignKey(ud => ud.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}