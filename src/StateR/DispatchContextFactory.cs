namespace StateR;

public class DispatchContextFactory : IDispatchContextFactory
{
    public IDispatchContext<TAction> Create<TAction>(TAction action, IDispatcher dispatcher, CancellationTokenSource cancellationTokenSource)
        where TAction : IAction
        => new DispatchContext<TAction>(action, dispatcher, cancellationTokenSource);
}
