namespace StateR;

public interface IDispatchManager
{
    Task DispatchAsync<TAction, TState>(IDispatchContext<TAction, TState> dispatchContext)
        where TAction : IAction<TState>
        where TState : StateBase;
}
