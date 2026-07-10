using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveCurrentUserSessions.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetActiveUserSessions.Responses;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Queries
{
    public class UserSessionQuery(ILogger<UserSessionQuery> logger, AppDbContext context) : Query<UserSession, int>(logger, context), IUserSessionQuery
    {
        public async Task<List<GetActiveUserSessionsResponse>> GetActiveUserSessionsByUserIdAsync(int userId, DateTime utcNow, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(us => us.UserId == userId && !us.IsRevoked && us.ExpiresAt > utcNow && us.AbsoluteExpiresAt > utcNow)
                .OrderByDescending(us => us.LastActivityAt ?? us.CreatedAt).Select(us => new GetActiveUserSessionsResponse(us.Id, us.UserId, us.UserDeviceId, 
                    us.UserDevice.DeviceName, us.UserDevice.ApiClientType, us.IpAddress, us.UserAgent, us.IsPersistent, us.CreatedAt, us.LastActivityAt, us.ExpiresAt, 
                    us.AbsoluteExpiresAt, us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                        .Select(uswps => new GetActiveUserSessionWorkProfileResponse(uswps.WorkProfileId, uswps.UserWorkProfile.WorkProfile.Name, 
                            uswps.UserWorkProfile.WorkProfile.WorkProfileType, uswps.UserSessionRoleCampusesSelected.Where(usrcs => usrcs.IsActive)
                                .OrderByDescending(usrcs => usrcs.CreatedAt).Select(usrcs => new GetActiveUserSessionRoleCampusResponse(usrcs.RoleId, 
                                    usrcs.UserRoleCampus.Role.Name, usrcs.CampusId, usrcs.UserRoleCampus.Campus.Name)).FirstOrDefault())).FirstOrDefault()))
                .ToListAsync(cancellationToken);
            //return await query.Where(us => us.UserId == userId && !us.IsRevoked && us.ExpiresAt > utcNow && us.AbsoluteExpiresAt > utcNow)
            //    .OrderByDescending(us => us.LastActivityAt ?? us.CreatedAt)
            //    .Select(us => new GetActiveUserSessionsResponse(us.Id, us.UserId, us.UserDeviceId, us.UserDevice.DeviceName, us.UserDevice.ApiClientType, us.IpAddress, 
            //        us.UserAgent, us.IsPersistent, us.CreatedAt, us.LastActivityAt, us.ExpiresAt, us.AbsoluteExpiresAt, 
            //        us.UserSessionWorkProfileSelected != null ? new GetActiveUserSessionWorkProfileResponse(us.UserSessionWorkProfileSelected.WorkProfileId,
            //            us.UserSessionWorkProfileSelected.UserWorkProfile.WorkProfile.Name, us.UserSessionWorkProfileSelected.UserWorkProfile.WorkProfile.WorkProfileType, 
            //            us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected != null
            //                ? new GetActiveUserSessionRoleCampusResponse(us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.RoleId,
            //                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.UserRoleCampus.Role.Name,
            //                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.CampusId,
            //                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.UserRoleCampus.Campus.Name) : null) : null)).ToListAsync(cancellationToken);
        }

        public async Task<List<GetActiveCurrentUserSessionsResponse>> GetActiveCurrentUserSessionsByUserIdAsync(int userId, int? userSessionId, DateTime utcNow, 
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(us => us.UserId == userId && !us.IsRevoked && us.ExpiresAt > utcNow && us.AbsoluteExpiresAt > utcNow)
                .OrderByDescending(us => us.LastActivityAt ?? us.CreatedAt).Select(us => new GetActiveCurrentUserSessionsResponse(us.Id, us.UserId, us.UserDeviceId, 
                    us.UserDevice.DeviceName, us.UserDevice.ApiClientType, us.IpAddress, us.UserAgent, us.IsPersistent, us.CreatedAt, us.LastActivityAt, us.ExpiresAt,
                    us.AbsoluteExpiresAt, userSessionId != null && us.Id == userSessionId, us.UserSessionWorkProfilesSelected.Where(wp => wp.IsActive)
                        .Select(wp => new GetActiveCurrentUserSessionWorkProfileResponse(wp.WorkProfileId, wp.UserWorkProfile.WorkProfile.Name, 
                            wp.UserWorkProfile.WorkProfile.WorkProfileType, wp.UserSessionRoleCampusesSelected.Where(rc => rc.IsActive)
                            .Select(rc => new GetActiveCurrentUserSessionRoleCampusResponse(rc.RoleId, rc.UserRoleCampus.Role.Name, rc.CampusId, rc.UserRoleCampus.Campus.Name))
                                .FirstOrDefault())).FirstOrDefault())).ToListAsync(cancellationToken);
            //return await query.Where(us => us.UserId == userId && !us.IsRevoked && us.ExpiresAt > utcNow && us.AbsoluteExpiresAt > utcNow)
            //    .OrderByDescending(us => us.LastActivityAt ?? us.CreatedAt)
            //    .Select(us => new GetActiveCurrentUserSessionsResponse(us.Id, us.UserId, us.UserDeviceId, us.UserDevice.DeviceName, us.UserDevice.ApiClientType, us.IpAddress,
            //        us.UserAgent, us.IsPersistent, us.CreatedAt, us.LastActivityAt, us.ExpiresAt, us.AbsoluteExpiresAt, userSessionId != null && us.Id == userSessionId,
            //        us.UserSessionWorkProfileSelected != null ? new GetActiveCurrentUserSessionWorkProfileResponse(us.UserSessionWorkProfileSelected.WorkProfileId,
            //            us.UserSessionWorkProfileSelected.UserWorkProfile.WorkProfile.Name, us.UserSessionWorkProfileSelected.UserWorkProfile.WorkProfile.WorkProfileType,
            //            us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected != null
            //                ? new GetActiveCurrentUserSessionRoleCampusResponse(us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.RoleId,
            //                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.UserRoleCampus.Role.Name,
            //                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.CampusId,
            //                    us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.UserRoleCampus.Campus.Name) : null) : null)).ToListAsync(cancellationToken);
        }
    }
}