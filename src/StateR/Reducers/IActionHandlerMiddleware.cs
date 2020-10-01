using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Reducers
{
    public interface IActionHandlerMiddleware
    {
        Task BeforeHandlersAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionHandler<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
        Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterHandlersAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionHandler<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
    }
}
