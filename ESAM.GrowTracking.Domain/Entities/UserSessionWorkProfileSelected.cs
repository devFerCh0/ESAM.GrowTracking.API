using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserSessionWorkProfileSelected : IEntity<int>
    {
        private readonly List<UserSessionRoleCampusSelected> _userSessionRoleCampusesSelected = [];

        private UserSessionWorkProfileSelected() { }

        public int Id { get; private set; }

        public int UserSessionId { get; private set; }

        public int UserId { get; private set; }

        public int WorkProfileId { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public UserSession UserSession { get; private set; } = null!;

        public UserWorkProfile UserWorkProfile { get; private set; } = null!;

        public IReadOnlyCollection<UserSessionRoleCampusSelected> UserSessionRoleCampusesSelected => _userSessionRoleCampusesSelected.AsReadOnly();

        public UserSessionWorkProfileSelected(int userId, int workProfileId, DateTime? createdAt = null)
        {
            UserId = userId;
            WorkProfileId = workProfileId;
            IsActive = true;
            CreatedAt = createdAt ?? DateTime.UtcNow;
        }

        public void AddUserSessionId(int userSessionId)
        {
            UserSessionId = userSessionId;
        }
    }

    //public sealed class UserSessionWorkProfileSelected
    //{
    //    private UserSessionWorkProfileSelected() { }

    //    public int UserSessionId { get; private set; }

    //    public int UserId { get; private set; }

    //    public int WorkProfileId { get; private set; }

    //    public UserSession UserSession { get; private set; } = null!;

    //    public UserWorkProfile UserWorkProfile { get; private set; } = null!;

    //    public UserSessionRoleCampusSelected UserSessionRoleCampusSelected { get; private set; } = null!;

    //    public UserSessionWorkProfileSelected(int userId, int workProfileId)
    //    {
    //        UserId = userId;
    //        WorkProfileId = workProfileId;
    //    }

    //    public void AddUserSessionId(int userSessionId)
    //    {
    //        UserSessionId = userSessionId;
    //    }
    //}
}