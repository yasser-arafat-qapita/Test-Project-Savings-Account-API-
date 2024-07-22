using System.Data;
using Domain.Events;
using Microsoft.Extensions.Logging;

namespace Domain.Aggregates;

public abstract class AggregateRoot
{
    protected Guid _id;
    private readonly List<BaseDomainEvent> _changes = new List<BaseDomainEvent>();
    
    public Guid Id => _id;
    public long Version { get; set; } = -1;

    public IEnumerable<BaseDomainEvent> GetUncommitedChanges()
    {
        return _changes;
    }

    public void MarkChangesAsCommited()
    {
        _changes.Clear();
    }

    private void ApplyChange(BaseDomainEvent @event, long version)
    {
        var method = this.GetType().GetMethod("Apply", new[] { @event.GetType() });
        if (method == null)
        {
            throw new ArgumentNullException($"Apply method not found for event : {@event.GetType().Name}");
        }
        method.Invoke(this, new object[]{@event});
        if (!_changes.Any(x => Equals(x.EventId, @event.EventId)))
        {
            _changes.Add(@event);
            Version = version;
        }
        else
        {
            if (nameof(@event).Equals(nameof(SavingsAccountCreated)))
            {
                var @eventTemp = @event as SavingsAccountCreated;
                throw new DuplicateNameException($"Account already exists for {@eventTemp.AccountId}");
            }
        }
    }

    protected void RaiseEvent(BaseDomainEvent @event)
    {
        ApplyChange(@event, @event.Version);
    }

    public void ReplayEvents(IEnumerable<BaseDomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyChange(@event, @event.Version);
        }
    }
}