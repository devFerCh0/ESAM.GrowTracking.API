using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries;
using ESAM.GrowTracking.Application.Abstractions.DataAccess.Queries.Filters;
using ESAM.GrowTracking.Application.Enums;
using ESAM.GrowTracking.Application.Features.Auth.AssumeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.AssumeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.ChangeWorkProfileRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserRoleCampus.Responses;
using ESAM.GrowTracking.Application.Features.Auth.GetCurrentUserWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.Login.Responses;
using ESAM.GrowTracking.Application.Features.Commons;
using ESAM.GrowTracking.Application.Features.Users.GetUsers.Responses;
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
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AssumeRoleCampusUserResponse?> GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId,
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new AssumeRoleCampusUserResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new AssumeRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new AssumeRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
                        urc.Campus.Name)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new AssumeRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new AssumeRoleCampusSessionWorkProfileSelectedResponse(uswps.WorkProfileId,
                                uswps.UserSessionRoleCampusesSelected.Where(usrcs => usrcs.IsActive).OrderByDescending(usrcs => usrcs.CreatedAt)
                                    .Select(usrcs => new AssumeRoleCampusSessionRoleCampusSelectedResponse(usrcs.RoleId, usrcs.CampusId))
                                    .FirstOrDefault()))
                            .FirstOrDefault())).FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AssumeWorkProfileUserResponse?> GetAssumeWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new AssumeWorkProfileUserResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new AssumeWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new AssumeWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new AssumeWorkProfileSessionWorkProfileSelectedResponse(uswps.WorkProfileId))
                            .FirstOrDefault())).FirstOrDefault()))
                    .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<GetCurrentUserRoleCampusResponse?> GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new GetCurrentUserRoleCampusResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetCurrentUserRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new GetCurrentUserRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
                        urc.Campus.Name)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new GetCurrentUserRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse(uswps.WorkProfileId,
                                uswps.UserSessionRoleCampusesSelected.Where(usrcs => usrcs.IsActive).OrderByDescending(usrcs => usrcs.CreatedAt)
                                    .Select(usrcs => new GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse(usrcs.RoleId, usrcs.CampusId))
                                    .FirstOrDefault()))
                            .FirstOrDefault())).FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<GetCurrentUserWorkProfileResponse?> GetCurrentUserWorkProfileByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new GetCurrentUserWorkProfileResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetCurrentUserWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new GetCurrentUserWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse(uswps.WorkProfileId))
                            .FirstOrDefault())).FirstOrDefault()))
                    .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ChangeRoleCampusUserResponse?> GetChangeRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId,
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new ChangeRoleCampusUserResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new ChangeRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new ChangeRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
                        urc.Campus.Name)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new ChangeRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new ChangeRoleCampusSessionWorkProfileSelectedResponse(uswps.WorkProfileId,
                                uswps.UserSessionRoleCampusesSelected.Where(usrcs => usrcs.IsActive).OrderByDescending(usrcs => usrcs.CreatedAt)
                                    .Select(usrcs => new ChangeRoleCampusSessionRoleCampusSelectedResponse(usrcs.RoleId, usrcs.CampusId))
                                    .FirstOrDefault()))
                            .FirstOrDefault())).FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ChangeWorkProfileUserResponse?> GetChangeWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new ChangeWorkProfileUserResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new ChangeWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new ChangeWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new ChangeWorkProfileSessionWorkProfileSelectedResponse(uswps.WorkProfileId)).FirstOrDefault())).FirstOrDefault()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ChangeWorkProfileRoleCampusUserResponse?> GetChangeWorkProfileRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId,
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            return await query.Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new ChangeWorkProfileRoleCampusUserResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
                    u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new ChangeWorkProfileRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
                        uwp.WorkProfile.WorkProfileType)).ToList(),
                    u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new ChangeWorkProfileRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
                        urc.Campus.Name)).ToList(),
                    u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new ChangeWorkProfileRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
                        us.UserSessionWorkProfilesSelected.Where(uswps => uswps.IsActive).OrderByDescending(uswps => uswps.CreatedAt)
                            .Select(uswps => new ChangeWorkProfileRoleCampusSessionWorkProfileSelectedResponse(uswps.WorkProfileId,
                                uswps.UserSessionRoleCampusesSelected.Where(usrcs => usrcs.IsActive).OrderByDescending(usrcs => usrcs.CreatedAt)
                                    .Select(usrcs => new ChangeWorkProfileRoleCampusSessionRoleCampusSelectedResponse(usrcs.RoleId, usrcs.CampusId)).FirstOrDefault()))
                            .FirstOrDefault())).FirstOrDefault())).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedResponse<GetUsersResponse>> GetUsersAsync(GetUsersFilter filter, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
            if (filter.IsDeleted.HasValue)
                query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);
            if (filter.IsLocked.HasValue)
                query = filter.IsLocked.Value ? query.Where(u => u.LockoutEndAt != null && u.LockoutEndAt > filter.UtcNow)
                    : query.Where(u => u.LockoutEndAt == null || u.LockoutEndAt <= filter.UtcNow);
            if (filter.WorkProfileId.HasValue)
                query = query.Where(u => u.UserWorkProfiles.Any(uwp => uwp.WorkProfileId == filter.WorkProfileId.Value && !uwp.IsDeleted));
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim();
                var normalizedSearchTerm = searchTerm.ToUpper();
                query = query.Where(u => u.NormalizedUserName.Contains(normalizedSearchTerm) || u.NormalizedEmail.Contains(normalizedSearchTerm) ||
                    u.Person.FirstName.Contains(searchTerm) || u.Person.LastName.Contains(searchTerm));
            }
            var totalCount = await query.CountAsync(cancellationToken);
            if (totalCount == 0)
                return new PagedResponse<GetUsersResponse>([], totalCount, filter.PageNumber, filter.PageSize);
            var items = await ApplySorting(query, filter.SortBy, filter.SortDirection).Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(u => new GetUsersResponse(u.Id, u.Username, u.Email,
                    u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
                    u.LockoutEndAt != null && u.LockoutEndAt > filter.UtcNow, u.LockoutEndAt, u.IsDeleted, u.UserSessions.Any(us => !us.IsRevoked && us.ExpiresAt > filter.UtcNow), 
                    u.CreatedAt, u.UpdatedAt, u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetUsersUserWorkProfileResponse(uwp.WorkProfileId, 
                        uwp.WorkProfile.Name, uwp.WorkProfile.WorkProfileType)).ToList())).ToListAsync(cancellationToken);
            return new PagedResponse<GetUsersResponse>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        private static IQueryable<User> ApplySorting(IQueryable<User> query, GetUsersSortBy sortBy, SortDirection sortDirection)
        {
            return (sortBy, sortDirection) switch
            {
                (GetUsersSortBy.Username, SortDirection.Ascending) => query.OrderBy(u => u.NormalizedUserName).ThenBy(u => u.Id),
                (GetUsersSortBy.Username, SortDirection.Descending) => query.OrderByDescending(u => u.NormalizedUserName).ThenByDescending(u => u.Id),
                (GetUsersSortBy.Email, SortDirection.Ascending) => query.OrderBy(u => u.NormalizedEmail).ThenBy(u => u.Id),
                (GetUsersSortBy.Email, SortDirection.Descending) => query.OrderByDescending(u => u.NormalizedEmail).ThenByDescending(u => u.Id),
                (GetUsersSortBy.Fullname, SortDirection.Ascending) => query.OrderBy(u => u.Person.FirstName).ThenBy(u => u.Person.LastName).ThenBy(u => u.Id),
                (GetUsersSortBy.Fullname, SortDirection.Descending) => query.OrderByDescending(u => u.Person.FirstName).ThenByDescending(u => u.Person.LastName)
                    .ThenByDescending(u => u.Id),
                (GetUsersSortBy.CreatedAt, SortDirection.Ascending) => query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
                _ => query.OrderByDescending(u => u.CreatedAt).ThenByDescending(u => u.Id)
            };
        }
    }

    //public class UserQuery(ILogger<UserQuery> logger, AppDbContext context) : Query<User, int>(logger, context), IUserQuery
    //{
    //    public async Task<LoginUserResponse?> GetLoginUserByIdAsync(int id, bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
    //        return await query.Where(u => u.Id == id && !u.IsDeleted)
    //            .Select(u => new LoginUserResponse(u.Id, u.Username, u.Email,
    //                u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
    //                u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted)
    //                    .Select(uwp => new LoginUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name, uwp.WorkProfile.WorkProfileType)).ToList()))
    //            .FirstOrDefaultAsync(cancellationToken);
    //    }

    //    public async Task<AssumeRoleCampusUserResponse?> GetAssumeRoleCampusUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId,
    //        bool asTracking = false, CancellationToken cancellationToken = default)
    //    {
    //        var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
    //        return await query.Where(u => u.Id == userId && !u.IsDeleted)
    //            .Select(u => new AssumeRoleCampusUserResponse(u.Id, u.Username, u.Email,
    //                u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
    //                u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
    //                u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new AssumeRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
    //                    uwp.WorkProfile.WorkProfileType)).ToList(),
    //                u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new AssumeRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
    //                    urc.Campus.Name)).ToList(),
    //                u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new AssumeRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
    //                    us.UserSessionWorkProfileSelected != null ? new AssumeRoleCampusSessionWorkProfileSelectedResponse(us.UserSessionWorkProfileSelected.WorkProfileId,
    //                        us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected != null ?
    //                            new AssumeRoleCampusSessionRoleCampusSelectedResponse(us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.RoleId,
    //                                us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.CampusId) : null) : null)).FirstOrDefault()))
    //            .FirstOrDefaultAsync(cancellationToken);
    //    }

    //    public async Task<AssumeWorkProfileUserResponse?> GetAssumeWorkProfileUserByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
    //        return await query.Where(u => u.Id == userId && !u.IsDeleted)
    //            .Select(u => new AssumeWorkProfileUserResponse(u.Id, u.Username, u.Email,
    //                u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
    //                u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
    //                u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new AssumeWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
    //                    uwp.WorkProfile.WorkProfileType)).ToList(),
    //                u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new AssumeWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
    //                    us.UserSessionWorkProfileSelected != null ? new AssumeWorkProfileSessionWorkProfileSelectedResponse(
    //                        us.UserSessionWorkProfileSelected.WorkProfileId) : null)).FirstOrDefault()))
    //                .FirstOrDefaultAsync(cancellationToken);
    //    }

    //    public async Task<GetCurrentUserRoleCampusResponse?> GetCurrentUserRoleCampusByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
    //        return await query.Where(u => u.Id == userId && !u.IsDeleted)
    //            .Select(u => new GetCurrentUserRoleCampusResponse(u.Id, u.Username, u.Email,
    //                u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
    //                u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
    //                u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetCurrentUserRoleCampusUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
    //                    uwp.WorkProfile.WorkProfileType)).ToList(),
    //                u.UserRoleCampuses.Where(urc => !urc.IsDeleted).Select(urc => new GetCurrentUserRoleCampusUserRoleCampusResponse(urc.RoleId, urc.Role.Name, urc.CampusId,
    //                    urc.Campus.Name)).ToList(),
    //                u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new GetCurrentUserRoleCampusUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
    //                    us.UserSessionWorkProfileSelected != null ? new GetCurrentUserRoleCampusSessionWorkProfileSelectedResponse(us.UserSessionWorkProfileSelected.WorkProfileId,
    //                        us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected != null ?
    //                            new GetCurrentUserRoleCampusSessionRoleCampusSelectedResponse(us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.RoleId,
    //                                us.UserSessionWorkProfileSelected.UserSessionRoleCampusSelected.CampusId) : null) : null)).FirstOrDefault()))
    //            .FirstOrDefaultAsync(cancellationToken);
    //    }

    //    public async Task<GetCurrentUserWorkProfileResponse?> GetCurrentUserWorkProfileByUserIdAndUserSessionIdAsync(int userId, int userSessionId, bool asTracking = false,
    //        CancellationToken cancellationToken = default)
    //    {
    //        var query = asTracking ? _dbSet.AsTracking() : _dbSet.AsNoTracking();
    //        return await query.Where(u => u.Id == userId && !u.IsDeleted)
    //            .Select(u => new GetCurrentUserWorkProfileResponse(u.Id, u.Username, u.Email,
    //                u.Person.FirstName + " " + u.Person.LastName + (string.IsNullOrWhiteSpace(u.Person.SecondLastName) ? "" : " " + u.Person.SecondLastName),
    //                u.UserPhotos.Where(up => !up.IsDeleted && up.IsDefault).Select(up => up.Photo).FirstOrDefault(),
    //                u.UserWorkProfiles.Where(uwp => !uwp.IsDeleted).Select(uwp => new GetCurrentUserWorkProfileUserWorkProfileResponse(uwp.WorkProfileId, uwp.WorkProfile.Name,
    //                    uwp.WorkProfile.WorkProfileType)).ToList(),
    //                u.UserSessions.Where(us => us.Id == userSessionId).Select(us => new GetCurrentUserWorkProfileUserSessionResponse(us.Id, us.IpAddress, us.UserAgent,
    //                    us.UserSessionWorkProfileSelected != null ? new GetCurrentUserWorkProfileSessionWorkProfileSelectedResponse(
    //                        us.UserSessionWorkProfileSelected.WorkProfileId) : null)).FirstOrDefault()))
    //                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    //    }
    //}
}