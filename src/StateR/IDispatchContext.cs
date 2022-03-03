namespace StateR;

public interface IDispatchContext<TAction, TState>
    where TAction : IAction<TState>
    where TState : StateBase
{
    IDispatcher Dispatcher { get; }
    TAction Action { get; }

    CancellationToken CancellationToken { get; }
    void Cancel();
}
