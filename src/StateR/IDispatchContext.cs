namespace StateR
{
    public interface IDispatchContext<TAction>
        where TAction : IAction
    {
        TAction Action { get; set; }
        bool StopReduce { get; set; }
        bool StopInterception { get; set; }
        bool StopAfterEffect { get; set; }
    }
}
