namespace StateR;

public interface IDispatchManager
{
    Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext)
        where TAction : IAction;
}
