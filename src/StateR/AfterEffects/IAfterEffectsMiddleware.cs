using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.AfterEffects
{
    public interface IAfterEffectsMiddleware
    {
        Task BeforeAfterEffectsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionAfterEffects<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
        Task BeforeAfterEffectAsync<TAction>(IDispatchContext<TAction> context, IActionAfterEffects<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterAfterEffectAsync<TAction>(IDispatchContext<TAction> context, IActionAfterEffects<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterAfterEffectsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionAfterEffects<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
    }
}
