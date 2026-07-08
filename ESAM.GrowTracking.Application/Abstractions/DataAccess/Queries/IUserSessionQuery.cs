using ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions.Responses;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries
{
    public interface IUserSessionQuery : IQuery<UserSession, int>
    {
        Task<List<GetActiveUserSessionsResponse>> GetActiveUserSessionsByUserIdAsync(int userId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<List<GetActiveCurrentUserSessionsResponse>> GetActiveCurrentUserSessionsByUserIdAsync(int userId, int? userSessionId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default);
    }
}