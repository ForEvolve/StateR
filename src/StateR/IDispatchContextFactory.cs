namespace StateR
{
    public interface IDispatchContextFactory
    {
        IDispatchContext<TAction> Create<TAction>(TAction action) where TAction : IAction;
    }
}
