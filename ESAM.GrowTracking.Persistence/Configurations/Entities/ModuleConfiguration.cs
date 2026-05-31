using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("Modules");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(m => m.Name).IsUnique();
            builder.Property(m => m.Description).IsRequired(false).HasMaxLength(250);
            builder.Property(m => m.IsDeleted).IsRequired().HasDefaultValue(false);
        }
    }
}