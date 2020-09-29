using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IDispatcher
    {
        Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default) where TAction : IAction;
    }
}
