
namespace StateR.Pipeline;

public interface IActionFilter<TAction, TState>
    where TAction : IAction<TState>
    where TState : StateBase
{
    Task InvokeAsync(IDispatchContext<TAction, TState> context, ActionDelegate<TAction, TState>? next, CancellationToken cancellationToken);
}

public delegate Task ActionDelegate<TAction, TState>(IDispatchContext<TAction, TState> context, CancellationToken cancellationToken)
    where TAction : IAction<TState>
    where TState : StateBase;
