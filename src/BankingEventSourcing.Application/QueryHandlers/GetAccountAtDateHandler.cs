using BankingEventSourcing.Application.DTOs;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Application.Queries;
using BankingEventSourcing.Domain.Aggregates;
using BankingEventSourcing.Domain.Events;
using MediatR;

namespace BankingEventSourcing.Application.QueryHandlers;

public class GetAccountAtDateHandler : IRequestHandler<GetAccountAtDateQuery, AccountDto>
{
    private readonly IEventStore _eventStore;

    public GetAccountAtDateHandler(IEventStore eventStore) => _eventStore = eventStore;

    public async Task<AccountDto> Handle(GetAccountAtDateQuery request, CancellationToken cancellationToken)
    {
        var allEvents = await _eventStore.GetEventsAsync(request.AccountId.ToString(), cancellationToken);
        var eventsUpToDate = allEvents.Where(e => e is DomainEvent de && de.OccurredAt <= request.AsOfDate).ToList();

        if (!eventsUpToDate.Any())
            throw new KeyNotFoundException($"Account {request.AccountId} did not exist at {request.AsOfDate:yyyy-MM-dd}");

        var account = new BankAccount();
        account.LoadFromHistory(eventsUpToDate);

        return new AccountDto
        {
            AccountId = account.Id,
            AccountHolderName = account.AccountHolderName,
            Email = account.Email,
            Balance = account.Balance,
            IsClosed = account.IsClosed,
            CreatedAt = account.CreatedAt,
            ClosedAt = account.ClosedAt,
            Version = account.Version
        };
    }
}
