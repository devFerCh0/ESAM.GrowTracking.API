using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.UserDevices.GetUserDevices;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Queries
{
    public class UserDeviceQuery(ILogger<UserDeviceQuery> logger, AppDbContext context) : Query<UserDevice, int>(logger, context), IUserDeviceQuery
    {
        //public async Task<List<GetLockedUserDeviceResponse>> GetAllLockedByUserIdAsync(int userId, DateTime utcNow, bool asTracking = false, 
        //    CancellationToken cancellationToken = default)
        //{
        //    var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
        //    return await query.Where(ud => !ud.IsDeleted && ud.LockoutEndAt != null && ud.LockoutEndAt > utcNow && ud.UserId == userId).OrderBy(ud => ud.LockoutEndAt)
        //        .Select(ud => new GetLockedUserDeviceResponse(ud.UserId, ud.User.Username, ud.User.Email,
        //            ud.User.Person.FirstName + " " + ud.User.Person.LastName + (string.IsNullOrWhiteSpace(ud.User.Person.SecondLastName) ? "" : " " + ud.User.Person.SecondLastName),
        //            ud.Id, ud.DeviceIdentifier, ud.DeviceName, ud.ApiClientType, ud.FailedLoginCount, ud.LastFailedLoginAt, ud.LockoutEndAt!.Value, ud.LastIp, ud.LastUserAgent, 
        //            ud.LastSeenAt)).ToListAsync(cancellationToken);
        //}

        public async Task<PagedResponse<GetUserDevicesResponse.UserDeviceResponse>> GetUserDevicesAsync(GetUserDevicesFilter userDevicesFilter, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = (asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking()).Where(ud => ud.UserId == userDevicesFilter.UserId);
            if (userDevicesFilter.IsLocked.HasValue)
                query = userDevicesFilter.IsLocked.Value ? query.Where(ud => ud.LockoutEndAt.HasValue && ud.LockoutEndAt.Value > userDevicesFilter.UtcNow)
                    : query.Where(ud => !ud.LockoutEndAt.HasValue || ud.LockoutEndAt.Value <= userDevicesFilter.UtcNow);
            if (userDevicesFilter.ApiClientType.HasValue)
                query = query.Where(ud => ud.ApiClientType == userDevicesFilter.ApiClientType.Value);
            if (userDevicesFilter.IsDeleted.HasValue)
                query = query.Where(ud => ud.IsDeleted == userDevicesFilter.IsDeleted.Value);
            if (!string.IsNullOrWhiteSpace(userDevicesFilter.SearchTerm))
                query = query.Where(ud => ud.DeviceName.Contains(userDevicesFilter.SearchTerm) || ud.DeviceIdentifier.Contains(userDevicesFilter.SearchTerm) ||
                    (ud.LastIp != null && ud.LastIp.Contains(userDevicesFilter.SearchTerm)));
            var totalCount = await query.CountAsync(cancellationToken);
            if (totalCount == 0)
                return new PagedResponse<GetUserDevicesResponse.UserDeviceResponse>([], totalCount, userDevicesFilter.PageNumber, userDevicesFilter.PageSize);
            var items = await ApplySorting(query, userDevicesFilter.UserDevicesSortBy, userDevicesFilter.SortDirection)
                .Skip((userDevicesFilter.PageNumber - 1) * userDevicesFilter.PageSize).Take(userDevicesFilter.PageSize)
                .Select(ud => new GetUserDevicesResponse.UserDeviceResponse(ud.Id, ud.UserId, ud.DeviceName, ud.DeviceIdentifier, ud.ApiClientType,
                    ud.LockoutEndAt.HasValue && ud.LockoutEndAt.Value > userDevicesFilter.UtcNow, ud.LockoutEndAt, ud.FailedLoginCount, ud.LastFailedLoginAt, ud.LastLoginAt,
                    ud.LastSeenAt, ud.LastIp, ud.IsDeleted, 
                    ud.UserSessions.Any(us => !us.IsRevoked && us.ExpiresAt > userDevicesFilter.UtcNow && us.AbsoluteExpiresAt > userDevicesFilter.UtcNow), ud.CreatedAt))
                .ToListAsync(cancellationToken);
            return new PagedResponse<GetUserDevicesResponse.UserDeviceResponse>(items, totalCount, userDevicesFilter.PageNumber, userDevicesFilter.PageSize);
        }

        private static IQueryable<UserDevice> ApplySorting(IQueryable<UserDevice> userDeviceQueryable, GetUserDevicesSortBy userDevicesSortBy, SortDirection sortDirection)
        {
            return (userDevicesSortBy, sortDirection) switch
            {
                (GetUserDevicesSortBy.LastLoginAt, SortDirection.Descending) => userDeviceQueryable.OrderByDescending(ud => ud.LastLoginAt),
                (GetUserDevicesSortBy.LastLoginAt, SortDirection.Ascending) => userDeviceQueryable.OrderBy(ud => ud.LastLoginAt),
                (GetUserDevicesSortBy.FailedLoginCount, SortDirection.Descending) => userDeviceQueryable.OrderByDescending(ud => ud.FailedLoginCount),
                (GetUserDevicesSortBy.FailedLoginCount, SortDirection.Ascending) => userDeviceQueryable.OrderBy(ud => ud.FailedLoginCount),
                (GetUserDevicesSortBy.CreatedAt, SortDirection.Descending) => userDeviceQueryable.OrderByDescending(ud => ud.CreatedAt),
                (GetUserDevicesSortBy.CreatedAt, SortDirection.Ascending) => userDeviceQueryable.OrderBy(ud => ud.CreatedAt),
                (GetUserDevicesSortBy.LastSeenAt, SortDirection.Descending) => userDeviceQueryable.OrderByDescending(ud => ud.LastSeenAt),
                _ => userDeviceQueryable.OrderBy(ud => ud.LastSeenAt)
            };
        }
    }
}