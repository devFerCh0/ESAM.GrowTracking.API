using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Abstractions.DataAccess;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly ILogger<Repository<TEntity>> _logger;
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(ILogger<Repository<TEntity>> logger, AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(context);
            _logger = logger;
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        public virtual Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                _dbSet.AddRange(list);
                return Task.CompletedTask;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
        }

        public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                _dbSet.RemoveRange(list);
                return Task.CompletedTask;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
        }
    }

    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : notnull
    {
        protected readonly ILogger<Repository<TEntity, TKey>> _logger;
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(ILogger<Repository<TEntity, TKey>> logger, AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(context);
            _logger = logger;
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            if (asTracking)
                return await _dbSet.AsTracking().FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken).ConfigureAwait(false);
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().AnyAsync(e => e.Id!.Equals(id), cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        public virtual Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                _dbSet.AddRange(list);
                return Task.CompletedTask;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
        }

        public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                _dbSet.UpdateRange(list);
                return Task.CompletedTask;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
        }

        public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            var list = entities as IList<TEntity> ?? [.. entities];
            var originalDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                _dbSet.RemoveRange(list);
                return Task.CompletedTask;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalDetect;
            }
        }
    }
}