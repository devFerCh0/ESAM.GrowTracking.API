using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using System.Data;

namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        bool HasActiveTransaction { get; }

        IBlacklistedAccessTokenSessionRepository BlacklistedAccessTokensSession { get; }

        IBlacklistedAccessTokenTemporaryRepository BlacklistedAccessTokensTemporary { get; }

        IBlacklistedRefreshTokenRepository BlacklistedRefreshTokens { get; }

        IRolePermissionRepository RolePermissions { get; }

        IUserRepository Users { get; }

        IUserDeviceRepository UserDevices { get; }

        IUserRoleCampusRepository UserRoleCampuses { get; }

        IUserSessionRepository UserSessions { get; }

        IUserSessionRefreshTokenRepository UserSessionRefreshTokens { get; }

        IUserSessionWorkProfileSelectedRepository UserSessionWorkProfilesSelected { get; }

        IUserSessionRoleCampusSelectedRepository UserSessionRoleCampusesSelected { get; }

        IUserWorkProfileRepository UserWorkProfiles { get; }

        IWorkProfileRepository WorkProfiles { get; }

        IWorkProfilePermissionRepository WorkProfilePermissions { get; }

        Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}