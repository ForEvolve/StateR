using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace StateR.Blazor.WebStorage;

public static class WebStorageStartupExtensions
{
    public static IServiceCollection AddWebStorage(this IServiceCollection services)
        => services.AddWebStorage(default);

    public static IServiceCollection AddWebStorage(this IServiceCollection services, Action<WebStorageOptions>? configure)
    {
        services.TryAddSingleton(sp => (sp.GetRequiredService<IJSRuntime>() as IJSInProcessRuntime)!);
        services.TryAddSingleton<LocalStorage>();
        services.TryAddSingleton<SessionStorage>();
        services.TryAddSingleton<IWebStorage, WebStorage>();
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<WebStorageOptions>();
            var webStorage = sp.GetRequiredService<IWebStorage>();
            return options.DefaultStorageType == StorageType.Local
                ? webStorage.LocalStorage
                : webStorage.SessionStorage;
        });
        services
            .AddWebStorageOptions(configure)
            .AddDefaultSerializer()
        ;
        return services;
    }

    private static IServiceCollection AddDefaultSerializer(this IServiceCollection services)
    {
        services.AddOptions<JsonWebStorageSerializerOptions>();
        services.TryAddSingleton<
            IConfigureOptions<JsonWebStorageSerializerOptions>,
            ConfigureJsonWebStorageSerializerOptions
        >();
        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<JsonWebStorageSerializerOptions>>().Value);
        services.TryAddSingleton<IWebStorageSerializer, JsonWebStorageSerializer>();
        return services;
    }

    private static IServiceCollection AddWebStorageOptions(this IServiceCollection services, Action<WebStorageOptions>? configure)
    {
        services.Configure<WebStorageOptions>(configureOptions
            => configure?.Invoke(configureOptions));
        services.TryAddSingleton<
            IPostConfigureOptions<WebStorageOptions>,
            PostConfigureWebStorageOptions
        >();
        services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<WebStorageOptions>>().Value);
        return services;
    }
}
