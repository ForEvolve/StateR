using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;

namespace StateR.Blazor.WebStorage;

public static class WebStorageStartupExtensions
{
    public static IServiceCollection AddWebStorage(this IServiceCollection services)
    {
        services.TryAddSingleton(sp => (sp.GetRequiredService<IJSRuntime>() as IJSInProcessRuntime)!);
        services.TryAddSingleton<LocalStorage>();
        services.TryAddSingleton<SessionStorage>();
        services.TryAddSingleton<IWebStorage, WebStorage>();
        return services;
    }
}
