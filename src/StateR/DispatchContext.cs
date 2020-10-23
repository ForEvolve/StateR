namespace StateR
{
    public class DispatchContext<TAction> : IDispatchContext<TAction>
        where TAction : IAction
    {
        public DispatchContext(TAction action)
        {
            Action = action;
        }

        public TAction Action { get; set; }

        public bool StopReduce { get; set; }
        public bool StopInterception { get; set; }
        public bool StopAfterEffect { get; set; }
    }
}
