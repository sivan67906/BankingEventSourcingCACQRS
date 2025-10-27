using BankingEventSourcing.Application.DTOs;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Application.Queries;
using BankingEventSourcing.Domain.Aggregates;
using MediatR;

namespace BankingEventSourcing.Application.QueryHandlers;

public class GetAccountHandler : IRequestHandler<GetAccountQuery, AccountDto>
{
    private readonly IEventStore _eventStore;

    public GetAccountHandler(IEventStore eventStore) => _eventStore = eventStore;

    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventStore.GetEventsAsync(request.AccountId.ToString(), cancellationToken);
        
        if (!events.Any())
            throw new KeyNotFoundException($"Account {request.AccountId} not found");

        var account = new BankAccount();
        account.LoadFromHistory(events);

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
