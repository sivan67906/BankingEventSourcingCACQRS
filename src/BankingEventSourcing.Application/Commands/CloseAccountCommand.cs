using MediatR;

namespace BankingEventSourcing.Application.Commands;

public record CloseAccountCommand(Guid AccountId, string Reason) : IRequest<Unit>;
