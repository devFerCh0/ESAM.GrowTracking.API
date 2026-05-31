using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class CampusConfiguration : AuditableEntityConfiguration<Campus>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Campus> builder)
        {
            builder.ToTable("Campuses");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(c => c.BusinessUnitId).IsRequired();
            builder.HasIndex(c => c.BusinessUnitId);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
            builder.HasIndex(c => c.Name).IsUnique();
            builder.Property(c => c.WebSite).IsRequired().HasMaxLength(256);
            builder.HasIndex(c => c.WebSite).IsUnique();
            builder.Property(c => c.FoundingDate).IsRequired().HasColumnType("date");
            builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(c => c.RecordVersion).IsRequired().IsRowVersion();
            builder.HasOne(c => c.BusinessUnit).WithMany(bu => bu.Campuses).HasForeignKey(c => c.BusinessUnitId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }
}