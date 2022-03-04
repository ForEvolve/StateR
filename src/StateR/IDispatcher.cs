namespace StateR;

public interface IDispatcher
{
    Task DispatchAsync(object action, CancellationToken cancellationToken);
    Task DispatchAsync<TAction, TState>(TAction action, CancellationToken cancellationToken)
        where TAction : IAction<TState>
        where TState : StateBase;
}
