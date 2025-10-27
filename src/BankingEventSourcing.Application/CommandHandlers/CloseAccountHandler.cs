using BankingEventSourcing.Application.Commands;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingEventSourcing.Application.CommandHandlers;

public class CloseAccountHandler : IRequestHandler<CloseAccountCommand, Unit>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<CloseAccountHandler> _logger;

    public CloseAccountHandler(IEventStore eventStore, ILogger<CloseAccountHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<Unit> Handle(CloseAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Closing account {AccountId}", request.AccountId);

        var events = await _eventStore.GetEventsAsync(request.AccountId.ToString(), cancellationToken);
        var account = new BankAccount();
        account.LoadFromHistory(events);

        account.Close(request.Reason);

        await _eventStore.AppendEventsAsync(request.AccountId.ToString(), account.GetUncommittedEvents(), expectedVersion: account.Version, cancellationToken);
        
        _logger.LogInformation("Account {AccountId} closed successfully", request.AccountId);
        return Unit.Value;
    }
}
