using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.Login.Responses;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Queries
{
    public class UserQuery(ILogger<UserQuery> logger, AppDbContext context) : Query<User, int>(logger, context), IUserQuery
    {
        public async Task<LoginUserResponse?> GetLoginUserByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new LoginUserResponse(u.Id, u.Username, u.Email, 
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName), 
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted)
                        .Select(uwp => new LoginUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name, uwp.WorkProfile.WorkProfileType)).ToList()))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<AssumeWorkProfileUserResponse?> GetAssumeWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId)
                .Select(u => new AssumeWorkProfileUserResponse(u.Id, u.Username, u.Email, 
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName), 
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new AssumeWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name, 
                        uwp.WorkProfile.WorkProfileType)).ToList(), 
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new AssumeWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent, 
                        us.UserSessionWorkProfileSelected != null ? new AssumeWorkProfileSessionWorkProfileSelectedResponse(
                            us.UserSessionWorkProfileSelected.WorkProfileId) : null)).FirstOrDefault()))
                    .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<AssumeRoleCampusUserResponse?> GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, 
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId)
                .Select(u => new AssumeRoleCampusUserResponse(u.Id, u.Username, u.Email, 
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName), 
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(), 
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new AssumeRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name, 
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new AssumeRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId, 
                        urc.Campus.Name)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new AssumeRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfileSelected != null ? new AssumeRoleCampusSessionWorkProfileSelectedResponse(us.UserSessionWorkProfileSelected.WorkProfileId, 
                            us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected != null ? 
                                new AssumeRoleCampusSessionRoleCampusSelectedResponse(us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.RoleId,
                                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.CampusId) : null) : null)).FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<GetCurrentUserWorkProfileResponse?> GetCurrentUserWorkProfileByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId)
                .Select(u => new GetCurrentUserWorkProfileResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetCurrentUserWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new GetCurrentUserWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfileSelected != null ? new GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse(
                            us.UserSessionWorkProfileSelected.WorkProfileId) : null)).FirstOrDefault()))
                    .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<GetCurrentUserRoleCampusResponse?> GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId)
                .Select(u => new GetCurrentUserRoleCampusResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetCurrentUserRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new GetCurrentUserRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
                        urc.Campus.Name)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new GetCurrentUserRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfileSelected != null ? new GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse(us.UserSessionWorkProfileSelected.WorkProfileId,
                            us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected != null ?
                                new GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse(us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.RoleId,
                                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.CampusId) : null) : null)).FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}