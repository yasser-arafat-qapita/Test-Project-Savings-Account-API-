using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Projections.Handlers;

namespace Projections.Services;

public class SavingsAccountProjectionService : BackgroundService
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly IConfiguration _configuration; 
    private readonly MongoProjectionHandler _mongoProjectionHandler; 
    private readonly ILogger<SavingsAccountProjectionService> _logger;

    public SavingsAccountProjectionService(EventStoreClient eventStoreClient, 
        ILogger<SavingsAccountProjectionService> logger, IConfiguration configuration, MongoProjectionHandler mongoProjectionHandler)
    {
        _logger = logger;
        _eventStoreClient = eventStoreClient;
        _configuration = configuration;
        _mongoProjectionHandler = mongoProjectionHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mongo Projection Service Started!");

        var streamPrefix = _configuration.GetSection("EventStoreSettings:EventStoreStreamPrefix").Value;
        if (string.IsNullOrEmpty(streamPrefix))
        {
            _logger.LogError("EventStoreStreamPrefix is not configured.");
            return;
        }
        
        var subscriptionFilter = new SubscriptionFilterOptions(
            StreamFilter.Prefix(streamPrefix)
        );

        _logger.LogInformation("Started Streaming Events!"); 

        try
        {
            await _eventStoreClient.SubscribeToAllAsync(
                start: FromAll.Start,
                eventAppeared: async (subscription, resolvedEvent, cancellationToken) =>
                {
                    try
                    {
                        _logger.LogInformation($"Event received: {resolvedEvent.Event.EventStreamId}");
                        await _mongoProjectionHandler.Handle(resolvedEvent.Event);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event");
                    }
                }, 
                cancellationToken: stoppingToken, filterOptions: subscriptionFilter);

            _logger.LogInformation("Subscription completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during subscription");
        }
    }
}