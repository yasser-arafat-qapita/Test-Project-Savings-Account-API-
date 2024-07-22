using Domain.Aggregates;
using NServiceBus;

namespace Domain.Events;

public class SavingsAccountCreated : BaseDomainEvent 
{
    public SavingsAccountCreated():base(nameof(SavingsAccountCreated))
    {
        
    }
    public string AccountId { get; set; }
    public decimal Balance { get; set; } 
}