namespace ESAM.GrowTracking.Domain.Abstractions
{
    public interface IEntity<TKey> where TKey : notnull
    {
        TKey Id { get; }
    }
}