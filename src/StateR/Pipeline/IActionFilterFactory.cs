
using Microsoft.Extensions.DependencyInjection;
using System;
namespace StateR.Pipeline;

public interface IActionFilterFactory
{
    IActionFilter<TAction, TState> Create<TAction, TState>(IDispatchContext<TAction, TState> context)
        where TAction : IAction<TState>
        where TState : StateBase;
}

public class ActionFilterFactory : IActionFilterFactory
{
    private readonly IServiceProvider _serviceProvider;
    public ActionFilterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IActionFilter<TAction, TState> Create<TAction, TState>(IDispatchContext<TAction, TState> context)
        where TAction : IAction<TState>
        where TState : StateBase
        => _serviceProvider.GetRequiredService<IActionFilter<TAction, TState>>();
    
}