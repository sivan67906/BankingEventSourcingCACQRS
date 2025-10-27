namespace BankingEventSourcing.Domain.Events;

public record AccountOpened(
    Guid AccountId,
    string AccountHolderName,
    string Email,
    decimal InitialDeposit,
    DateTime OccurredAt
) : DomainEvent;
