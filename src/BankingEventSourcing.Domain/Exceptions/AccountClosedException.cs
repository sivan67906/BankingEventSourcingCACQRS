namespace BankingEventSourcing.Domain.Exceptions;

public class AccountClosedException : Exception
{
    public Guid AccountId { get; }

    public AccountClosedException(Guid accountId)
        : base($"Account {accountId} is closed and cannot be modified")
    {
        AccountId = accountId;
    }
}
