using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class Permission : IEntity<int>
    {
        private readonly List<RolePermission> _rolePermissions = [];
        private readonly List<WorkProfilePermission> _workProfilePermissions = [];

        private Permission() { }

        public int Id { get; private set; }

        public int ModuleId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public string? Code { get; private set; }

        public bool IsDeleted { get; private set; }

        public Module Module { get; private set; } = null!;

        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        public IReadOnlyCollection<WorkProfilePermission> WorkProfilePermissions => _workProfilePermissions.AsReadOnly();

        public Permission(int id, int moduleId, string name, string? description = null, string? code = null)
        {
            Id = id;
            ModuleId = moduleId;
            Name = name;
            Description = description;
            Code = code;
        }
    }
}