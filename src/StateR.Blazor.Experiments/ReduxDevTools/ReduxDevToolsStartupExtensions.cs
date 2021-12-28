using Microsoft.Extensions.DependencyInjection;
using StateR.Updaters.Hooks;

namespace StateR.Blazor.ReduxDevTools;

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
        builder.Services.AddSingleton<IBeforeUpdateHook>(sp => sp.GetRequiredService<ReduxDevToolsInterop>());
        builder.Services.AddSingleton<IAfterUpdateHook>(sp => sp.GetRequiredService<ReduxDevToolsInterop>());
        return builder;
    }
}
