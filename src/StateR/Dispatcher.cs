using Microsoft.Extensions.Logging;
using StateR.Pipeline;
using System;

namespace StateR;

public class Dispatcher : IDispatcher
{
    private readonly IDispatchContextFactory _dispatchContextFactory;
    private readonly IActionFilterFactory _actionFilterFactory;
    private readonly ILogger _logger;

    public Dispatcher(IDispatchContextFactory dispatchContextFactory, IActionFilterFactory actionFilterFactory, ILogger<Dispatcher> logger)
    {
        _dispatchContextFactory = dispatchContextFactory ?? throw new ArgumentNullException(nameof(dispatchContextFactory));
        _actionFilterFactory = actionFilterFactory ?? throw new ArgumentNullException(nameof(actionFilterFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchAsync<TAction, TState>(TAction action, CancellationToken cancellationToken)
        where TAction : IAction<TState>
        where TState : StateBase
    {
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var dispatchContext = _dispatchContextFactory.Create<TAction, TState>(action, this, cancellationTokenSource);
        var actionFilter = _actionFilterFactory.Create(dispatchContext);
        try
        {
            await actionFilter.InvokeAsync(dispatchContext, null, cancellationToken);
        }
        catch (DispatchCancelledException ex)
        {
            _logger.LogWarning(ex, ex.Message);
        }
    }

    public Task DispatchAsync(object action, CancellationToken cancellationToken)
    {
        var actionType = action
            .GetType();
        var actionInterface = actionType.GetInterfaces()
            .FirstOrDefault(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IAction<>));
        if (actionInterface == null)
        {
            // TODO: Find a better exception
            throw new InvalidOperationException($"The action must implement the {typeof(IAction<>).Name} interface.");
        }
        var stateType = actionInterface.GetGenericArguments()[0];
        var method = GetType().GetMethods().FirstOrDefault(m => m.IsGenericMethod && m.Name == nameof(DispatchAsync));
        if(method == null)
        {
            throw new MissingMethodException(nameof(Dispatcher), nameof(DispatchAsync));
        }
        var genericMethod = method.MakeGenericMethod(actionType, stateType);
        var task = genericMethod.Invoke(this, new[] { action, cancellationToken });
        return (Task)task!;
    }
}
