
using Microsoft.Extensions.DependencyInjection;
using StateR.Internal;

namespace StateR.Pipeline;

public interface IPipelineFactory
{
    ActionDelegate<TAction, TState> Create<TAction, TState>(IDispatchContext<TAction, TState> context)
        where TAction : IAction<TState>
        where TState : StateBase;
}

public class PipelineFactory : IPipelineFactory
{
    private readonly IServiceProvider _serviceProvider;
    public PipelineFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public ActionDelegate<TAction, TState> Create<TAction, TState>(IDispatchContext<TAction, TState> context)
        where TAction : IAction<TState>
        where TState : StateBase
    {
        var filters = _serviceProvider.GetServices<IActionFilter<TAction, TState>>();
        var enumerator = filters.GetEnumerator();
        enumerator.MoveNext();
        return MakeDelegate(enumerator.Current);

        ActionDelegate<TAction, TState> MakeDelegate(IActionFilter<TAction, TState> filter)
        {
            var hasNext = enumerator.MoveNext();
            Console.WriteLine($"ActionDelegate: {filter.GetType().GetStatorName()}");
            return new((a, s) => filter.InvokeAsync(a, hasNext ? MakeDelegate(enumerator.Current) : null, s));
        }
    }
}