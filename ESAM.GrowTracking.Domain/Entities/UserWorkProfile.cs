using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserWorkProfile : AuditableEntity
    {
        private readonly List<UserSessionWorkProfileSelected> _userSessionWorkProfilesSelected = [];

        private UserWorkProfile() { }

        public int UserId { get; private set; }

        public int WorkProfileId { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public WorkProfile WorkProfile { get; private set; } = null!;

        public IReadOnlyCollection<UserSessionWorkProfileSelected> UserSessionWorkProfilesSelected => _userSessionWorkProfilesSelected.AsReadOnly();

        public UserWorkProfile(int userId, int workProfileId, int createdBy, DateTime? createdAt = null)
        {
            UserId = userId;
            WorkProfileId = workProfileId;
            SetCreatedAudit(createdBy, createdAt);
        }

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