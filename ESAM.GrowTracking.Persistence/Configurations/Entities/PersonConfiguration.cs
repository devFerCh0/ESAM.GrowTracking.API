using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Configurations.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class PersonConfiguration : AuditableEntityConfiguration<Person>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("People");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            builder.HasIndex(p => p.FirstName);
            builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            builder.HasIndex(p => p.LastName);
            builder.Property(p => p.SecondLastName).IsRequired(false).HasMaxLength(100);
            builder.HasIndex(p => p.SecondLastName);
            builder.Property(p => p.IdentityDocument).IsRequired().HasMaxLength(50);
            builder.HasIndex(p => p.IdentityDocument).IsUnique();
            builder.Property(p => p.IdentityDocumentType).HasConversion<byte>().IsRequired();
            builder.HasIndex(p => p.IdentityDocumentType);
            builder.Property(p => p.Gender).HasConversion<byte>().IsRequired();
            builder.HasIndex(p => p.Gender);
            builder.Property(p => p.MaritalStatus).HasConversion<byte>().IsRequired();
            builder.HasIndex(p => p.MaritalStatus);
            builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
            builder.Property(p => p.RecordVersion).IsRequired().IsRowVersion();
        }
    }
}