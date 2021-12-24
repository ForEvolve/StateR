using Microsoft.Extensions.DependencyInjection;
using StateR.Interceptors.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors
{
    public class InterceptorsManager : IInterceptorsManager
    {
        private readonly IInterceptorsHooksCollection _hooks;
        private readonly IServiceProvider _serviceProvider;

        public InterceptorsManager(IInterceptorsHooksCollection hooks, IServiceProvider serviceProvider)
        {
            _hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext) where TAction : IAction
        {
            var interceptors = _serviceProvider.GetServices<IInterceptor<TAction>>().ToList();
            foreach (var interceptor in interceptors)
            {
                dispatchContext.CancellationToken.ThrowIfCancellationRequested();
                
                await _hooks.BeforeHandlerAsync(dispatchContext, interceptor, dispatchContext.CancellationToken);
                await interceptor.InterceptAsync(dispatchContext, dispatchContext.CancellationToken);
                await _hooks.AfterHandlerAsync(dispatchContext, interceptor, dispatchContext.CancellationToken);
            }
        }
    }
}
