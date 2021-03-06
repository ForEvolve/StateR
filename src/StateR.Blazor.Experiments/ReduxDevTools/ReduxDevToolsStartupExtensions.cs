﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using StateR.Reducers;
using StateR.Reducers.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateR.Blazor.ReduxDevTools
{
    public static class ReduxDevToolsStartupExtensions
    {
        public static IStatorBuilder AddReduxDevTools(this IStatorBuilder builder)
        {
            builder.Services.AddSingleton(sp =>
            {
                var states = new List<Type>();
                var iStateType = typeof(IState<>);
                foreach (var state in builder.States.OrderBy(x => x.Name))
                {
                    var x = iStateType.MakeGenericType(state);
                    states.Add(x);
                }
                return new DevToolsStateCollection(states);
            });
            builder.Services.AddSingleton<ReduxDevToolsInteropInitializer>();
            builder.Services.AddSingleton<ReduxDevToolsInterop>();
            builder.Services.AddSingleton<IBeforeReducerHook>(sp => sp.GetService<ReduxDevToolsInterop>());
            builder.Services.AddSingleton<IAfterReducerHook>(sp => sp.GetService<ReduxDevToolsInterop>());
            return builder;
        }
    }
}
