using Ardalis.Specification;
using CoolNewProject.Domain;

namespace CoolNewProject.UnitTests; 

// this is only an example implementation - create implementations as needed for the unit tests
public abstract class AbstractRepository<T> : IRepository<T> where T : IAggregateRoot {
    public virtual Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) {
        return Task.FromException<List<TResult>>(new NotImplementedException());
    }
    
    public virtual Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) {
        return Task.FromException<int>(new NotImplementedException());
    }
    
    public virtual Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) {
        return Task.FromException<bool>(new NotImplementedException());
    }
    
    public virtual Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) {
        return Task.FromException<TResult?>(new NotImplementedException());
    }
    
    public virtual Task<T> AddAsync(T entity, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
    
    public virtual void Update(T entity) {
        throw new NotImplementedException();
    }
    
    public virtual void Delete(T entity) {
        throw new NotImplementedException();
    }
    
    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
}