
using System.Transactions;

namespace CqsWorkshop.Infrastructure.Behaviours;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TransactionalAttribute : Attribute {
    public IsolationLevel? IsolationLevel { get; init; }
}