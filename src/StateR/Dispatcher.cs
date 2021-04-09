using StateR.ActionHandlers;
using StateR.AfterEffects;
using StateR.Interceptors;
using StateR.Reducers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public class Dispatcher : IDispatcher
    {
        private readonly IInterceptorsManager _interceptorsManager;
        private readonly IActionHandlersManager _actionHandlersManager;
        private readonly IAfterEffectsManager _afterEffectsManager;
        private readonly IDispatchContextFactory _dispatchContextFactory;

        public Dispatcher(IDispatchContextFactory dispatchContextFactory, IInterceptorsManager interceptorsManager, IActionHandlersManager actionHandlersManager, IAfterEffectsManager afterEffectsManager)
        {
            _dispatchContextFactory = dispatchContextFactory ?? throw new ArgumentNullException(nameof(dispatchContextFactory));
            _interceptorsManager = interceptorsManager ?? throw new ArgumentNullException(nameof(interceptorsManager));
            _actionHandlersManager = actionHandlersManager ?? throw new ArgumentNullException(nameof(actionHandlersManager));
            _afterEffectsManager = afterEffectsManager ?? throw new ArgumentNullException(nameof(afterEffectsManager));
        }

        public async Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken) where TAction : IAction
        {
            var dispatchContext = _dispatchContextFactory.Create(action, this);
            await _interceptorsManager.DispatchAsync(dispatchContext, cancellationToken);
            await _actionHandlersManager.DispatchAsync(dispatchContext, cancellationToken);
            await _afterEffectsManager.DispatchAsync(dispatchContext, cancellationToken);
        }
    }
}
