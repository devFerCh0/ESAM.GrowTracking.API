using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Domain.Enums;

namespace ESAM.GrowTracking.Application.DTOs
{
    public sealed record AccessTokenClaimsDTO(string? Jti, int? UserId, string? SecurityStamp, int? TokenVersion, int? UserDeviceId, AccessTokenType? AccessTokenType, 
        DateTime? AccessTokenExpiration, int? UserSessionId, int? WorkProfileId, WorkProfileType? WorkProfileType, int? RoleId, int? CampusId);
}