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

    //public static IStatorBuilder AddStateR(this IServiceCollection services, params Assembly[] assembliesToScanForStates)
    //{
    //    var builder = services.AddStateR();
    //    var allTypes = assembliesToScanForStates
    //        .SelectMany(a => a.GetTypes());
    //    //builder.AddTypes(allTypes);

    //    var states = TypeScanner.FindStates(allTypes);
    //    builder.AddStates(states);

    //    return builder;
    //}

    //public static IStatorBuilder AddMiddleware(this IStatorBuilder builder)
    //{

    //}

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

        /*

    public ActionDelegate<TAction, TState> Create<TAction, TState>(IDispatchContext<TAction, TState> context, CancellationToken cancellationToken)
    {

    }
         */

        return builder.Services;

        // Extract types
        builder.ScanTypes();

        // Scan
        builder.Services.Scan(s => s
            .AddTypes(builder.All)

            // Equivalent to: AddSingleton<IInitialState<TState>, Implementation>();
            .AddClasses(classes => classes.AssignableTo(typeof(IInitialState<>)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime()

            //// Equivalent to: AddSingleton<IBeforeInterceptorHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IBeforeInterceptorHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()
            //// Equivalent to: AddSingleton<IAfterInterceptorHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IAfterInterceptorHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()

            //// Equivalent to: AddSingleton<IBeforeAfterEffectHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IBeforeAfterEffectHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()
            //// Equivalent to: AddSingleton<IAfterAfterEffectHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IAfterAfterEffectHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()

            //// Equivalent to: AddSingleton<IBeforeActionHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IBeforeActionHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()
            //// Equivalent to: AddSingleton<IAfterActionHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IAfterActionHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()

            //// Equivalent to: AddSingleton<IBeforeUpdaterHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IBeforeUpdateHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()
            //// Equivalent to: AddSingleton<IAfterUpdaterHook, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IAfterUpdateHook)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()

            //// Equivalent to: AddSingleton<IInterceptor<TState>, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IInterceptor<>)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()

            //// Equivalent to: AddSingleton<IActionAfterEffects<TState>, Implementation>();
            //.AddClasses(classes => classes.AssignableTo(typeof(IAfterEffects<>)))
            //.AsImplementedInterfaces()
            //.WithSingletonLifetime()
        );

        // Register States
        foreach (var state in builder.States)
        {
            Console.WriteLine($"state: {state.FullName}");

            // Equivalent to: AddSingleton<IState<TState>, State<TState>>();
            var stateServiceType = typeof(IState<>).MakeGenericType(state);
            var stateImplementationType = typeof(State<>).MakeGenericType(state);
            builder.Services.AddSingleton(stateServiceType, stateImplementationType);
        }

        //// Register Updaters and their respective IMiddleware
        //var iUpdaterType = typeof(IUpdater<,>);
        //var updaterHandler = typeof(UpdaterMiddleware<,>);
        //var handlerType = typeof(IActionFilter<,>);
        //foreach (var updater in builder.Updaters)
        //{
        //    Console.WriteLine($"updater: {updater.FullName}");
        //    var interfaces = updater.GetInterfaces()
        //        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iUpdaterType);
        //    foreach (var @interface in interfaces)
        //    {
        //        // Equivalent to: AddSingleton<IActionFilter<TAction>, UpdaterMiddleware<TState, TAction>>
        //        var actionType = @interface.GenericTypeArguments[0];
        //        var stateType = @interface.GenericTypeArguments[1];
        //        var iMiddlewareServiceType = handlerType.MakeGenericType(actionType, stateType);
        //        var updaterMiddlewareImplementationType = updaterHandler.MakeGenericType(stateType, actionType);
        //        builder.Services.AddSingleton(iMiddlewareServiceType, updaterMiddlewareImplementationType);

        //        // Equivalent to: AddSingleton<IUpdater<TState, TAction>, Updater>();
        //        builder.Services.AddSingleton(@interface, updater);

        //        Console.WriteLine($"- AddSingleton<{iMiddlewareServiceType.GetStatorName()}, {updaterMiddlewareImplementationType.GetStatorName()}>()");
        //        Console.WriteLine($"- AddSingleton<{@interface.GetStatorName()}, {updater.GetStatorName()}>()");
        //    }
        //}

        // Register Middleware
        foreach (var middleware in builder.Middlewares.AsEnumerable().Reverse())
        {
            builder.Services.Decorate(typeof(IActionFilter<,>), middleware);
        }

        // Run post-configuration
        postConfiguration?.Invoke(builder);

        return builder.Services;
    }
}
