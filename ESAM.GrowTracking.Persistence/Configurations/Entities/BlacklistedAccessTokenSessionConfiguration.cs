using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class BlacklistedAccessTokenSessionConfiguration : IEntityTypeConfiguration<BlacklistedAccessTokenSession>
    {
        public void Configure(EntityTypeBuilder<BlacklistedAccessTokenSession> builder)
        {
            builder.ToTable("BlacklistedAccessTokensSession");
            builder.HasKey(bats => bats.Id);
            builder.Property(bats => bats.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(bats => bats.UserSessionId).IsRequired();
            builder.HasIndex(bats => bats.UserSessionId);
            builder.Property(bats => bats.Jti).IsRequired().HasMaxLength(256);
            builder.HasIndex(bats => bats.Jti);
            builder.Property(bats => bats.ExpiresAt).IsRequired();
            builder.HasIndex(bats => bats.ExpiresAt);
            builder.Property(bats => bats.BlacklistedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(bats => bats.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(bats => bats.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(bats => bats.CreatedBy).IsRequired();
            builder.Property(bats => bats.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(batp => new { batp.UserSessionId, batp.ExpiresAt });
            builder.HasOne(bats => bats.UserSession).WithMany(us => us.BlacklistedAccessTokensSession).HasForeignKey(bats => bats.UserSessionId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}