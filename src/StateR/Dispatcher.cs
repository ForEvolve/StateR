using StateR.ActionHandlers;
using StateR.AfterEffects;
using StateR.Interceptors;
using StateR.Updaters;
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
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var dispatchContext = _dispatchContextFactory.Create(action, this, cancellationTokenSource);
            //
            // TODO: design how to handle OperationCanceledException
            //
            await _interceptorsManager.DispatchAsync(dispatchContext);
            await _actionHandlersManager.DispatchAsync(dispatchContext);
            await _afterEffectsManager.DispatchAsync(dispatchContext);
        }
    }
}
