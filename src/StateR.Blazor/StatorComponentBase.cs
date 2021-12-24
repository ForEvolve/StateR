using Microsoft.AspNetCore.Components;

namespace StateR.Blazor;

public abstract class StatorComponentBase : ComponentBase, IDisposable
{
    private bool disposedValue;

    [Inject]
    public IDispatcher Dispatcher { get; set; }

    protected virtual async Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default)
        where TAction : IAction
        => await Dispatcher.DispatchAsync(action, cancellationToken);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                FreeManagedResources();
            }
            FreeUnmanagedResources();
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void FreeManagedResources() { }
    protected virtual void FreeUnmanagedResources() { }
}
