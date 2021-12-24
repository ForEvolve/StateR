using Microsoft.Extensions.DependencyInjection;
using StateR.AfterEffects.Hooks;

namespace StateR.AfterEffects;

public class AfterEffectsManager : IAfterEffectsManager
{
    private readonly IAfterEffectHooksCollection _hooks;
    private readonly IServiceProvider _serviceProvider;

    public AfterEffectsManager(IAfterEffectHooksCollection hooks, IServiceProvider serviceProvider)
    {
        _hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext) where TAction : IAction
    {
        var afterEffects = _serviceProvider.GetServices<IAfterEffects<TAction>>().ToList();
        foreach (var afterEffect in afterEffects)
        {
            dispatchContext.CancellationToken.ThrowIfCancellationRequested();

            await _hooks.BeforeHandlerAsync(dispatchContext, afterEffect, dispatchContext.CancellationToken);
            await afterEffect.HandleAfterEffectAsync(dispatchContext, dispatchContext.CancellationToken);
            await _hooks.AfterHandlerAsync(dispatchContext, afterEffect, dispatchContext.CancellationToken);
        }
    }
}
