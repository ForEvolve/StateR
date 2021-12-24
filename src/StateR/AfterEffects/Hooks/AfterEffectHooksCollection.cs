using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.AfterEffects.Hooks;

public class AfterEffectHooksCollection : IAfterEffectHooksCollection
{
    private readonly IEnumerable<IBeforeAfterEffectHook> _beforeAfterEffectHooks;
    private readonly IEnumerable<IAfterAfterEffectHook> _afterAfterEffectHooks;
    public AfterEffectHooksCollection(IEnumerable<IBeforeAfterEffectHook> beforeAfterEffectHooks, IEnumerable<IAfterAfterEffectHook> afterAfterEffectHooks)
    {
        _beforeAfterEffectHooks = beforeAfterEffectHooks ?? throw new ArgumentNullException(nameof(beforeAfterEffectHooks));
        _afterAfterEffectHooks = afterAfterEffectHooks ?? throw new ArgumentNullException(nameof(afterAfterEffectHooks));
    }

    public async Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> afterEffect, CancellationToken cancellationToken) where TAction : IAction
    {
        foreach (var hook in _beforeAfterEffectHooks)
        {
            await hook.BeforeHandlerAsync(context, afterEffect, cancellationToken);
        }
    }

    public async Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> afterEffect, CancellationToken cancellationToken) where TAction : IAction
    {
        foreach (var hook in _afterAfterEffectHooks)
        {
            await hook.AfterHandlerAsync(context, afterEffect, cancellationToken);
        }
    }
}

