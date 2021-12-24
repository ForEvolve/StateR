using System.Threading;
using System.Threading.Tasks;

namespace StateR.AfterEffects.Hooks;

public interface IAfterAfterEffectHook
{
    Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> afterEffect, CancellationToken cancellationToken) where TAction : IAction;
}

