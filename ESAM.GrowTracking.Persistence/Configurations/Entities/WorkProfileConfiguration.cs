using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class WorkProfileConfiguration : IEntityTypeConfiguration<WorkProfile>
    {
        public void Configure(EntityTypeBuilder<WorkProfile> builder)
        {
            builder.ToTable("WorkProfiles");
            builder.HasKey(wp => wp.Id);
            builder.Property(wp => wp.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(wp => wp.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(wp => wp.Name).IsUnique();
            builder.Property(wp => wp.Description).IsRequired(false).HasMaxLength(250);
            builder.Property(wp => wp.WorkProfileType).HasConversion<byte>().IsRequired().HasDefaultValue(WorkProfileType.None);
            builder.HasIndex(wp => wp.WorkProfileType);
            builder.Property(wp => wp.IsDeleted).IsRequired().HasDefaultValue(false);
        }
    }
}