using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IActionHandler<TAction>
        where TAction : IAction
    {
        Task HandleAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken);
    }
}