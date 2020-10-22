using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Interceptors.Hooks
{
    public class InterceptorsHooksCollection : IInterceptorsHooksCollection
    {
        private readonly IEnumerable<IBeforeInterceptorHook> _beforeInterceptorHooks;
        private readonly IEnumerable<IAfterInterceptorHook> _afterInterceptorHooks;
        public InterceptorsHooksCollection(IEnumerable<IBeforeInterceptorHook> beforeInterceptorHooks, IEnumerable<IAfterInterceptorHook> afterInterceptorHooks)
        {
            _beforeInterceptorHooks = beforeInterceptorHooks ?? throw new ArgumentNullException(nameof(beforeInterceptorHooks));
            _afterInterceptorHooks = afterInterceptorHooks ?? throw new ArgumentNullException(nameof(afterInterceptorHooks));
        }

        public async Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
        {
            foreach (var hook in _beforeInterceptorHooks)
            {
                await hook.BeforeHandlerAsync(context, interceptor, cancellationToken);
            }
        }

        public async Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
        {
            foreach (var hook in _afterInterceptorHooks)
            {
                await hook.AfterHandlerAsync(context, interceptor, cancellationToken);
            }
        }
    }
}
