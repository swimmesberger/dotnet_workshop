using Mediator;

namespace CqsWorkshop.Contract.Queries; 

public sealed record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto?>;