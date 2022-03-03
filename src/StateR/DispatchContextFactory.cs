namespace StateR;

public class DispatchContextFactory : IDispatchContextFactory
{
    public IDispatchContext<TAction, TState> Create<TAction, TState>(TAction action, IDispatcher dispatcher, CancellationTokenSource cancellationTokenSource)
        where TAction : IAction<TState>
        where TState : StateBase
        => new DispatchContext<TAction, TState>(action, dispatcher, cancellationTokenSource);
}
