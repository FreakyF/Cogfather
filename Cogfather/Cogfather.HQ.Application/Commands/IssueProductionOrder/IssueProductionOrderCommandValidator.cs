using FluentValidation;

namespace Cogfather.HQ.Application.Commands.IssueProductionOrder;

public class IssueProductionOrderCommandValidator : AbstractValidator<IssueProductionOrderCommand>
{
    public IssueProductionOrderCommandValidator()
    {
        RuleFor(x => x.RecipeId)
            .NotEmpty()
            .WithMessage("Recipe ID cannot be empty.");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0)
            .WithMessage("Target amount must be greater than zero.");
    }
}