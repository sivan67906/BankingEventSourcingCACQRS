using BankingEventSourcing.Application.Interfaces;
using Marten;

namespace BankingEventSourcing.Infrastructure.EventStore;

public class MartenEventStore : IEventStore
{
    private readonly IDocumentStore _documentStore;

    public MartenEventStore(IDocumentStore documentStore) => _documentStore = documentStore;

    public async Task AppendEventsAsync(string streamId, IEnumerable<object> events, int expectedVersion, CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.LightweightSession();
        session.Events.Append(streamId, expectedVersion, events.ToArray());
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<object>> GetEventsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var events = await session.Events.FetchStreamAsync(streamId, token: cancellationToken);
        return events.Select(e => e.Data).ToList();
    }

    public async Task<List<string>> GetAllStreamsAsync(CancellationToken cancellationToken = default)
    {
        await using var session = _documentStore.QuerySession();
        var streams = await session.Events.QueryAllRawEvents()
            .Select(x => x.StreamKey)
            .Distinct()
            .ToListAsync(cancellationToken);
        return streams.ToList();
    }

    public async Task RebuildProjectionsAsync(CancellationToken cancellationToken = default)
    {
        using var daemon = await _documentStore.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync("AccountSummary", cancellationToken);
    }
}
