using Application.Commands;
using Domain.Aggregates;
using Domain.Events;
using EventStore.Client;
using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Application.Handlers;

public class CreateSavingsAccountHandler : IHandleMessages<CreateSavingsAccountCommand>
{
    private readonly EventStoreClient _eventStoreClient;
    private ILogger<CreateSavingsAccountHandler> _logger;
    private readonly IConfiguration _configuration;
    public CreateSavingsAccountHandler(EventStoreClient eventStoreClient, ILogger<CreateSavingsAccountHandler> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        _eventStoreClient = eventStoreClient;
        _logger = logger;
    }

    public async Task Handle(CreateSavingsAccountCommand message, IMessageHandlerContext context)
    {
        var aggregate = new SavingsAccountAggregate(message.AccountId, message.Balance);
        _logger.LogInformation("Savings Account Aggregate created!");
        var aggregateCreated = new SavingsAccountCreated()
        {
            AccountId = aggregate.AccountId,
            Balance = aggregate.Balance,
        };
        var streamPrefix = _configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value;
        var eventData = aggregateCreated.ToEventData();
        await _eventStoreClient.AppendToStreamAsync(
            $"{streamPrefix}-{message.AccountId}",
            StreamState.Any,
            new[] { eventData });
        _logger.LogInformation("Savings Account EventData Published!");
        aggregate.MarkChangesAsCommited();
        _logger.LogInformation($"Aggregate for command {nameof(CreateSavingsAccountCommand)} marked commited!");
    }
}