using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.AfterEffects
{
    public class AfterEffectsManager : IAfterEffectsManager
    {
        private readonly IEnumerable<IAfterEffectsMiddleware> _middlewares;
        private readonly IServiceProvider _serviceProvider;

        public AfterEffectsManager(IEnumerable<IAfterEffectsMiddleware> middlewares, IServiceProvider serviceProvider)
        {
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var afterEffects = _serviceProvider.GetServices<IActionAfterEffects<TAction>>().ToList();
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeAfterEffectsAsync(dispatchContext, afterEffects, cancellationToken);
            }
            foreach (var afterEffect in afterEffects)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeAfterEffectAsync(dispatchContext, afterEffect, cancellationToken);
                }
                if (dispatchContext.StopAfterEffect)
                {
                    break;
                }
                await afterEffect.HandleAfterEffectAsync(dispatchContext, cancellationToken);
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterAfterEffectAsync(dispatchContext, afterEffect, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterAfterEffectsAsync(dispatchContext, afterEffects, cancellationToken);
            }
        }
    }
}
