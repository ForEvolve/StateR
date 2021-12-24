using Microsoft.Extensions.DependencyInjection;
using StateR.ActionHandlers.Hooks;

namespace StateR.ActionHandlers;

public class ActionHandlersManager : IActionHandlersManager
{
    private readonly IActionHandlerHooksCollection _hooksCollection;
    private readonly IServiceProvider _serviceProvider;

    public ActionHandlersManager(IActionHandlerHooksCollection hooksCollection, IServiceProvider serviceProvider)
    {
        _hooksCollection = hooksCollection ?? throw new ArgumentNullException(nameof(hooksCollection));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext) where TAction : IAction
    {
        var updaterHandlers = _serviceProvider.GetServices<IActionHandler<TAction>>().ToList();
        foreach (var handler in updaterHandlers)
        {
            dispatchContext.CancellationToken.ThrowIfCancellationRequested();

            await _hooksCollection.BeforeHandlerAsync(dispatchContext, handler, dispatchContext.CancellationToken);
            await handler.HandleAsync(dispatchContext, dispatchContext.CancellationToken);
            await _hooksCollection.AfterHandlerAsync(dispatchContext, handler, dispatchContext.CancellationToken);
        }
    }
}
