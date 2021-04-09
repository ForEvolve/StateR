namespace StateR
{
    public interface IDispatchContext<TAction>
        where TAction : IAction
    {
        IDispatcher Dispatcher { get; }
        TAction Action { get; set; }
        bool StopReduce { get; set; }
        bool StopInterception { get; set; }
        bool StopAfterEffect { get; set; }
    }
}
