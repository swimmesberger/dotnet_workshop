using CqsWorkshop.Contract;
using CqsWorkshop.Contract.Queries;
using CqsWorkshop.Infrastructure.Database;
using CqsWorkshop.Infrastructure.Mappings;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace CqsWorkshop.Infrastructure.Handlers; 

public sealed class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDto?> {
    private readonly OrderManagementDbContext _dbContext;

    public GetCustomerByIdQueryHandler(OrderManagementDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async ValueTask<CustomerDto?> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken) {
        return await _dbContext.Customers
            .Where(c => c.Id == query.Id)
            .ProjectToDto()
            .FirstOrDefaultAsync(cancellationToken);
    }
}