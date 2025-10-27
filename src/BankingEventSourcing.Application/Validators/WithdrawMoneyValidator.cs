using BankingEventSourcing.Application.Commands;
using FluentValidation;

namespace BankingEventSourcing.Application.Validators;

public class WithdrawMoneyValidator : AbstractValidator<WithdrawMoneyCommand>
{
    public WithdrawMoneyValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Account ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Withdrawal amount must be positive")
            .LessThanOrEqualTo(50000).WithMessage("Cannot withdraw more than 50,000 in single transaction");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}
