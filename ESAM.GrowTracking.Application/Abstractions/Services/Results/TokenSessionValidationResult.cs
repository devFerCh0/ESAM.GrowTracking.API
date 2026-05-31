using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services.Results
{
    public sealed class TokenSessionValidationResult
    {
        public bool IsValid { get; }
        public string RevokedReason { get; }
        public int WorkProfileId { get; }
        public int? RoleId { get; }
        public int? CampusId { get; }
        public User? ValidatedUser { get; }

        private TokenSessionValidationResult(bool isValid, string revokedReason, User? validatedUser, int workProfileId, int? roleId, int? campusId)
        {
            IsValid = isValid;
            RevokedReason = revokedReason;
            ValidatedUser = validatedUser;
            WorkProfileId = workProfileId;
            RoleId = roleId;
            CampusId = campusId;
        }

        public static TokenSessionValidationResult Success(string revokedReason, User validatedUser, int workProfileId, int? roleId, int? campusId)
            => new(true, revokedReason, validatedUser, workProfileId, roleId, campusId);

        public static TokenSessionValidationResult Failure(string revokedReason) => new(false, revokedReason, null, 0, null, null);
    }
}