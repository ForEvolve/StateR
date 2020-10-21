using Microsoft.Extensions.DependencyInjection;
using StateR.AfterEffects.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.AfterEffects
{
    public class AfterEffectsManager : IAfterEffectsManager
    {
        private readonly IAfterEffectHooksCollection _hooks;
        private readonly IServiceProvider _serviceProvider;

        public AfterEffectsManager(IAfterEffectHooksCollection hooks, IServiceProvider serviceProvider)
        {
            _hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var afterEffects = _serviceProvider.GetServices<IAfterEffects<TAction>>().ToList();
            foreach (var afterEffect in afterEffects)
            {
                if (dispatchContext.StopAfterEffect)
                {
                    break;
                }
                await _hooks.BeforeHandlerAsync(dispatchContext, afterEffect, cancellationToken);
                await afterEffect.HandleAfterEffectAsync(dispatchContext, cancellationToken);
                await _hooks.AfterHandlerAsync(dispatchContext, afterEffect, cancellationToken);
            }
        }
    }
}
