using System.Reflection;

namespace StateR.Blazor;

public abstract class StatorComponent : StatorComponentBase
{
    private readonly List<Action> _unsubscribeDelegates = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var subscribableStateType = typeof(ISubscribable);
        var properties = GetType()
            .GetTypeInfo()
            .DeclaredProperties
            .Concat(GetType().GetProperties());

        foreach (var propertyInfo in properties)
        {
            if (propertyInfo.GetValue(this) is ISubscribable subscribableState)
            {
                subscribableState.Subscribe(StateHasChanged);
                _unsubscribeDelegates.Add(() => subscribableState.Unsubscribe(StateHasChanged));
            }
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
