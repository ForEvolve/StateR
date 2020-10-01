using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IAfterEffects<TAction>
        where TAction : IAction
    {
        Task HandleAfterEffectAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken);
    }
}