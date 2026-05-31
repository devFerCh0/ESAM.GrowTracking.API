using ESAM.GrowTracking.Application.Abstractions.DataAccess;
using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.DataAccess
{
    public class Query<TEntity> : IQuery<TEntity> where TEntity : class
    {
        protected readonly ILogger<Query<TEntity>> _logger;
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Query(ILogger<Query<TEntity>> logger, AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(context);
            _logger = logger;
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
    }

    public class Query<TEntity, TKey> : IQuery<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : notnull
    {
        protected readonly ILogger<Query<TEntity, TKey>> _logger;
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Query(ILogger<Query<TEntity, TKey>> logger, AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(context);
            _logger = logger;
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
    }
}