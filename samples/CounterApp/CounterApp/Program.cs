using Blazored.SessionStorage;
using CounterApp;
using CounterApp.Features;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StateR;
using StateR.AfterEffects;
using StateR.Blazor.ReduxDevTools;
using StateR.Experiments.AsyncLogic;
using StateR.Interceptors;
using StateR.Validations.FluentValidation;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


////builder.Services.AddSingleton<IInterceptor<Counter.Increment>, PersistanceMiddleware<Counter.State, Counter.Increment>>();
//builder.Services.AddSingleton<IAfterEffects<Counter.Increment>, PersistenceMiddleware<Counter.State, Counter.Increment>>();

////builder.Services.AddSingleton<IInterceptor<Counter.Decrement>, PersistanceMiddleware<Counter.State, Counter.Decrement>>();
//builder.Services.AddSingleton<IAfterEffects<Counter.Decrement>, PersistenceMiddleware<Counter.State, Counter.Decrement>>();

////builder.Services.AddSingleton<IInterceptor<Counter.SetPositive>, PersistanceMiddleware<Counter.State, Counter.SetPositive>>();
//builder.Services.AddSingleton<IAfterEffects<Counter.SetPositive>, PersistenceMiddleware<Counter.State, Counter.SetPositive>>();

builder.Services.RegisterServices();
builder.Services.AddBlazoredSessionStorage(null, ServiceLifetime.Singleton);

//builder.Services.Decorate<IInitialState<Counter.State>, InitialSessionStateDecorator<Counter.State>>();
//IInitialState<Counter.State>
//InitialSessionState<TState> : IInitialState<TState>

await builder.Build().RunAsync();

public partial class Program { }

public static class ProgramExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        var appAssembly = typeof(App).Assembly;
        services
            .AddStateR(appAssembly)
            .AddAsyncOperations()
            .AddReduxDevTools()
            .AddFluentValidation(appAssembly)
            .Apply()
            .AddPersistence(appAssembly)
            .AddStateValidation(appAssembly)
        ;
        services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IWebAssemblyHostEnvironment>().BaseAddress) });
    }
}
