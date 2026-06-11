using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class WorkProfilePermission : AuditableEntity
    {
        private WorkProfilePermission() { }

        public int WorkProfileId { get; private set; }

        public int PermissionId { get; private set; }

        public bool HasAccess { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public WorkProfile WorkProfile { get; private set; } = null!;

        public Permission Permission { get; private set; } = null!;

        public WorkProfilePermission(int workProfileId, int permissionId, bool hasAccess, int createdBy, DateTime? createdAt = null)
        {
            WorkProfileId = workProfileId;
            PermissionId = permissionId;
            HasAccess = hasAccess;
            SetCreatedAudit(createdBy, createdAt);
        }
    }
}