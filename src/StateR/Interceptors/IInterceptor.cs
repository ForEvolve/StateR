using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors
{
    public interface IInterceptor<TAction>
        where TAction : IAction
    {
        Task InterceptAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken);
    }
}
