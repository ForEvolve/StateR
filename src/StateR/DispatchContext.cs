using System;
namespace StateR
{
    public class DispatchContext<TAction> : IDispatchContext<TAction>
        where TAction : IAction
    {
        public DispatchContext(TAction action, IDispatcher dispatcher)
        {
            Action = action;
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public IDispatcher Dispatcher { get; }
        public TAction Action { get; set; }

        public bool StopUpdate { get; set; }
        public bool StopInterception { get; set; }
        public bool StopAfterEffect { get; set; }

        public void DoNotContinue()
        {
            StopAfterEffect = true;
            StopInterception = true;
            StopUpdate = true;
        }
    }
}
