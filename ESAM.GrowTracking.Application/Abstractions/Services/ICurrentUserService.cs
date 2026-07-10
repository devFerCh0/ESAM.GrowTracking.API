using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }

        AccessTokenType? AccessTokenType { get; }

        int? UserId { get; }

        string? SecurityStamp { get; }

        int? TokenVersion { get; }

        string? Jti { get; }

        DateTime? AccessTokenExpiration { get; }

        int? UserDeviceId { get; }

        int? UserSessionId { get; }

        bool? IsPersistent { get; }

        int? WorkProfileSelectedId { get; }

        int? WorkProfileId { get; }

        WorkProfileType? WorkProfileType { get; }

        int? RoleCampusSelectedId { get; }

        int? RoleId { get; }

        int? CampusId { get; }

        IReadOnlyList<string> Permissions { get; }

        bool HasPermission(string permission);
    }

    //public interface ICurrentUserService
    //{
    //    bool IsAuthenticated { get; }

    //    AccessTokenType? AccessTokenType { get; }

    //    int? UserId { get; }

    //    string? SecurityStamp { get; }

    //    int? TokenVersion { get; }

    //    string? Jti { get; }

    //    DateTime? AccessTokenExpiration { get; }

    //    int? UserDeviceId { get; }

    //    int? UserSessionId { get; }

    //    bool? IsPersistent { get; }

    //    int? WorkProfileId { get; }

    //    WorkProfileType? WorkProfileType { get; }

    //    int? RoleId { get; }

    //    int? CampusId { get; }

    //    IReadOnlyList<string> Permissions { get; }

    //    bool HasPermission(string permission);
    //}
}