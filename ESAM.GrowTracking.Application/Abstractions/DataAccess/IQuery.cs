using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Application.Abstractions.DataAccess
{
    public interface IQuery<TEntity> where TEntity : class { }

    public interface IQuery<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : notnull { }
}