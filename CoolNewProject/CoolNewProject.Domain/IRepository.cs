namespace CoolNewProject.Domain; 

public interface IRepository<T> : IReadOnlyRepository<T> where T: IAggregateRoot {
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}