using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cogfather.HQ.Application.Behaviors;
using Cogfather.HQ.Application.Commands.IssueProductionOrder;
using FluentValidation;
using MediatR;
using Xunit;

namespace Cogfather.HQ.Tests;

public class ValidationTests
{
    [Fact]
    public void Validator_ValidCommand_NoErrors()
    {
        var validator = new IssueProductionOrderCommandValidator();
        var result = validator.Validate(new IssueProductionOrderCommand("recipe1", 10));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_EmptyRecipeId_HasError()
    {
        var validator = new IssueProductionOrderCommandValidator();
        var result = validator.Validate(new IssueProductionOrderCommand("", 10));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RecipeId");
    }

    [Fact]
    public void Validator_ZeroAmount_HasError()
    {
        var validator = new IssueProductionOrderCommandValidator();
        var result = validator.Validate(new IssueProductionOrderCommand("recipe1", 0));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetAmount");
    }

    [Fact]
    public void Validator_NegativeAmount_HasError()
    {
        var validator = new IssueProductionOrderCommandValidator();
        var result = validator.Validate(new IssueProductionOrderCommand("recipe1", -5));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TargetAmount");
    }

    [Fact]
    public async Task ValidationBehavior_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<TestRequest, bool>(
            new List<IValidator<TestRequest>>());

        var called = false;
        var result = await behavior.Handle(
            new TestRequest(),
            _ => { called = true; return Task.FromResult(true); },
            CancellationToken.None);

        Assert.True(called);
        Assert.True(result);
    }

    [Fact]
    public async Task ValidationBehavior_WithFailingValidator_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, bool>(
            new List<IValidator<TestRequest>> { new FailingValidator() });

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(
                new TestRequest(),
                _ => Task.FromResult(true),
                CancellationToken.None));
    }

    private record TestRequest : IRequest<bool>;

    private class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator()
        {
            RuleFor(x => x).Must(_ => false).WithMessage("always fails");
        }
    }
}
