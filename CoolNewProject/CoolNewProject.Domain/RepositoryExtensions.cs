using System.Linq.Expressions;
using Ardalis.Specification;

namespace CoolNewProject.Domain; 

public static class RepositoryExtensions {
    public static Task<List<T>> ListAsync<T>(this IReadOnlyRepository<T> repository, CancellationToken cancellationToken = default) 
        where T : IAggregateRoot {
        return repository.ListAsync(new AllSpecification<T>(), cancellationToken);
    }
    
    public static Task<List<TResult>> ListAsync<TEntity, TResult>(this IReadOnlyRepository<TEntity> repository, Expression<Func<TEntity, TResult>> projection, 
        CancellationToken cancellationToken = default) where TEntity : IAggregateRoot {
        return repository.ListAsync(new ProjectionSpecification<TEntity, TResult>(projection), cancellationToken);
    }
    
    public static Task<T?> FirstOrDefaultAsync<T>(this IReadOnlyRepository<T> repository, CancellationToken cancellationToken = default) 
        where T : IAggregateRoot {
        return repository.FirstOrDefaultAsync(new AllSpecification<T>(), cancellationToken);
    }
    
    public static Task<TResult?> FirstOrDefaultAsync<TEntity, TResult>(this IReadOnlyRepository<TEntity> repository, Expression<Func<TEntity, TResult>> projection, 
        CancellationToken cancellationToken = default) where TEntity : IAggregateRoot {
        return repository.FirstOrDefaultAsync(new ProjectionSpecification<TEntity, TResult>(projection), cancellationToken);
    }
    
    private class AllSpecification<T> : Specification<T, T>{}

    private class ProjectionSpecification<TEntity, TResult> : Specification<TEntity, TResult> {
        public ProjectionSpecification(Expression<Func<TEntity, TResult>> selector) {
            // ReSharper disable once VirtualMemberCallInConstructor
            Query.Select(selector);
        }
    }
}