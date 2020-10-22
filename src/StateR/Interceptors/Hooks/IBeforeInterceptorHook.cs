using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors.Hooks
{
    public interface IBeforeInterceptorHook
    {
        Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
    }
}
