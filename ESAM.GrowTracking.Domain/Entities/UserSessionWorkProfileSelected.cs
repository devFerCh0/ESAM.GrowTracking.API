namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserSessionWorkProfileSelected
    {
        private UserSessionWorkProfileSelected() { }

        public int UserSessionId { get; private set; }

        public int UserId { get; private set; }

        public int WorkProfileId { get; private set; }

        public UserSession UserSession { get; private set; } = null!;

        public UserWorkProfile UserWorkProfile { get; private set; } = null!;

        public UserSessionRoleCampusSelected? UserSessionRoleCampusSelected { get; private set; }

        public UserSessionWorkProfileSelected(int userId, int workProfileId)
        {
            UserId = userId;
            WorkProfileId = workProfileId;
        }

        public void AddUserSessionId(int userSessionId)
        {
            UserSessionId = userSessionId;
        }
    }
}