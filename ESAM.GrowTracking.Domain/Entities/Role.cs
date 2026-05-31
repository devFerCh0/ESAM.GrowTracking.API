using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class Role : AuditableEntity, IEntity<int>
    {
        private readonly List<RolePermission> _rolePermissions = [];
        private readonly List<UserRoleCampus> _userRoleCampuses = [];

        private Role() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        public IReadOnlyCollection<UserRoleCampus> UserRoleCampuses => _userRoleCampuses.AsReadOnly();

        public Role(int id, string name, int createdBy, string? description = null, DateTime? createdAt = null)
        {
            Id = id;
            Name = name;
            Description = description;
            SetCreatedAudit(createdBy, createdAt);
        }

        //public void Update(string name, string? description, int updatedBy)
        //{
        //    Name = name;
        //    Description = description;
        //    SetUpdatedAudit(updatedBy);
        //}

        //public void SoftDelete(int updatedBy)
        //{
        //    IsDeleted = true;
        //    SetUpdatedAudit(updatedBy);
        //}

        //public void Restore(int updatedBy)
        //{
        //    IsDeleted = false;
        //    SetUpdatedAudit(updatedBy);
        //}
    }
}