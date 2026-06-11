using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class Module : IEntity<int>
    {
        private readonly List<Permission> _permissions = [];

        private Module() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public bool IsDeleted { get; private set; }

        public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

        public Module(int id, string name, string? description = null)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}