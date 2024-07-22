using Domain.Aggregates;
using NServiceBus;

namespace Domain.Events;

public class SavingsAccountUpdated : BaseDomainEvent 
{
    public SavingsAccountUpdated() : base(nameof(SavingsAccountUpdated))
    {
        
    }
    public string AccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; }
}