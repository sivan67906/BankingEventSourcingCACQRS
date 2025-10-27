using BankingEventSourcing.Application.Commands;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingEventSourcing.Application.CommandHandlers;

public class WithdrawMoneyHandler : IRequestHandler<WithdrawMoneyCommand, Unit>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<WithdrawMoneyHandler> _logger;

    public WithdrawMoneyHandler(IEventStore eventStore, ILogger<WithdrawMoneyHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<Unit> Handle(WithdrawMoneyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing withdrawal of {Amount:C} from account {AccountId}", request.Amount, request.AccountId);

        var events = await _eventStore.GetEventsAsync(request.AccountId.ToString(), cancellationToken);
        var account = new BankAccount();
        account.LoadFromHistory(events);

        account.Withdraw(request.Amount, request.Description);

        await _eventStore.AppendEventsAsync(request.AccountId.ToString(), account.GetUncommittedEvents(), expectedVersion: account.Version, cancellationToken);
        
        _logger.LogInformation("Withdrawal completed for account {AccountId}", request.AccountId);
        return Unit.Value;
    }
}
