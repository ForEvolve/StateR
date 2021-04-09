namespace StateR
{
    public interface IDispatchContextFactory
    {
        IDispatchContext<TAction> Create<TAction>(TAction action, IDispatcher dispatcher) where TAction : IAction;
    }
}
