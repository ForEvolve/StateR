using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace StateR.Blazor.Components;

public class DispatchActionFactory<TAction> : StatorComponentBase
    where TAction : IAction
{
    [Parameter]
    public Func<ValueTask<TAction>>? ActionFactory { get; set; }

    protected override async Task OnInitializedAsync()
    {
        GuardAgainstNullAction();
        var action = await ActionFactory.Invoke();
        await DispatchAsync(action);
        await base.OnInitializedAsync();
    }

    [MemberNotNull(nameof(ActionFactory))]
    private void GuardAgainstNullAction()
    {
        ArgumentNullException.ThrowIfNull(ActionFactory, nameof(ActionFactory));
    }
}
