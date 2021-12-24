using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.ActionHandlers.Hooks;

public class ActionHandlerHooksCollection : IActionHandlerHooksCollection
{
    private readonly IEnumerable<IBeforeActionHook> _beforeActionHooks;
    private readonly IEnumerable<IAfterActionHook> _afterActionHooks;
    public ActionHandlerHooksCollection(IEnumerable<IBeforeActionHook> beforeActionHooks, IEnumerable<IAfterActionHook> afterActionHooks)
    {
        _beforeActionHooks = beforeActionHooks ?? throw new ArgumentNullException(nameof(beforeActionHooks));
        _afterActionHooks = afterActionHooks ?? throw new ArgumentNullException(nameof(afterActionHooks));
    }

    public async Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> actionHandler, CancellationToken cancellationToken) where TAction : IAction
    {
        foreach (var hook in _beforeActionHooks)
        {
            await hook.BeforeHandlerAsync(context, actionHandler, cancellationToken);
        }
    }

    public async Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> actionHandler, CancellationToken cancellationToken) where TAction : IAction
    {
        foreach (var hook in _afterActionHooks)
        {
            await hook.AfterHandlerAsync(context, actionHandler, cancellationToken);
        }
    }
}
