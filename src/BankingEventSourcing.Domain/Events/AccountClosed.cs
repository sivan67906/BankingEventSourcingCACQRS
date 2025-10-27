namespace BankingEventSourcing.Domain.Events;

public record AccountClosed(
    Guid AccountId,
    string Reason,
    DateTime OccurredAt
) : DomainEvent;
