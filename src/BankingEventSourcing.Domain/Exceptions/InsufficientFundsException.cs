namespace BankingEventSourcing.Domain.Exceptions;

public class InsufficientFundsException : Exception
{
    public Guid AccountId { get; }
    public decimal CurrentBalance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientFundsException(Guid accountId, decimal currentBalance, decimal requestedAmount)
        : base($"Insufficient funds in account {accountId}. Current: {currentBalance:C}, Requested: {requestedAmount:C}")
    {
        AccountId = accountId;
        CurrentBalance = currentBalance;
        RequestedAmount = requestedAmount;
    }
}
