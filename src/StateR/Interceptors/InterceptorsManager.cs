using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors
{
    public class InterceptorsManager : IInterceptorsManager
    {
        private readonly IEnumerable<IInterceptorsMiddleware> _middlewares;
        private readonly IServiceProvider _serviceProvider;

        public InterceptorsManager(IEnumerable<IInterceptorsMiddleware> middlewares, IServiceProvider serviceProvider)
        {
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var interceptors = _serviceProvider.GetServices<IActionInterceptor<TAction>>().ToList();
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeInterceptorsAsync(dispatchContext, interceptors, cancellationToken);
            }
            foreach (var interceptor in interceptors)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeInterceptorAsync(dispatchContext, interceptor, cancellationToken);
                }
                if (dispatchContext.StopInterception)
                {
                    break;
                }
                await interceptor.InterceptAsync(dispatchContext, cancellationToken);
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterInterceptorAsync(dispatchContext, interceptor, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterInterceptorsAsync(dispatchContext, interceptors, cancellationToken);
            }
        }
    }
}
