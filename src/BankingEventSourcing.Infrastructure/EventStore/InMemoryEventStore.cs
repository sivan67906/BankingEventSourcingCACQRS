using BankingEventSourcing.Application.Interfaces;

namespace BankingEventSourcing.Infrastructure.EventStore;

public class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<string, List<StoredEvent>> _streams = new();
    private readonly object _lock = new();

    public Task AppendEventsAsync(string streamId, IEnumerable<object> events, int expectedVersion, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_streams.ContainsKey(streamId))
                _streams[streamId] = new List<StoredEvent>();

            var stream = _streams[streamId];
            
            if (stream.Count != expectedVersion + 1)
                throw new InvalidOperationException($"Concurrency conflict: Expected version {expectedVersion}, current is {stream.Count - 1}");

            foreach (var @event in events)
            {
                stream.Add(new StoredEvent
                {
                    Event = @event,
                    Version = stream.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task<List<object>> GetEventsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_streams.ContainsKey(streamId))
                return Task.FromResult(new List<object>());

            var events = _streams[streamId].OrderBy(e => e.Version).Select(e => e.Event).ToList();
            return Task.FromResult(events);
        }
    }

    public Task<List<string>> GetAllStreamsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_streams.Keys.ToList());
        }
    }

    public Task RebuildProjectionsAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private class StoredEvent
    {
        public object Event { get; set; } = default!;
        public int Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
