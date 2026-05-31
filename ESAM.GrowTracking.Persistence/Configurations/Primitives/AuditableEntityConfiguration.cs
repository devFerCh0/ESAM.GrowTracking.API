using ESAM.GrowTracking.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Primitives
{
    internal abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : AuditableEntity
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(e => e.CreatedBy).IsRequired();
            builder.Property(e => e.UpdatedAt).IsRequired(false);
            builder.Property(e => e.UpdatedBy).IsRequired(false);
            ConfigureEntity(builder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
    }
}