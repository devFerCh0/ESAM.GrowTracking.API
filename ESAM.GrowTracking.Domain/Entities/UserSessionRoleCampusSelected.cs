using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserSessionRoleCampusSelected : IEntity<int>
    {
        private UserSessionRoleCampusSelected() { }

        public int Id { get; private set; }

        public int UserSessionWorkProfileSelectedId { get; private set; }

        public int UserId { get; private set; }

        public int RoleId { get; private set; }

        public int CampusId { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public UserSessionWorkProfileSelected UserSessionWorkProfileSelected { get; private set; } = null!;

        public UserRoleCampus UserRoleCampus { get; private set; } = null!;

        public UserSessionRoleCampusSelected(int userId, int roleId, int campusId, DateTime? createdAt = null)
        {
            UserId = userId;
            RoleId = roleId;
            CampusId = campusId;
            IsActive = true;
            CreatedAt = createdAt ?? DateTime.UtcNow;
        }

        public UserSessionRoleCampusSelected(int userSessionWorkProfileSelectedId, int userId, int roleId, int campusId, DateTime? createdAt = null)
        {
            UserSessionWorkProfileSelectedId = userSessionWorkProfileSelectedId;
            UserId = userId;
            RoleId = roleId;
            CampusId = campusId;
            IsActive = true;
            CreatedAt = createdAt ?? DateTime.UtcNow;
        }

        public void AddUserSessionWorkProfileSelectedId(int userSessionWorkProfileSelectedId)
        {
            UserSessionWorkProfileSelectedId = userSessionWorkProfileSelectedId;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }

    //public sealed class UserSessionRoleCampusSelected
    //{
    //    private UserSessionRoleCampusSelected() { }

    //    public int UserSessionId { get; private set; }

    //    public int UserId { get; private set; }

    //    public int RoleId { get; private set; }

    //    public int CampusId { get; private set; }

    //    public UserSessionWorkProfileSelected UserSessionWorkProfileSelected { get; private set; } = null!;

    //    public UserRoleCampus UserRoleCampus { get; private set; } = null!;

    //    public UserSessionRoleCampusSelected(int userId, int roleId, int campusId)
    //    {
    //        UserId = userId;
    //        RoleId = roleId;
    //        CampusId = campusId;
    //    }

    //    public void AddUserSessionId(int userSessionId)
    //    {
    //        UserSessionId = userSessionId;
    //    }
    //}
}