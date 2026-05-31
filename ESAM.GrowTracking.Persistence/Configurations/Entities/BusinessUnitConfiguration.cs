using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class BusinessUnitConfiguration : AuditableEntityConfiguration<BusinessUnit>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<BusinessUnit> builder)
        {
            builder.ToTable("BusinessUnits");
            builder.HasKey(bu => bu.Id);
            builder.Property(bu => bu.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(bu => bu.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(bu => bu.Name).IsUnique();
            builder.Property(bu => bu.Abbreviation).IsRequired().HasMaxLength(10);
            builder.HasIndex(bu => bu.Abbreviation).IsUnique();
            builder.Property(bu => bu.WebSite).IsRequired().HasMaxLength(256);
            builder.HasIndex(bu => bu.WebSite).IsUnique();
            builder.Property(bu => bu.FoundingDate).IsRequired().HasColumnType("date");
            builder.Property(bu => bu.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(bu => bu.RecordVersion).IsRequired().IsRowVersion();
        }
    }
}