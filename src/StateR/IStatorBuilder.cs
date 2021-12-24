using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StateR
{
    public interface IStatorBuilder
    {
        IServiceCollection Services { get; }
        List<Type> Actions { get; }
        List<Type> States { get; }
        //List<Type> Interceptors { get; }
        List<Type> ActionHandlers { get; }
        //List<Type> AfterEffects { get; }
        List<Type> Updaters { get; }
        List<Type> All { get; }

        IStatorBuilder AddTypes(IEnumerable<Type> types);
        IStatorBuilder AddStates(IEnumerable<Type> states);
        IStatorBuilder AddActions(IEnumerable<Type> states);
        IStatorBuilder AddUpdaters(IEnumerable<Type> states);
        IStatorBuilder AddActionHandlers(IEnumerable<Type> types);
    }
}