using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries.Filters;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.Login.Responses;
using ESAM.GrowTracking.Application.Features.Users.GetUsers.Responses;
using ESAM.GrowTracking.Application.Results;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries
{
    public interface IUserQuery : IQuery<User, int>
    {
        Task<LoginUserResponse?> GetLoginUserByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<AssumeRoleCampusUserResponse?> GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<AssumeWorkProfileUserResponse?> GetAssumeWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<GetCurrentUserRoleCampusResponse?> GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<GetCurrentUserWorkProfileResponse?> GetCurrentUserWorkProfileByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<ChangeRoleCampusUserResponse?> GetChangeRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default);

        Task<ChangeWorkProfileUserResponse?> GetChangeWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false, 
            CancellationToken cancellationToken = default);
        Task<ChangeWorkProfileRoleCampusUserResponse?> GetChangeWorkProfileRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false, 
            CancellationToken cancellationToken = default);

        Task<PagedResult<GetUsersResponse>> GetUsersAsync(GetUsersFilter filter, bool asTracking = false, CancellationToken cancellationToken = default);
    }
}