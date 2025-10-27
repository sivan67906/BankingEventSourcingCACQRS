using MediatR;

namespace BankingEventSourcing.Application.Commands;

public record WithdrawMoneyCommand(Guid AccountId, decimal Amount, string Description) : IRequest<Unit>;
