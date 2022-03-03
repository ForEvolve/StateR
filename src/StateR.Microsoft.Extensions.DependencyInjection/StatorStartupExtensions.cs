using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StateR.Internal;
using StateR.Pipeline;
using StateR.Updaters;
using System.Reflection;

namespace StateR;

public static class StatorStartupExtensions
{
    public static IStatorBuilder AddStateR(this IServiceCollection services)
    {
        services.TryAddSingleton<IStore, Store>();
        services.TryAddSingleton<IDispatcher, Dispatcher>();
        services.TryAddSingleton<IDispatchContextFactory, DispatchContextFactory>();
        services.TryAddSingleton<IPipelineFactory, PipelineFactory>();

        return new StatorBuilder(services);
    }

    public static IStatorBuilder ScanAndAddStates(this IStatorBuilder builder, params Assembly[] assembliesToScan)
    {
        var allTypes = assembliesToScan
            .SelectMany(a => a.GetTypes());
        var initialStates = TypeScanner.FindInitialStates(allTypes);

        foreach (var initialState in initialStates)
        {
            var state = initialState.GenericTypeArguments[0];
            builder.AddState(state, initialState);
        }

        return builder;
    }

    public static IServiceCollection Apply(this IStatorBuilder builder, Action<IStatorBuilder>? postConfiguration = null)
    {
        // Register States
        foreach (var state in builder.States)
        {
            Console.WriteLine($"state: {state.FullName}");

            // Equivalent to: AddSingleton<IState<TState>, State<TState>>();
            var stateServiceType = typeof(IState<>).MakeGenericType(state);
            var stateImplementationType = typeof(State<>).MakeGenericType(state);
            builder.Services.AddSingleton(stateServiceType, stateImplementationType);
        }

        // Register Initial States
        builder.Services.Scan(s => s
            .AddTypes(builder.InitialStates)

            // Equivalent to: AddSingleton<IInitialState<TState>, Implementation>();
            .AddClasses(classes => classes.AssignableTo(typeof(IInitialState<>)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        );

        // Register Updaters and their respective IActionFilter
        var iUpdaterType = typeof(IUpdater<,>);
        var updaterHandler = typeof(UpdaterMiddleware<,>);
        var handlerType = typeof(IActionFilter<,>);
        foreach (var updater in builder.Updaters)
        {
            Console.WriteLine($"updater: {updater.FullName}");
            var interfaces = updater.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iUpdaterType);
            foreach (var @interface in interfaces)
            {
                // Equivalent to: AddSingleton<IActionFilter<TAction>, UpdaterMiddleware<TState, TAction>>
                var actionType = @interface.GenericTypeArguments[0];
                var stateType = @interface.GenericTypeArguments[1];
                var iMiddlewareServiceType = handlerType.MakeGenericType(actionType, stateType);
                var updaterMiddlewareImplementationType = updaterHandler.MakeGenericType(stateType, actionType);
                builder.Services.AddSingleton(iMiddlewareServiceType, updaterMiddlewareImplementationType);

                // Equivalent to: AddSingleton<IUpdater<TState, TAction>, Updater>();
                builder.Services.AddSingleton(@interface, updater);

                Console.WriteLine($"- AddSingleton<{iMiddlewareServiceType.GetStatorName()}, {updaterMiddlewareImplementationType.GetStatorName()}>()");
                Console.WriteLine($"- AddSingleton<{@interface.GetStatorName()}, {updater.GetStatorName()}>()");
            }
        }

        var iActionFilterType = typeof(IActionFilter<,>);
        foreach (var filter in builder.ActionFilters)
        {
            var interfaces = filter.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iActionFilterType);
            foreach (var @interface in interfaces)
            {
                var actionType = @interface.GenericTypeArguments[0];
                var stateType = @interface.GenericTypeArguments[1];
                var filterType = iActionFilterType.MakeGenericType(actionType, stateType);

                builder.Services.AddSingleton(@interface, filter);
            }
        }

        return builder.Services;
    }
}
