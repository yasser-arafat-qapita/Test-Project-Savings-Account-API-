using System.Text.Json;
using EventStore.Client;
namespace Infrastructure.Extensions;

public static class EventDataExtensions
{
    public static EventData ToEventData<T>(this T @event)
    {
        return new EventData(
            Uuid.NewUuid(),
            @event.GetType().Name.ToString(),
            JsonSerializer.SerializeToUtf8Bytes(@event)
            );
    }
}
