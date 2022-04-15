namespace Seedwork.DomainObjects
{
    public interface IRepository<T> where T : IAggregateRoot
    {
         Task Add(T aggregateRoot);
         Task<IEnumerable<T>> GetAll();
         Task<T> GetById(Guid id);
         void Update(T aggregateRoot);
         Task Delete(Guid id);
    }
}