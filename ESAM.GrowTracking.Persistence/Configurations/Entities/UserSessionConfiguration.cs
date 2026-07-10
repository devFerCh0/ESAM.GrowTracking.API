using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserSessionConfiguration : AuditableEntityConfiguration<UserSession>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("UserSessions");
            builder.HasKey(us => us.Id);
            builder.Property(us => us.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(us => us.UserId).IsRequired();
            builder.HasIndex(us => us.UserId);
            builder.Property(us => us.UserDeviceId).IsRequired();
            builder.HasIndex(us => us.UserDeviceId);
            builder.Property(us => us.IpAddress).IsRequired(false).HasMaxLength(50);
            builder.Property(us => us.UserAgent).IsRequired(false).HasMaxLength(512);
            builder.Property(us => us.ExpiresAt).IsRequired();
            builder.HasIndex(us => us.ExpiresAt);
            builder.Property(us => us.AbsoluteExpiresAt).IsRequired();
            builder.HasIndex(us => us.AbsoluteExpiresAt);
            builder.Property(us => us.LastActivityAt).IsRequired(false);
            builder.Property(us => us.IsRevoked).IsRequired().HasDefaultValue(false);
            builder.Property(us => us.RevokedAt).IsRequired(false);
            builder.Property(us => us.RevokedReason).IsRequired(false).HasMaxLength(512);
            builder.Property(us => us.IsPersistent).IsRequired();
            builder.Property(us => us.ClosedByUserId).IsRequired(false);
            builder.HasIndex(us => us.ClosedByUserId);
            builder.Property(us => us.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(us => new { us.UserId, us.IsRevoked });
            builder.HasIndex(us => new { us.UserDeviceId, us.IsRevoked });
            builder.HasOne(us => us.User).WithMany(u => u.UserSessions).HasForeignKey(us => us.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(us => us.ClosedByUser).WithMany(u => u.SessionsClosedByUser).HasForeignKey(us => us.ClosedByUserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(us => us.UserDevice).WithMany(ud => ud.UserSessions).HasForeignKey(us => us.UserDeviceId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }

    //internal sealed class UserSessionConfiguration : AuditableEntityConfiguration<UserSession>
    //{
    //    protected override void ConfigureEntity(EntityTypeBuilder<UserSession> builder)
    //    {
    //        builder.ToTable("UserSessions");
    //        builder.HasKey(us => us.Id);
    //        builder.Property(us => us.Id).IsRequired().ValueGeneratedOnAdd();
    //        builder.Property(us => us.UserId).IsRequired();
    //        builder.HasIndex(us => us.UserId);
    //        builder.Property(us => us.UserDeviceId).IsRequired();
    //        builder.HasIndex(us => us.UserDeviceId);
    //        builder.Property(us => us.IpAddress).IsRequired(false).HasMaxLength(50);
    //        builder.Property(us => us.UserAgent).IsRequired(false).HasMaxLength(512);
    //        builder.Property(us => us.ExpiresAt).IsRequired();
    //        builder.HasIndex(us => us.ExpiresAt);
    //        builder.Property(us => us.AbsoluteExpiresAt).IsRequired();
    //        builder.HasIndex(us => us.AbsoluteExpiresAt);
    //        builder.Property(us => us.LastActivityAt).IsRequired(false);
    //        builder.Property(us => us.IsRevoked).IsRequired().HasDefaultValue(false);
    //        builder.Property(us => us.RevokedAt).IsRequired(false);
    //        builder.Property(us => us.RevokedReason).IsRequired(false).HasMaxLength(512);
    //        builder.Property(us => us.IsPersistent).IsRequired();
    //        builder.Property(us => us.ClosedByUserId).IsRequired(false);
    //        builder.HasIndex(us => us.ClosedByUserId);
    //        builder.Property(us => us.RecordVersion).IsRequired().IsRowVersion();
    //        builder.HasIndex(us => new { us.UserId, us.IsRevoked });
    //        builder.HasIndex(us => new { us.UserDeviceId, us.IsRevoked });
    //        builder.HasOne(us => us.User).WithMany(u => u.UserSessions).HasForeignKey(us => us.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    //        builder.HasOne(us => us.ClosedByUser).WithMany(u => u.SessionsClosedByUser).HasForeignKey(us => us.ClosedByUserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
    //        builder.HasOne(us => us.UserDevice).WithMany(ud => ud.UserSessions).HasForeignKey(us => us.UserDeviceId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    //    }
    //}
}