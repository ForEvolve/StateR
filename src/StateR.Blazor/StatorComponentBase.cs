using MediatR;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Blazor
{
    public abstract class StatorComponentBase : ComponentBase, IDisposable
    {
        private bool disposedValue;

        [Inject]
        public IStore Store { get; set; }

        protected virtual async Task DispatchAsync(IAction action, CancellationToken cancellationToken = default)
            => await Store.DispatchAsync(action, cancellationToken);

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
}
