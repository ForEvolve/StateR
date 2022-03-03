using StateR.Pipeline;
using StateR.Updaters;
using System.Reflection;

namespace StateR.Internal;

//public static class TypeScannerBuilderExtensions
//{
//    public static IStatorBuilder ScanTypes(this IStatorBuilder builder)
//    {
//        var states = TypeScanner.FindStates(builder.All);
//        builder.AddStates(states);

//        var actions = TypeScanner.FindActions(builder.All);
//        builder.AddActions(actions);

//        var updaters = TypeScanner.FindUpdaters(builder.All);
//        builder.AddUpdaters(updaters);

//        var actionHandlers = TypeScanner.FindMiddlewares(builder.All);
//        builder.AddUpdaters(actionHandlers);

//        return builder;
//    }
//}
public static class TypeScanner
{
    public static IEnumerable<Type> FindStates(IEnumerable<Type> types)
    {
        var states = types
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(StateBase)));
        return states;
    }

    public static IEnumerable<Type> FindInitialStates(IEnumerable<Type> types)
    {
        var initialStates = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i == typeof(IInitialState<>))
        );
        return initialStates;
    }

    public static IEnumerable<Type> FindActions(IEnumerable<Type> types)
    {
        var actions = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i == typeof(IAction<>))
        );
        return actions;
    }

    public static IEnumerable<Type> FindUpdaters(IEnumerable<Type> types)
    {
        var updaters = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUpdater<,>))
        );
        return updaters;
    }

    public static IEnumerable<Type> FindMiddlewares(IEnumerable<Type> types)
    {
        var handlers = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IActionFilter<,>))
        );
        return handlers;
    }
}