namespace BankingEventSourcing.Domain.Events;

public record MoneyDeposited(
    Guid AccountId,
    decimal Amount,
    string Description,
    DateTime OccurredAt
) : DomainEvent;
