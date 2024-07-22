using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class EventStoreExtension
{
    public static IServiceCollection AddEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("EventStoreSettings:ConnectionString").Value;
        var settings = EventStoreClientSettings.Create(connectionString);
        var client = new EventStoreClient(settings);
        services.AddSingleton(client);
        return services;
    }
}