using System.Threading;
using System.Threading.Tasks;

namespace StateR.AfterEffects.Hooks
{
    public interface IAfterEffectHooksCollection
    {
        Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> afterEffect, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> afterEffect, CancellationToken cancellationToken) where TAction : IAction;
    }
}

