namespace ESAM.GrowTracking.Domain.Primitives
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; private set; }

        public int CreatedBy { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public int? UpdatedBy { get; private set; }

        protected void SetCreatedAudit(int createdBy, DateTime? createdAt = null)
        {
            CreatedAt = createdAt ?? DateTime.UtcNow;
            CreatedBy = createdBy;
        }

        protected void SetUpdatedAudit(int updatedBy, DateTime? updatedAt = null)
        {
            UpdatedAt = updatedAt ?? DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
    }
}