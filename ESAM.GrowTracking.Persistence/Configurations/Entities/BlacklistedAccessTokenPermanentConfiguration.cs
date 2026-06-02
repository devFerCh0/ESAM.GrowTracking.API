using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class BlacklistedAccessTokenPermanentConfiguration : IEntityTypeConfiguration<BlacklistedAccessTokenPermanent>
    {
        public void Configure(EntityTypeBuilder<BlacklistedAccessTokenPermanent> builder)
        {
            builder.ToTable("BlacklistedAccessTokensPermanent");
            builder.HasKey(batp => batp.Id);
            builder.Property(batp => batp.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(batp => batp.UserSessionId).IsRequired();
            builder.HasIndex(batp => batp.UserSessionId);
            builder.Property(batp => batp.Jti).IsRequired().HasMaxLength(256);
            builder.HasIndex(batp => batp.Jti);
            builder.Property(batp => batp.ExpiresAt).IsRequired();
            builder.HasIndex(batp => batp.ExpiresAt);
            builder.Property(batp => batp.BlacklistedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batp => batp.Reason).IsRequired(false).HasMaxLength(250);
            builder.Property(batp => batp.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(batp => batp.CreatedBy).IsRequired();
            builder.Property(batp => batp.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(batp => new { batp.UserSessionId, batp.ExpiresAt });
            builder.HasOne(batp => batp.UserSession).WithMany(us => us.BlacklistedAccessTokensPermanent).HasForeignKey(batp => batp.UserSessionId).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}