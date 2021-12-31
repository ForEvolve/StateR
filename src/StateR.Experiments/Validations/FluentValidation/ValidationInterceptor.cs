using FluentValidation;
using FluentValidation.Results;
using StateR.Interceptors;
using StateR.Updaters;
using System.Collections.Immutable;

namespace StateR.Validations.FluentValidation;

public class ValidationInterceptor<TAction> : IInterceptor<TAction>
    where TAction : IAction
{
    private readonly IEnumerable<IValidator<TAction>> _validators;
    public ValidationInterceptor(IEnumerable<IValidator<TAction>> validators)
    {
        _validators = validators;
    }

    public async Task InterceptAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
    {
        var result = _validators
            .Select(validator => validator.Validate(context.Action));
        if (result?.Any(validator => !validator.IsValid) ?? false)
        {
            var errors = result
                .Where(validator => !validator.IsValid)
                .SelectMany(validator => validator.Errors);
            await context.Dispatcher.DispatchAsync(new ReplaceValidationError(errors), cancellationToken);
            context.Cancel();
        }
    }
}

public record class ValidationState(ImmutableList<ValidationFailure> Errors) : StateBase
{
    public bool HasErrors() => Errors.Any();
}
public class ValidationInitialState : IInitialState<ValidationState>
{
    public ValidationState Value => new(ImmutableList.Create<ValidationFailure>());
}
public record class ReplaceValidationError(IEnumerable<ValidationFailure> Errors) : IAction;
public record class CleanValidationError() : IAction;

public class ValidationUpdaters : IUpdater<ReplaceValidationError, ValidationState>, IUpdater<CleanValidationError, ValidationState>
{
    public ValidationState Update(ReplaceValidationError action, ValidationState state)
         => state with { Errors = ImmutableList.Create(action.Errors.ToArray()) };

    public ValidationState Update(CleanValidationError action, ValidationState state)
        => state with { Errors = ImmutableList.Create<ValidationFailure>() };
}