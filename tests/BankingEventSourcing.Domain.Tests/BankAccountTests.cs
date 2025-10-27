using BankingEventSourcing.Domain.Aggregates;
using BankingEventSourcing.Domain.Events;
using BankingEventSourcing.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BankingEventSourcing.Domain.Tests;

public class BankAccountTests
{
    [Fact]
    public void OpenAccount_ShouldCreateAccountWithInitialBalance()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var name = "John Doe";
        var email = "john@example.com";
        var initialDeposit = 1000m;

        // Act
        var account = BankAccount.OpenAccount(accountId, name, email, initialDeposit);

        // Assert
        account.Id.Should().Be(accountId);
        account.AccountHolderName.Should().Be(name);
        account.Email.Should().Be(email);
        account.Balance.Should().Be(initialDeposit);
        account.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void OpenAccount_ShouldEmitAccountOpenedEvent()
    {
        // Arrange & Act
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 1000m);

        // Assert
        var events = account.GetUncommittedEvents();
        events.Should().ContainSingle();
        events[0].Should().BeOfType<AccountOpened>();
    }

    [Fact]
    public void Deposit_ShouldIncreaseBalance()
    {
        // Arrange
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 1000m);

        // Act
        account.Deposit(500m, "Salary");

        // Assert
        account.Balance.Should().Be(1500m);
    }

    [Fact]
    public void Withdraw_ShouldDecreaseBalance()
    {
        // Arrange
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 1000m);

        // Act
        account.Withdraw(300m, "Rent");

        // Assert
        account.Balance.Should().Be(700m);
    }

    [Fact]
    public void Withdraw_WithInsufficientFunds_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 100m);

        // Act & Assert
        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(200m, "Purchase"));
    }

    [Fact]
    public void Close_WithPositiveBalance_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 1000m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => account.Close("Test"));
        exception.Message.Should().Contain("positive balance");
    }

    [Fact]
    public void Close_WithZeroBalance_ShouldCloseAccount()
    {
        // Arrange
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 100m);
        account.Withdraw(100m, "Withdraw all");

        // Act
        account.Close("Customer request");

        // Assert
        account.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void Deposit_OnClosedAccount_ShouldThrowException()
    {
        // Arrange
        var account = BankAccount.OpenAccount(Guid.NewGuid(), "John", "john@test.com", 100m);
        account.Withdraw(100m, "Empty account");
        account.Close("Test");

        // Act & Assert
        Assert.Throws<AccountClosedException>(() => account.Deposit(50m, "Try deposit"));
    }

    [Fact]
    public void LoadFromHistory_ShouldReconstructState()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var events = new List<object>
        {
            new AccountOpened(accountId, "John", "john@test.com", 1000m, DateTime.UtcNow),
            new MoneyDeposited(accountId, 500m, "Salary", DateTime.UtcNow),
            new MoneyWithdrawn(accountId, 300m, "Rent", DateTime.UtcNow)
        };

        // Act
        var account = new BankAccount();
        account.LoadFromHistory(events);

        // Assert
        account.Balance.Should().Be(1200m);
        account.Version.Should().Be(3);
    }
}
