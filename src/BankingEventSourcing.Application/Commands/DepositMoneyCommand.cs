using MediatR;

namespace BankingEventSourcing.Application.Commands;

public record DepositMoneyCommand(Guid AccountId, decimal Amount, string Description) : IRequest<Unit>;
