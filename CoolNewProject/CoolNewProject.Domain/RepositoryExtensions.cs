using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Ardalis.Specification;

namespace CoolNewProject.Domain; 

/// <summary>
/// Only add the most basic/general specifications here, for more complex queries use use-case specific specifications.
/// Additionally not everything must be implemented as a specification (but as much as possible). If there are specific
/// queries required which require the full feature set of IQueryable (e.g. complex joins) those methods can be easily
/// added as a specific repository method to your specific repository.
/// ISpecification itself is there to improve testability because IQueryable (with extension methods) is difficult to
/// mock. Only using repositories with specific methods can be tedious to mock because every specific methods requires
/// a specific mock which can get out-of-hand really fast - therefore is the specification pattern a good middle-ground
/// between specific repositories and complex IQueryable.
/// </summary>
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
    
    public static Task<TResult?> FirstOrDefaultAsync<TEntity, TResult>(this IReadOnlyRepository<TEntity> repository, 
        Expression<Func<TEntity, TResult>> projection, 
        Expression<Func<TEntity, object?>>? orderExpression = null,
        CancellationToken cancellationToken = default) where TEntity : IAggregateRoot {
        return repository.FirstOrDefaultAsync(new ProjectionSpecification<TEntity, TResult>(projection, orderExpression), cancellationToken);
    }
    
    private sealed class AllSpecification<T> : Specification<T, T>{}

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    private sealed class ProjectionSpecification<TEntity, TResult> : Specification<TEntity, TResult> {
        public ProjectionSpecification(Expression<Func<TEntity, TResult>> selector, 
            Expression<Func<TEntity, object?>>? orderExpression = null) {
            Query.Select(selector);
            if (orderExpression != null) {
                Query.OrderBy(orderExpression);
            }
        }
    }
}