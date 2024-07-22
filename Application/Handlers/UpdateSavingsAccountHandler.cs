using Application.Commands;
using Domain.Events;
using EventStore.Client;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Application.Handlers;

public class UpdateSavingsAccountHandler : IHandleMessages<UpdateSavingsAccountCommand>
{
    private readonly EventStoreClient _eventStoreClient;
    private ILogger<UpdateSavingsAccountHandler> _logger;
    private readonly SavingsAccountRepository _savingsAccountRepository;
    private readonly IConfiguration _configuration;
    public UpdateSavingsAccountHandler(EventStoreClient eventStoreClient, IConfiguration configuration,
        ILogger<UpdateSavingsAccountHandler> logger, SavingsAccountRepository savingsAccountRepository)
    {
        _configuration = configuration;
        _eventStoreClient = eventStoreClient;
        _logger = logger;
        _savingsAccountRepository = savingsAccountRepository;
    }

    public async Task Handle(UpdateSavingsAccountCommand message, IMessageHandlerContext context)
    {
        var aggregate = await _savingsAccountRepository.GetByIdAsync(message.AccountId);
        aggregate.UpdateSavingsAccount(message.Amount, message.TransactionType, aggregate.Version);
        _logger.LogInformation("Savings Account Aggregate created!");
        var lastEventApplied = (SavingsAccountUpdated) aggregate.GetUncommitedChanges().LastOrDefault();
        var eventData = lastEventApplied.ToEventData();
        var streamPrefix = _configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value;
        await _eventStoreClient.AppendToStreamAsync(
            $"{streamPrefix}-{message.AccountId}",
            StreamState.Any,
            new[] { eventData });
        _logger.LogInformation("UpdateSavings Account EventData Published!");
        aggregate.MarkChangesAsCommited();
        _logger.LogInformation($"Aggregate for command {nameof(UpdateSavingsAccountCommand)} marked commited!");
        
    }
}