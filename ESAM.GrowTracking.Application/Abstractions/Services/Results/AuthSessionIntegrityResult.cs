using ESAM.GrowTracking.Application.ValueObjects;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.Services.Results
{
    public sealed class AuthSessionIntegrityResult
    {
        public bool IsValid { get; }

        public UserSession? UserSession { get; }

        public UserSessionRefreshToken? UserSessionRefreshToken { get; }

        public Error? Error { get; }

        private AuthSessionIntegrityResult(bool isValid, UserSession? userSession, UserSessionRefreshToken? userSessionRefreshToken, Error? error)
        {
            IsValid = isValid;
            UserSession = userSession;
            UserSessionRefreshToken = userSessionRefreshToken;
            Error = error;
        }

        public static AuthSessionIntegrityResult Success(UserSession userSession, UserSessionRefreshToken userSessionRefreshToken)
            => new(true, userSession, userSessionRefreshToken, null);

        public static AuthSessionIntegrityResult Invalid(Error error) => new(false, null, null, error);
    }
}