using Microsoft.Extensions.DependencyInjection;
using StateR.ActionHandlers.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.ActionHandlers
{
    public class ActionHandlersManager : IActionHandlersManager
    {
        private readonly IActionHandlerHooksCollection _hooksCollection;
        private readonly IServiceProvider _serviceProvider;

        public ActionHandlersManager(IActionHandlerHooksCollection hooksCollection, IServiceProvider serviceProvider)
        {
            _hooksCollection = hooksCollection ?? throw new ArgumentNullException(nameof(hooksCollection));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var reducerHandlers = _serviceProvider.GetServices<IActionHandler<TAction>>().ToList();
            foreach (var handler in reducerHandlers)
            {
                if (dispatchContext.StopReduce)
                {
                    break;
                }
                await _hooksCollection.BeforeHandlerAsync(dispatchContext, handler, cancellationToken);
                await handler.HandleAsync(dispatchContext, cancellationToken);
                await _hooksCollection.AfterHandlerAsync(dispatchContext, handler, cancellationToken);
            }
        }
    }
}
