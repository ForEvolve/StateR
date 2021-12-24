namespace StateR;

public interface IDispatcher
{
    Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken) where TAction : IAction;
}
