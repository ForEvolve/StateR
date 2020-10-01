using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors
{
    public interface IInterceptorsMiddleware
    {
        Task BeforeInterceptorsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionInterceptor<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
        Task BeforeInterceptorAsync<TAction>(IDispatchContext<TAction> context, IActionInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterInterceptorAsync<TAction>(IDispatchContext<TAction> context, IActionInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterInterceptorsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionInterceptor<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
    }
}
