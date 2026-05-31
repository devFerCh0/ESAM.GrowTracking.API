using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserSessionRefreshTokenConfiguration : AuditableEntityConfiguration<UserSessionRefreshToken>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserSessionRefreshToken> builder)
        {
            builder.ToTable("UserSessionRefreshTokens");
            builder.HasKey(ust => ust.Id);
            builder.Property(ust => ust.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(ust => ust.UserSessionId).IsRequired();
            builder.HasIndex(ust => ust.UserSessionId);
            builder.Property(ust => ust.Identifier).IsRequired().HasMaxLength(256);
            builder.HasIndex(ust => ust.Identifier).IsUnique();
            builder.Property(ust => ust.Salt).IsRequired().HasMaxLength(256);
            builder.Property(ust => ust.TokenHash).IsRequired().HasMaxLength(512);
            builder.Property(ust => ust.ExpiresAt).IsRequired();
            builder.HasIndex(ust => ust.ExpiresAt);
            builder.Property(ust => ust.LastUsedAt).IsRequired(false);
            builder.Property(ust => ust.RotationCount).IsRequired().HasDefaultValue(0);
            builder.Property(ust => ust.ReplacedByUserSessionRefreshTokenId).IsRequired(false);
            builder.HasIndex(ust => ust.ReplacedByUserSessionRefreshTokenId).IsUnique().HasFilter("[ReplacedByUserSessionRefreshTokenId] IS NOT NULL");
            builder.Property(ust => ust.IsRevoked).IsRequired().HasDefaultValue(false);
            builder.Property(ust => ust.RevokedAt).IsRequired(false);
            builder.Property(ust => ust.RevokedReason).IsRequired(false).HasMaxLength(512);
            builder.Property(ust => ust.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(ust => new { ust.UserSessionId, ust.IsRevoked });
            builder.HasOne(ust => ust.UserSession).WithMany(us => us.UserSessionRefreshTokens).HasForeignKey(ust => ust.UserSessionId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(ust => ust.ReplacedByUserSessionRefreshToken).WithOne(ust => ust.ReplacesUserSessionRefreshToken)
                .HasForeignKey<UserSessionRefreshToken>(ust => ust.ReplacedByUserSessionRefreshTokenId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}