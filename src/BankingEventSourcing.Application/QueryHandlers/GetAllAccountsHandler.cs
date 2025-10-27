using BankingEventSourcing.Application.DTOs;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Application.Queries;
using BankingEventSourcing.Domain.Aggregates;
using MediatR;

namespace BankingEventSourcing.Application.QueryHandlers;

public class GetAllAccountsHandler : IRequestHandler<GetAllAccountsQuery, List<AccountDto>>
{
    private readonly IEventStore _eventStore;

    public GetAllAccountsHandler(IEventStore eventStore) => _eventStore = eventStore;

    public async Task<List<AccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
    {
        var allStreams = await _eventStore.GetAllStreamsAsync(cancellationToken);
        var accounts = new List<AccountDto>();

        foreach (var streamId in allStreams)
        {
            var events = await _eventStore.GetEventsAsync(streamId, cancellationToken);
            
            if (events.Any())
            {
                var account = new BankAccount();
                account.LoadFromHistory(events);

                accounts.Add(new AccountDto
                {
                    AccountId = account.Id,
                    AccountHolderName = account.AccountHolderName,
                    Email = account.Email,
                    Balance = account.Balance,
                    IsClosed = account.IsClosed,
                    CreatedAt = account.CreatedAt,
                    ClosedAt = account.ClosedAt,
                    Version = account.Version
                });
            }
        }

        return accounts;
    }
}
