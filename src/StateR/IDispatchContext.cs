using System.Threading;

namespace StateR
{
    public interface IDispatchContext<TAction>
        where TAction : IAction
    {
        IDispatcher Dispatcher { get; }
        TAction Action { get; }

        CancellationToken CancellationToken { get; }
        void Cancel();
    }
}
