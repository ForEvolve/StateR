using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors
{
    public interface IActionInterceptor<TAction>
        where TAction : IAction
    {
        Task InterceptAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken);
    }
}
