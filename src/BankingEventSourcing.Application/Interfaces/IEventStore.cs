namespace BankingEventSourcing.Application.Interfaces;

public interface IEventStore
{
    Task AppendEventsAsync(string streamId, IEnumerable<object> events, int expectedVersion, CancellationToken cancellationToken = default);
    Task<List<object>> GetEventsAsync(string streamId, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllStreamsAsync(CancellationToken cancellationToken = default);
    Task RebuildProjectionsAsync(CancellationToken cancellationToken = default);
}
