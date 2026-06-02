using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class BlacklistedRefreshTokenConfiguration : IEntityTypeConfiguration<BlacklistedRefreshToken>
    {
        public void Configure(EntityTypeBuilder<BlacklistedRefreshToken> builder)
        {
            builder.ToTable("BlacklistedRefreshTokens");
            builder.HasKey(brt => brt.Id);
            builder.Property(brt => brt.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(brt => brt.UserSessionRefreshTokenId).IsRequired();
            builder.HasIndex(brt => brt.UserSessionRefreshTokenId).IsUnique();
            builder.Property(brt => brt.Identifier).IsRequired().HasMaxLength(256);
            builder.HasIndex(brt => brt.Identifier);
            builder.Property(brt => brt.ExpiresAt).IsRequired();
            builder.HasIndex(brt => brt.ExpiresAt);
            builder.Property(brt => brt.BlacklistedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(brt => brt.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(brt => brt.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(brt => brt.CreatedBy).IsRequired();
            builder.Property(brt => brt.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(brt => brt.UserSessionRefreshToken).WithMany(ust => ust.BlacklistedRefreshTokens).HasForeignKey(brt => brt.UserSessionRefreshTokenId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}