using Ardalis.Specification;
using CoolNewProject.Domain;

namespace CoolNewProject.UnitTests; 

// this is only an example implementation - create implementations as needed for the unit tests
public class InMemoryRepository<T> : AbstractRepository<T> where T : IAggregateRoot {
    private readonly List<T> _items;
    private readonly IInMemorySpecificationEvaluator _specificationEvaluator;
    
    public InMemoryRepository() : this(Enumerable.Empty<T>(), InMemorySpecificationEvaluator.Default) { }
    
    public InMemoryRepository(IEnumerable<T> items) : this(items, InMemorySpecificationEvaluator.Default) { }
    
    public InMemoryRepository(IEnumerable<T> items, IInMemorySpecificationEvaluator specificationEvaluator) {
        _items = new List<T>(items);
        _specificationEvaluator = specificationEvaluator;
    }
    
    public override Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) {
        return Task.FromResult(_specificationEvaluator.Evaluate(_items, specification).ToList());
    }
    
    public override Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) {
        return Task.FromResult(_specificationEvaluator.Evaluate(_items, specification).Count());
    }
    
    public override Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) {
        return Task.FromResult(_specificationEvaluator.Evaluate(_items, specification).Any());
    }
    
    public override Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, 
        CancellationToken cancellationToken = default) where TResult : default {
        return Task.FromResult(_specificationEvaluator.Evaluate(_items, specification).FirstOrDefault());
    }
}