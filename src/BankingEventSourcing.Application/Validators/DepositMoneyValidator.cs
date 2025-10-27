using BankingEventSourcing.Application.Commands;
using FluentValidation;

namespace BankingEventSourcing.Application.Validators;

public class DepositMoneyValidator : AbstractValidator<DepositMoneyCommand>
{
    public DepositMoneyValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Account ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Deposit amount must be positive")
            .LessThanOrEqualTo(100000).WithMessage("Cannot deposit more than 100,000 in single transaction");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}
