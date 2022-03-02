using FluentValidation;
using FluentValidation.Results;
using StateR.Pipeline;
using StateR.Updaters;
using System.Collections.Immutable;

namespace StateR.Validations.FluentValidation;

public class ValidationFilter<TAction, TState> : IActionFilter<TAction, TState>
    where TAction : IAction<TState>
    where TState : StateBase
{
    private readonly IEnumerable<IValidator<TAction>> _validators;
    public ValidationFilter(IEnumerable<IValidator<TAction>> validators)
    {
        _validators = validators;
    }

    public async Task InvokeAsync(IDispatchContext<TAction, TState> context, ActionDelegate<TAction, TState>? next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next, nameof(next));

        var result = _validators
            .Select(validator => validator.Validate(context.Action));
        if (result?.Any(validator => !validator.IsValid) ?? false)
        {
            var errors = result
                .Where(validator => !validator.IsValid)
                .SelectMany(validator => validator.Errors);
            await context.Dispatcher.DispatchAsync(new AddValidationErrors(errors), cancellationToken);
            context.Cancel();
        }
        try
        {
            await next(context, cancellationToken);
        }
        catch (ValidationException ex)
        {
            await context.Dispatcher.DispatchAsync(
                new AddValidationErrors(ex.Errors),
                context.CancellationToken
            );
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

public record class AddValidationErrors(IEnumerable<ValidationFailure> Errors) : IAction<ValidationState>;
public record class ReplaceValidationErrors(IEnumerable<ValidationFailure> Errors) : IAction<ValidationState>;
public record class CleanValidationError() : IAction<ValidationState>;
public record class RemoveValidationError(ValidationFailure Error) : IAction<ValidationState>;

public class ValidationUpdaters :
    IUpdater<ReplaceValidationErrors, ValidationState>,
    IUpdater<CleanValidationError, ValidationState>,
    IUpdater<AddValidationErrors, ValidationState>,
    IUpdater<RemoveValidationError, ValidationState>
{
    public ValidationState Update(ReplaceValidationErrors action, ValidationState state)
         => state with { Errors = ImmutableList.Create(action.Errors.ToArray()) };
    public ValidationState Update(CleanValidationError action, ValidationState state)
        => state with { Errors = ImmutableList.Create<ValidationFailure>() };
    public ValidationState Update(AddValidationErrors action, ValidationState state)
        => state with { Errors = state.Errors.AddRange(action.Errors) };
    public ValidationState Update(RemoveValidationError action, ValidationState state)
        => state with { Errors = state.Errors.Remove(action.Error) };
}