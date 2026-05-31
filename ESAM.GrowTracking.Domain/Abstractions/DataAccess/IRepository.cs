namespace ESAM.GrowTracking.Domain.Abstractions.DataAccess
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }

    public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : notnull
    {
        Task<TEntity?> GetByIdAsync(TKey id, bool asTracking = false, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

        Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}