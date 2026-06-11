namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserSessionRoleCampusSelected
    {
        private UserSessionRoleCampusSelected() { }

        public int UserSessionId { get; private set; }

        public int UserId { get; private set; }

        public int RoleId { get; private set; }

        public int CampusId { get; private set; }

        public UserSessionWorkProfileSelected UserSessionWorkProfileSelected { get; private set; } = null!;

        public UserRoleCampus UserRoleCampus { get; private set; } = null!;

        //public UserSessionRoleCampusSelected(int userId, int roleId, int campusId)
        //{
        //    UserId = userId;
        //    RoleId = roleId;
        //    CampusId = campusId;
        //}

        //public void AddUserSessionId(int userSessionId)
        //{
        //    UserSessionId = userSessionId;
        //}
    }
}