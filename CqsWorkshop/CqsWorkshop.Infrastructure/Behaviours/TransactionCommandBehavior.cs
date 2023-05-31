using Mediator;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Transactions;

namespace CqsWorkshop.Infrastructure.Behaviours; 

public sealed class TransactionCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage {
    private readonly ILogger<TransactionCommandBehavior<TRequest, TResponse>> _logger;

    public TransactionCommandBehavior(
        ILogger<TransactionCommandBehavior<TRequest, TResponse>> logger) {
        _logger = logger;
    }

    public ValueTask<TResponse> Handle(TRequest request,
        CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next) {
        // get the next command handler type that is invoked
        Type? handlerType = next.Target?.GetType();
        var transactionalAttribute = handlerType?.GetCustomAttribute<TransactionalAttribute>();
        // check if the transactional attribute is present
        return transactionalAttribute is null ? 
            next(request, cancellationToken) : 
            HandleImpl(request, next, transactionalAttribute, cancellationToken);
    }

    private async ValueTask<TResponse> HandleImpl(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, 
        TransactionalAttribute transactionalAttribute, CancellationToken cancellationToken) {
        var transactionOptions = new TransactionOptions();
        if (transactionalAttribute.IsolationLevel != null) {
            transactionOptions.IsolationLevel = transactionalAttribute.IsolationLevel.Value;
        }
        TResponse response;
        // TODO: uses sync methods - look for an async variant
        using (var tx = new TransactionScope(TransactionScopeOption.Required, transactionOptions,
                   TransactionScopeAsyncFlowOption.Enabled)) {
            _logger.LogInformation("Transaction BEGIN {IsolationLevel}", Transaction.Current?.IsolationLevel);
            // adds completion handler to tx
            LogTransactionCompleted();
            response = await next(request, cancellationToken);
            tx.Complete();
        }
        _logger.LogInformation("Transaction DISPOSED");
        return response;
    }

    private void LogTransactionCompleted() {
        if (Transaction.Current is not null) {
            Transaction.Current.TransactionCompleted += (_, e) => {
                bool committed = e.Transaction?.TransactionInformation.Status == TransactionStatus.Committed;
                _logger.LogInformation("Transaction {TransactionStatus}", committed ? "COMMITTED" : "ROLLED BACK");
            };
        }
    }
}