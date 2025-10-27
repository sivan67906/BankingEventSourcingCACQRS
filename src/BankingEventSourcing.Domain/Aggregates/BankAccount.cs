using BankingEventSourcing.Domain.Events;
using BankingEventSourcing.Domain.Exceptions;

namespace BankingEventSourcing.Domain.Aggregates;

public class BankAccount : AggregateRoot<Guid>
{
    public string AccountHolderName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public bool IsClosed { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // Public parameterless constructor for event replay
    public BankAccount() { }

    public static BankAccount OpenAccount(Guid accountId, string accountHolderName, string email, decimal initialDeposit)
    {
        if (string.IsNullOrWhiteSpace(accountHolderName))
            throw new ArgumentException("Account holder name is required", nameof(accountHolderName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (initialDeposit < 0)
            throw new ArgumentException("Initial deposit cannot be negative", nameof(initialDeposit));

        var account = new BankAccount();
        account.ApplyChange(new AccountOpened(accountId, accountHolderName, email, initialDeposit, DateTime.UtcNow));
        return account;
    }

    public void Deposit(decimal amount, string description)
    {
        if (IsClosed)
            throw new AccountClosedException(Id);

        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));

        ApplyChange(new MoneyDeposited(Id, amount, description, DateTime.UtcNow));
    }

    public void Withdraw(decimal amount, string description)
    {
        if (IsClosed)
            throw new AccountClosedException(Id);

        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));

        if (Balance < amount)
            throw new InsufficientFundsException(Id, Balance, amount);

        ApplyChange(new MoneyWithdrawn(Id, amount, description, DateTime.UtcNow));
    }

    public void Close(string reason)
    {
        if (IsClosed)
            throw new InvalidOperationException("Account is already closed");

        if (Balance > 0)
            throw new InvalidOperationException("Cannot close account with positive balance");

        ApplyChange(new AccountClosed(Id, reason, DateTime.UtcNow));
    }

    protected override void Apply(object @event)
    {
        switch (@event)
        {
            case AccountOpened e: Apply(e); break;
            case MoneyDeposited e: Apply(e); break;
            case MoneyWithdrawn e: Apply(e); break;
            case AccountClosed e: Apply(e); break;
        }
    }

    private void Apply(AccountOpened @event)
    {
        Id = @event.AccountId;
        AccountHolderName = @event.AccountHolderName;
        Email = @event.Email;
        Balance = @event.InitialDeposit;
        IsClosed = false;
        CreatedAt = @event.OccurredAt;
    }

    private void Apply(MoneyDeposited @event) => Balance += @event.Amount;
    private void Apply(MoneyWithdrawn @event) => Balance -= @event.Amount;
    private void Apply(AccountClosed @event)
    {
        IsClosed = true;
        ClosedAt = @event.OccurredAt;
    }
}
