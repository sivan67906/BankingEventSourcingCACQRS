namespace BankingEventSourcing.Domain.Events;

public record MoneyWithdrawn(
    Guid AccountId,
    decimal Amount,
    string Description,
    DateTime OccurredAt
) : DomainEvent;
