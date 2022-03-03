using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace StateR.Blazor;

public abstract class StatorComponentBase : ComponentBase, IDisposable
{
    private bool _disposedValue;

    [Inject]
    public IDispatcher? Dispatcher { get; set; }

    protected virtual async Task DispatchAsync(object action, CancellationToken cancellationToken = default)
    {
        GuardAgainstNullDispatcher();
        await Dispatcher.DispatchAsync(action, cancellationToken);
    }


    protected virtual async Task DispatchAsync<TAction, TState>(TAction action, CancellationToken cancellationToken = default)
        where TAction : IAction<TState>
        where TState : StateBase
    {
        GuardAgainstNullDispatcher();
        await Dispatcher.DispatchAsync<TAction, TState>(action, cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                FreeManagedResources();
            }
            FreeUnmanagedResources();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void FreeManagedResources() { }
    protected virtual void FreeUnmanagedResources() { }

    [MemberNotNull(nameof(Dispatcher))]
    protected void GuardAgainstNullDispatcher()
    {
        ArgumentNullException.ThrowIfNull(Dispatcher, nameof(Dispatcher));
    }
}
