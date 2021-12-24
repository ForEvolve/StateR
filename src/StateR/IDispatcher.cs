using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace StateR;

public interface IDispatcher
{
    Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken) where TAction : IAction;
}
