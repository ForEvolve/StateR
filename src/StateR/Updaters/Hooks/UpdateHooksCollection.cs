using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Updaters.Hooks;

public class UpdateHooksCollection : IUpdateHooksCollection
{
    private readonly IEnumerable<IBeforeUpdateHook> _beforeUpdateHooks;
    private readonly IEnumerable<IAfterUpdateHook> _afterUpdateHooks;
    public UpdateHooksCollection(IEnumerable<IBeforeUpdateHook> beforeUpdateHooks, IEnumerable<IAfterUpdateHook> afterUpdateHooks)
    {
        _beforeUpdateHooks = beforeUpdateHooks ?? throw new ArgumentNullException(nameof(beforeUpdateHooks));
        _afterUpdateHooks = afterUpdateHooks ?? throw new ArgumentNullException(nameof(afterUpdateHooks));
    }

    public async Task BeforeUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
        where TAction : IAction
        where TState : StateBase
    {
        foreach (var hook in _beforeUpdateHooks)
        {
            await hook.BeforeUpdateAsync(context, state, updater, cancellationToken);
        }
    }

    public async Task AfterUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
        where TAction : IAction
        where TState : StateBase
    {
        foreach (var hook in _afterUpdateHooks)
        {
            await hook.AfterUpdateAsync(context, state, updater, cancellationToken);
        }
    }
}
