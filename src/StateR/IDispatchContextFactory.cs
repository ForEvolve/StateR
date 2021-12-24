using System.Threading;

namespace StateR;

public interface IDispatchContextFactory
{
    IDispatchContext<TAction> Create<TAction>(TAction action, IDispatcher dispatcher, CancellationTokenSource cancellationTokenSource) where TAction : IAction;
}
