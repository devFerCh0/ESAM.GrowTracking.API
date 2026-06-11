using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class RolePermission : AuditableEntity
    {
        private RolePermission() { }

        public int RoleId { get; private set; }

        public int PermissionId { get; private set; }

        public bool HasAccess { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public Role Role { get; private set; } = null!;

        public Permission Permission { get; private set; } = null!;

        public RolePermission(int roleId, int permissionId, bool hasAccess, int createdBy, DateTime? createdAt = null)
        {
            RoleId = roleId;
            PermissionId = permissionId;
            HasAccess = hasAccess;
            SetCreatedAudit(createdBy, createdAt);
        }
    }
}