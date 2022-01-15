namespace StateR;

public interface IDispatchContextFactory
{
    IDispatchContext<TAction, TState> Create<TAction, TState>(TAction action, IDispatcher dispatcher, CancellationTokenSource cancellationTokenSource)
        where TAction : IAction<TState>
        where TState : StateBase;
}
