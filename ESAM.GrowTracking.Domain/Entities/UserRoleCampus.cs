using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserRoleCampus : AuditableEntity
    {
        private readonly List<UserSessionRoleCampusSelected> _userSessionRoleCampusesSelected = [];

        private UserRoleCampus() { }

        public int UserId { get; private set; }

        public int RoleId { get; private set; }

        public int CampusId { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public Role Role { get; private set; } = null!;

        public Campus Campus { get; private set; } = null!;

        public IReadOnlyCollection<UserSessionRoleCampusSelected> UserSessionRoleCampusesSelected => _userSessionRoleCampusesSelected.AsReadOnly();

        public UserRoleCampus(int userId, int roleId, int campusId, int createdBy, DateTime? createdAt = null)
        {
            UserId = userId;
            RoleId = roleId;
            CampusId = campusId;
            SetCreatedAudit(createdBy, createdAt);
        }
    }
}