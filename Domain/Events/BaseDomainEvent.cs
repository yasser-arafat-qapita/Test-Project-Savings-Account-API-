using NServiceBus;

namespace Domain.Aggregates;

public abstract class  BaseDomainEvent : IEvent
{
    public Guid EventId { get; set; }
    protected BaseDomainEvent(string type)
    {
        this.Type = type;
        this.EventId = Guid.NewGuid();
    }
    public long Version { get; set; }
    public string Type { get; set; }
}