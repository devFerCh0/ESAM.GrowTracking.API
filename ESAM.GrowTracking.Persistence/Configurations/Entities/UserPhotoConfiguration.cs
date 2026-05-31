using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserPhotoConfiguration : AuditableEntityConfiguration<UserPhoto>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<UserPhoto> builder)
        {
            builder.ToTable("UserPhotos");
            builder.HasKey(up => up.Id);
            builder.Property(up => up.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(up => up.UserId).IsRequired();
            builder.HasIndex(up => up.UserId);
            builder.Property(up => up.Photo).IsRequired().HasMaxLength(512);
            builder.Property(up => up.IsDefault).IsRequired().HasDefaultValue(false);
            builder.Property(up => up.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(up => up.RecordVersion).IsRequired().IsRowVersion();
            builder.HasIndex(up => new { up.UserId, up.IsDefault }).HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0").IsUnique();
            builder.HasOne(up => up.User).WithMany(u => u.UserPhotos).HasForeignKey(up => up.UserId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}