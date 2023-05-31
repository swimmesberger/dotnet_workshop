using Mediator;

namespace CqsWorkshop.Contract.Commands; 

public sealed record CreateCustomerCommand(CustomerForCreationDto Body) : ICommand;