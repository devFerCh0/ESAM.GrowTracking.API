using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class WorkProfile : IEntity<int>
    {
        private readonly List<UserWorkProfile> _userWorkProfiles = [];
        private readonly List<WorkProfilePermission> _workProfilePermissions = [];

        private WorkProfile() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public WorkProfileType WorkProfileType { get; private set; }

        public bool IsDeleted { get; private set; }

        public IReadOnlyCollection<UserWorkProfile> UserWorkProfiles => _userWorkProfiles.AsReadOnly();

        public IReadOnlyCollection<WorkProfilePermission> WorkProfilePermissions => _workProfilePermissions.AsReadOnly();

        public WorkProfile(int id, string name, WorkProfileType workProfileType, string? description = null)
        {
            Id = id;
            Name = name;
            WorkProfileType = workProfileType;
            Description = description;
        }

        //public void Update(string name, string? description)
        //{
        //    Name = name;
        //    Description = description;
        //}

        //public void SoftDelete() => IsDeleted = true;

        //public void Restore() => IsDeleted = false;
    }
}