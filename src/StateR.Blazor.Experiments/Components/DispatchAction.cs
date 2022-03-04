using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace StateR.Blazor.Components;

public class DispatchAction<TAction> : StatorComponentBase
    where TAction : IAction
{
    [Parameter]
    public TAction? Action { get; set; }

    protected override async Task OnInitializedAsync()
    {
        GuardAgainstNullAction();
        await DispatchAsync(Action);
        await base.OnInitializedAsync();
    }

    [MemberNotNull(nameof(Action))]
    private void GuardAgainstNullAction()
    {
        ArgumentNullException.ThrowIfNull(Action, nameof(Action));
    }
}
