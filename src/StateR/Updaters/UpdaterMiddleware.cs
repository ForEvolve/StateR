using StateR.Pipeline;

namespace StateR.Updaters;

public class UpdaterMiddleware<TState, TAction> : IActionFilter<TAction, TState>
    where TState : StateBase
    where TAction : IAction<TState>
{
    private readonly IEnumerable<IUpdater<TAction, TState>> _updaters;
    private readonly IState<TState> _state;

    public UpdaterMiddleware(IState<TState> state, IEnumerable<IUpdater<TAction, TState>> updaters)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _updaters = updaters ?? throw new ArgumentNullException(nameof(updaters));
    }

    public Task InvokeAsync(IDispatchContext<TAction, TState> context, ActionDelegate<TAction, TState>? next, CancellationToken cancellationToken)
    {
        foreach (var updater in _updaters)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            _state.Set(updater.Update(context.Action, _state.Current));
        }
        _state.Notify();
        cancellationToken.ThrowIfCancellationRequested();

        next?.Invoke(context, cancellationToken);
        return Task.CompletedTask;
    }
}
