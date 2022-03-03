using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace StateR;

public interface IStatorBuilder
{
    IServiceCollection Services { get; }
    ReadOnlyCollection<Type> States { get; }
    ReadOnlyCollection<Type> InitialStates { get; }
    ReadOnlyCollection<Type> Actions { get; }
    ReadOnlyCollection<Type> Updaters { get; }
    ReadOnlyCollection<Type> ActionFilters { get; }

    IStatorBuilder AddState<TState, TInitialState>()
        where TState : StateBase
        where TInitialState : IInitialState<TState>;
    IStatorBuilder AddState(Type state, Type initialState);

    IStatorBuilder AddAction(Type actionType);
    IStatorBuilder AddUpdaters(Type updaterType);
    IStatorBuilder AddActionFilter(Type actionFilterType);
}
