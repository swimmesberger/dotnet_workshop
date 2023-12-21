using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using CoolNewProject.Domain;
using Microsoft.EntityFrameworkCore;

namespace CoolNewProject.DataAccess; 

public class EfBaseRepository<T> : IRepository<T> where T : class, IAggregateRoot {
    private readonly DbContext _dbContext;
    private readonly ISpecificationEvaluator _specificationEvaluator;
    
    public EfBaseRepository(DbContext dbContext) : this(dbContext, SpecificationEvaluator.Default) {
        _dbContext = dbContext;
    }
    
    public EfBaseRepository(DbContext dbContext, ISpecificationEvaluator specificationEvaluator) {
        _dbContext = dbContext;
        _specificationEvaluator = specificationEvaluator;
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default) 
        => (await _dbContext.Set<T>().AddAsync(entity, cancellationToken)).Entity;

    public void Update(T entity) => _dbContext.Update(entity);

    public void Delete(T entity) => _dbContext.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) {
        var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);
        return specification.PostProcessingAction == null ? queryResult : specification.PostProcessingAction(queryResult).ToList();
    }
    
    public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) {
        return await ApplySpecification(specification, true).CountAsync(cancellationToken);
    }
    
    public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) {
        return await ApplySpecification(specification, true).AnyAsync(cancellationToken);
    }
    
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification) {
        return _specificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), specification);
    }
    
    private IQueryable<T> ApplySpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false) {
        return _specificationEvaluator.GetQuery(_dbContext.Set<T>().AsQueryable(), specification, evaluateCriteriaOnly);
    }
}