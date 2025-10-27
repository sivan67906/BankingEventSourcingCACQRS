using BankingEventSourcing.Application.Commands;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingEventSourcing.Application.CommandHandlers;

public class DepositMoneyHandler : IRequestHandler<DepositMoneyCommand, Unit>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<DepositMoneyHandler> _logger;

    public DepositMoneyHandler(IEventStore eventStore, ILogger<DepositMoneyHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<Unit> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing deposit of {Amount:C} to account {AccountId}", request.Amount, request.AccountId);

        var events = await _eventStore.GetEventsAsync(request.AccountId.ToString(), cancellationToken);
        var account = new BankAccount();
        account.LoadFromHistory(events);

        account.Deposit(request.Amount, request.Description);

        await _eventStore.AppendEventsAsync(request.AccountId.ToString(), account.GetUncommittedEvents(), expectedVersion: account.Version, cancellationToken);
        
        _logger.LogInformation("Deposit completed for account {AccountId}", request.AccountId);
        return Unit.Value;
    }
}
