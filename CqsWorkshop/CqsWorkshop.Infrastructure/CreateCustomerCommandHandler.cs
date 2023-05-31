using CqsWorkshop.Contract.Commands;
using CqsWorkshop.Infrastructure.Database;
using CqsWorkshop.Infrastructure.Mappings;
using Mediator;

namespace CqsWorkshop.Infrastructure; 

public sealed class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand> {
    private readonly OrderManagementDbContext _dbContext;

    public CreateCustomerCommandHandler(OrderManagementDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async ValueTask<Unit> Handle(CreateCustomerCommand command, CancellationToken cancellationToken) {
        var customer = command.Body.ToEntity();
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}