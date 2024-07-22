using Domain.Aggregates;
using Domain.Events;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Repository;

public class SavingsAccountRepository
{
    private readonly EventStoreClient _eventStoreClient;
    private ILogger<SavingsAccountRepository> _logger;
    private readonly IConfiguration _configuration;
    public SavingsAccountRepository(EventStoreClient eventStoreClient, ILogger<SavingsAccountRepository> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _eventStoreClient = eventStoreClient;
        _logger = logger;
    }

    public async Task<SavingsAccountAggregate> GetByIdAsync(string id)
    {
        var savingsAccountAggregate = new SavingsAccountAggregate();
        var streamName = $"{_configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value}-{id}";
        _logger.LogInformation($"Trying to read stream {streamName} from EventStoreDB");
        var events = _eventStoreClient.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);
        var resolvedEvents = await events.ToListAsync();
        if (resolvedEvents.Any())
        {
            var deserializedEvents = resolvedEvents.Select(resolvedEvent => (BaseDomainEvent)DeserializeEvent(resolvedEvent.Event)).ToList();
            savingsAccountAggregate.ReplayEvents(deserializedEvents);
            var latestVersion = resolvedEvents.Max(x => x.Event.EventNumber);
            savingsAccountAggregate.Version = latestVersion.ToInt64() + 1;
        }

        return savingsAccountAggregate;
    }
    private object? DeserializeEvent(EventRecord eventRecord)
    {
        var eventData = eventRecord.Data.ToArray();
        var eventType = eventRecord.EventType;

        return eventType switch
        {
            nameof(SavingsAccountCreated) => JsonConvert.DeserializeObject<SavingsAccountCreated>(System.Text.Encoding.UTF8.GetString(eventData)),
            nameof(SavingsAccountUpdated) => JsonConvert.DeserializeObject<SavingsAccountUpdated>(System.Text.Encoding.UTF8.GetString(eventData)),
            _ => throw new InvalidOperationException("Unknown event type")
        };
    }
}