using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Reducers
{
    public class ReducersManager : IReducersManager
    {
        private readonly IEnumerable<IActionHandlerMiddleware> _middlewares;
        private readonly IServiceProvider _serviceProvider;

        public ReducersManager(IEnumerable<IActionHandlerMiddleware> middlewares, IServiceProvider serviceProvider)
        {
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var reducerHandlers = _serviceProvider.GetServices<IActionHandler<TAction>>().ToList();
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeHandlersAsync(dispatchContext, reducerHandlers, cancellationToken);
            }
            foreach (var handler in reducerHandlers)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeHandlerAsync(dispatchContext, handler, cancellationToken);
                }
                if (dispatchContext.StopReduce)
                {
                    break;
                }
                await handler.HandleAsync(dispatchContext, cancellationToken);
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterHandlerAsync(dispatchContext, handler, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterHandlersAsync(dispatchContext, reducerHandlers, cancellationToken);
            }
        }
    }
}
