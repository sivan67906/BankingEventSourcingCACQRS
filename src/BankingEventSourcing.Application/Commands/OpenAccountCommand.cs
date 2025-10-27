using MediatR;

namespace BankingEventSourcing.Application.Commands;

public record OpenAccountCommand(string AccountHolderName, string Email, decimal InitialDeposit) : IRequest<Guid>;
