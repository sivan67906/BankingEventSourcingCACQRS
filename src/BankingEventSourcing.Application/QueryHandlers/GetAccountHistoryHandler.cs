using BankingEventSourcing.Application.DTOs;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Application.Queries;
using BankingEventSourcing.Domain.Events;
using MediatR;

namespace BankingEventSourcing.Application.QueryHandlers;

public class GetAccountHistoryHandler : IRequestHandler<GetAccountHistoryQuery, List<EventDto>>
{
    private readonly IEventStore _eventStore;

    public GetAccountHistoryHandler(IEventStore eventStore) => _eventStore = eventStore;

    public async Task<List<EventDto>> Handle(GetAccountHistoryQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventStore.GetEventsAsync(request.AccountId.ToString(), cancellationToken);

        return events.Select((e, index) => new EventDto
        {
            EventType = e.GetType().Name,
            EventData = System.Text.Json.JsonSerializer.Serialize(e),
            OccurredAt = e is DomainEvent de ? de.OccurredAt : DateTime.MinValue,
            Version = index + 1
        }).ToList();
    }
}
