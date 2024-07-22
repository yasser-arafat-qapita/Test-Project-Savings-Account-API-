using System.Text.Json;
using Domain.Events;
using EventStore.Client;
using Microsoft.Extensions.Logging;
using Projections.Repositories;

namespace Projections.Handlers;

public class MongoProjectionHandler
{
    private readonly ILogger<MongoProjectionHandler> _logger;
    private readonly MongoRepository _mongoRepository;
    public MongoProjectionHandler(ILogger<MongoProjectionHandler> logger, MongoRepository mongoRepository)
    {
        _logger = logger;
        _mongoRepository = mongoRepository;
    }
    public async Task Handle(EventRecord @event)
    {
        var primaryId = GetValidEventPrimaryId(@event);
        if (!string.IsNullOrEmpty(primaryId))
        {
            await _mongoRepository.SaveAsync(primaryId);
        }
    }

    private string GetValidEventPrimaryId(EventRecord @event)
    {
        switch (@event.EventType)
        {
            case nameof(SavingsAccountCreated): return JsonSerializer.Deserialize<SavingsAccountCreated>(@event.Data.Span).AccountId;
            case nameof(SavingsAccountUpdated): return JsonSerializer.Deserialize<SavingsAccountUpdated>(@event.Data.Span).AccountId;
            default: return new string("");
        }
    }
}
