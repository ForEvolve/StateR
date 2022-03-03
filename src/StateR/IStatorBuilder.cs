using Microsoft.Extensions.DependencyInjection;
using StateR.Updaters;
using System.Collections.ObjectModel;

namespace StateR;

public interface IStatorBuilder : IOldStatorBuilder
{
    IServiceCollection Services { get; }
    ReadOnlyCollection<Type> States { get; }
    ReadOnlyCollection<Type> InitialStates { get; }
    ReadOnlyCollection<Type> Actions { get; }
    ReadOnlyCollection<Type> Updaters { get; }

    IStatorBuilder AddState<TState, TInitialState>()
        where TState : StateBase
        where TInitialState : IInitialState<TState>;
    IStatorBuilder AddState(Type state, Type initialState);

    IStatorBuilder AddAction(Type actionType);
    IStatorBuilder AddUpdaters(Type updaterType);
}

public interface IOldStatorBuilder
{
    //List<Type> Interceptors { get; }
    List<Type> ActionHandlers { get; }
    //List<Type> AfterEffects { get; }
    List<Type> All { get; }

    IStatorBuilder AddTypes(IEnumerable<Type> types);
    IStatorBuilder AddStates(IEnumerable<Type> states);
    IStatorBuilder AddActions(IEnumerable<Type> states);
    IStatorBuilder AddUpdaters(IEnumerable<Type> states);
    IStatorBuilder AddActionHandlers(IEnumerable<Type> types);

    IStatorBuilder AddMiddlewares(IEnumerable<Type> types);
    List<Type> Middlewares { get; }

    //IStatorBuilder AddAction<TAction, TState>()
    //    where TState : StateBase
    //    where TAction : IAction<TState>;
}
