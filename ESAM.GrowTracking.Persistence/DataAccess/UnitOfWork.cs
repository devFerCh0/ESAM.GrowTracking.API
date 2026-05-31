using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess.Repositories;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ESAM.GrowTracking.Persistence.DataAccess
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger<UnitOfWork> _logger;
        private readonly AppDbContext _context;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokensPermanent;
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokensTemporary;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokens;
        private readonly IRolePermissionRepository _rolePermissions;
        private readonly IUserRepository _users;
        private readonly IUserDeviceRepository _userDevices;
        private readonly IUserRoleCampusRepository _userRoleCampuses;
        private readonly IUserSessionRepository _userSessions;
        private readonly IUserSessionRefreshTokenRepository _userSessionRefreshTokens;
        private readonly IUserSessionWorkProfileSelectedRepository _userSessionWorkProfilesSelected;
        private readonly IUserSessionRoleCampusSelectedRepository _userSessionRoleCampusesSelected;
        private readonly IUserWorkProfileRepository _userWorkProfiles;
        private readonly IWorkProfileRepository _workProfiles;
        private readonly IWorkProfilePermissionRepository _workProfilePermissions;
        private readonly SemaphoreSlim _semaphore;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public UnitOfWork(ILogger<UnitOfWork> logger, AppDbContext context, IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokensPermanent,
            IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokensTemporary, IBlacklistedRefreshTokenRepository blacklistedRefreshTokens,
            IRolePermissionRepository rolePermissions, IUserRepository users, IUserDeviceRepository userDevices, IUserRoleCampusRepository userRoleCampuses,
            IUserSessionRepository userSessions, IUserSessionRefreshTokenRepository userSessionRefreshTokens, 
            IUserSessionWorkProfileSelectedRepository userSessionWorkProfilesSelected, IUserSessionRoleCampusSelectedRepository userSessionRoleCampusesSelected,
            IUserWorkProfileRepository userWorkProfiles, IWorkProfileRepository workProfiles, IWorkProfilePermissionRepository workProfilePermissions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokensPermanent);
            ArgumentNullException.ThrowIfNull(blacklistedAccessTokensTemporary);
            ArgumentNullException.ThrowIfNull(blacklistedRefreshTokens);
            ArgumentNullException.ThrowIfNull(rolePermissions);
            ArgumentNullException.ThrowIfNull(users);
            ArgumentNullException.ThrowIfNull(userDevices);
            ArgumentNullException.ThrowIfNull(userRoleCampuses);
            ArgumentNullException.ThrowIfNull(userSessions);
            ArgumentNullException.ThrowIfNull(userSessionRefreshTokens);
            ArgumentNullException.ThrowIfNull(userSessionWorkProfilesSelected);
            ArgumentNullException.ThrowIfNull(userSessionRoleCampusesSelected);
            ArgumentNullException.ThrowIfNull(userWorkProfiles);
            ArgumentNullException.ThrowIfNull(workProfiles);
            ArgumentNullException.ThrowIfNull(workProfilePermissions);
            _logger = logger;
            _context = context;
            _blacklistedAccessTokensPermanent = blacklistedAccessTokensPermanent;
            _blacklistedAccessTokensTemporary = blacklistedAccessTokensTemporary;
            _blacklistedRefreshTokens = blacklistedRefreshTokens;
            _rolePermissions = rolePermissions;
            _users = users;
            _userDevices = userDevices;
            _userRoleCampuses = userRoleCampuses;
            _userSessions = userSessions;
            _userSessionRefreshTokens = userSessionRefreshTokens;
            _userSessionWorkProfilesSelected = userSessionWorkProfilesSelected;
            _userSessionRoleCampusesSelected = userSessionRoleCampusesSelected;
            _userWorkProfiles = userWorkProfiles;
            _workProfiles = workProfiles;
            _workProfilePermissions = workProfilePermissions;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public bool HasActiveTransaction => _transaction is not null;

        public IBlacklistedAccessTokenPermanentRepository BlacklistedAccessTokensPermanent => _blacklistedAccessTokensPermanent;

        public IBlacklistedAccessTokenTemporaryRepository BlacklistedAccessTokensTemporary => _blacklistedAccessTokensTemporary;

        public IBlacklistedRefreshTokenRepository BlacklistedRefreshTokens => _blacklistedRefreshTokens;

        public IRolePermissionRepository RolePermissions => _rolePermissions;

        public IUserRepository Users => _users;

        public IUserDeviceRepository UserDevices => _userDevices;

        public IUserRoleCampusRepository UserRoleCampuses => _userRoleCampuses;

        public IUserSessionRepository UserSessions => _userSessions;

        public IUserSessionRefreshTokenRepository UserSessionRefreshTokens => _userSessionRefreshTokens;

        public IUserSessionWorkProfileSelectedRepository UserSessionWorkProfilesSelected => _userSessionWorkProfilesSelected;

        public IUserSessionRoleCampusSelectedRepository UserSessionRoleCampusesSelected => _userSessionRoleCampusesSelected;

        public IUserWorkProfileRepository UserWorkProfiles => _userWorkProfiles;

        public IWorkProfileRepository WorkProfiles => _workProfiles;

        public IWorkProfilePermissionRepository WorkProfilePermissions => _workProfilePermissions;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("SaveChangesAsync: operación cancelada.");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "SaveChangesAsync: error de base de datos al guardar cambios.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveChangesAsync: error inesperado al guardar cambios.");
                throw;
            }
        }

        public async Task BeginTransactionAsync(IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_transaction is not null)
                    throw new InvalidOperationException("Ya existe una transacción activa.");
                _transaction = isolationLevel.HasValue ? await _context.Database.BeginTransactionAsync(isolationLevel.Value, cancellationToken).ConfigureAwait(false)
                    : await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("BeginTransactionAsync: transacción iniciada. IsolationLevel={IsolationLevel}.", _transaction.GetDbTransaction().IsolationLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BeginTransactionAsync: no se pudo iniciar la transacción.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_transaction is null)
                    throw new InvalidOperationException("No hay una transacción activa para confirmar.");
                try
                {
                    await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("CommitTransactionAsync: transacción confirmada.");
                }
                catch (Exception commitEx)
                {
                    _logger.LogError(commitEx, "CommitTransactionAsync: fallo durante el commit, iniciando rollback.");
                    try
                    {
                        await RollbackTransactionCoreAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.LogError(rollbackEx, "CommitTransactionAsync: el rollback también falló.");
                        throw new InvalidOperationException("Falló el commit y también falló el rollback.", new AggregateException(commitEx, rollbackEx));
                    }
                    throw;
                }
                finally
                {
                    await DisposeCurrentTransactionAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_transaction is null)
                    throw new InvalidOperationException("No hay una transacción activa para revertir.");
                try
                {
                    await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("RollbackTransactionAsync: transacción revertida.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RollbackTransactionAsync: error durante el rollback.");
                    throw;
                }
                finally
                {
                    await DisposeCurrentTransactionAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, IsolationLevel? isolationLevel = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(action);
            ThrowIfDisposed();
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
                try
                {
                    await action(cancellationToken).ConfigureAwait(false);
                    await CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    if (HasActiveTransaction)
                        await RollbackTransactionAsync(CancellationToken.None).ConfigureAwait(false);
                    throw;
                }
            }).ConfigureAwait(false);
        }

        private async Task RollbackTransactionCoreAsync(CancellationToken cancellationToken)
        {
            if (_transaction is null)
                return;
            try
            {
                await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("RollbackTransactionCoreAsync: rollback exitoso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RollbackTransactionCoreAsync: el rollback falló.");
                throw;
            }
        }

        private async Task DisposeCurrentTransactionAsync()
        {
            if (_transaction is null)
                return;
            try
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DisposeCurrentTransactionAsync: error al liberar la transacción (no crítico).");
            }
            finally
            {
                _transaction = null;
            }
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

        public void Dispose()
        {
            if (_disposed)
                return;
            try
            {
                _transaction?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Dispose: error al liberar la transacción (no crítico).");
            }
            finally
            {
                _transaction = null;
                _semaphore.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            try
            {
                if (_transaction is not null)
                    await _transaction.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "DisposeAsync: error al liberar la transacción (no crítico).");
            }
            finally
            {
                _transaction = null;
                _semaphore.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}