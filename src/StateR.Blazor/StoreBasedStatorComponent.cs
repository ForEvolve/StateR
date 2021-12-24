using Microsoft.AspNetCore.Components;

namespace StateR.Blazor;

public abstract class StoreBasedStatorComponent : StatorComponentBase
{
    private bool _subscribed = false;
    private readonly List<Action> _unsubscribeDelegates = new List<Action>();

    [Inject]
    public IStore Store { get; set; }

    protected virtual TState GetState<TState>() where TState : StateBase
    {
        Subscribe<TState>();
        return Store.GetState<TState>();
    }

    protected virtual void Subscribe<TState>() where TState : StateBase
    {
        if (!_subscribed)
        {
            _subscribed = true;
            Store.Subscribe<TState>(StateHasChanged);
            _unsubscribeDelegates.Add(() => Store.Unsubscribe<TState>(StateHasChanged));
        }
    }

    protected override void FreeManagedResources()
    {
        foreach (var unsubscribe in _unsubscribeDelegates)
        {
            unsubscribe();
        }
    }
}
