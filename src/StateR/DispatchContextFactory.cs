namespace StateR
{
    public class DispatchContextFactory : IDispatchContextFactory
    {
        public IDispatchContext<TAction> Create<TAction>(TAction action)
            where TAction : IAction
            => new DispatchContext<TAction>(action);
    }
}
