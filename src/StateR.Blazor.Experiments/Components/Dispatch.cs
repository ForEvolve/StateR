namespace StateR.Blazor.Components;
public class Dispatch<TAction> : StatorComponentBase
    where TAction : IAction, new()
{
    protected override async Task OnInitializedAsync()
    {
        await DispatchAsync(new TAction());
        await base.OnInitializedAsync();
    }
}
