using BankingEventSourcing.Application.Commands;
using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BankingEventSourcing.Application.CommandHandlers;

public class OpenAccountHandler : IRequestHandler<OpenAccountCommand, Guid>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<OpenAccountHandler> _logger;

    public OpenAccountHandler(IEventStore eventStore, ILogger<OpenAccountHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<Guid> Handle(OpenAccountCommand request, CancellationToken cancellationToken)
    {
        var accountId = Guid.NewGuid();
        _logger.LogInformation("Opening new account for {AccountHolderName}", request.AccountHolderName);

        var account = BankAccount.OpenAccount(accountId, request.AccountHolderName, request.Email, request.InitialDeposit);

        await _eventStore.AppendEventsAsync(accountId.ToString(), account.GetUncommittedEvents(), expectedVersion: -1, cancellationToken);
        
        _logger.LogInformation("Account {AccountId} opened successfully", accountId);
        return accountId;
    }
}
