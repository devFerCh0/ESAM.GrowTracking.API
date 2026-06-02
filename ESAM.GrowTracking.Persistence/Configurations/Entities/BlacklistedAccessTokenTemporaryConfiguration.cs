using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class BlacklistedAccessTokenTemporaryConfiguration : IEntityTypeConfiguration<BlacklistedAccessTokenTemporary>
    {
        public void Configure(EntityTypeBuilder<BlacklistedAccessTokenTemporary> builder)
        {
            builder.ToTable("BlacklistedAccessTokensTemporary");

            builder.HasKey(batt => batt.Id);
            builder.Property(batt => batt.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(batt => batt.UserId).IsRequired();
            builder.HasIndex(batt => batt.UserId);
            builder.Property(batt => batt.Jti).IsRequired().HasMaxLength(256);
            builder.HasIndex(batt => batt.Jti);
            builder.Property(batt => batt.ExpiresAt).IsRequired();
            builder.HasIndex(batt => batt.ExpiresAt);
            builder.Property(batt => batt.BlacklistedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batt => batt.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(batt => batt.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batt => batt.CreatedBy).IsRequired();
            builder.Property(batt => batt.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(batt => new { batt.UserId, batt.ExpiresAt });
            builder.HasOne(batt => batt.User).WithMany(u => u.BlacklistedAccessTokensTemporary).HasForeignKey(batt => batt.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}