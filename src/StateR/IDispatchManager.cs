using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IDispatchManager
    {
        Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken)
            where TAction : IAction;
    }
}
