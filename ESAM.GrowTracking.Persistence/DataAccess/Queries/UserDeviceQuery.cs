using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Features.Auth.GetLockedUserDevices;
using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess.Queries
{
    public class UserDeviceQuery(ILogger<UserDeviceQuery> logger, AppDbContext context) : Query<UserDevice, int>(logger, context), IUserDeviceQuery
    {
        public async Task<List<GetLockedUserDeviceResponse>> GetAllLockedByUserIdAsync(int userId, DateTime utcNow, bool asTracking = false, 
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();


            return await query.Where(ud => !ud.IsDeleted && ud.LockoutEndAt != null && ud.LockoutEndAt > utcNow && ud.UserId == userId).OrderBy(ud => ud.LockoutEndAt)
                .Select(ud => new GetLockedUserDeviceResponse(ud.UserId, ud.User.Username, ud.User.Email,
                    ud.User.Person.FirstName + " " + ud.User.Person.LastName + (string.IsNullOrWhiteSpace(ud.User.Person.SecondLastName) ? "" : " " + ud.User.Person.SecondLastName),
                    ud.Id, ud.DeviceIdentifier, ud.DeviceName, ud.ApiClientType, ud.FailedLoginCount, ud.LastFailedLoginAt, ud.LockoutEndAt!.Value, ud.LastIp, ud.LastUserAgent, 
                    ud.LastSeenAt)).ToListAsync(cancellationToken);
        }
    }
}