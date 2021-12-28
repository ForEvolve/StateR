using CounterApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StateR;
using StateR.Experiments.AsyncLogic;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.RegisterServices();

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
            .Apply()
        ;
        services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(sp.GetRequiredService<IWebAssemblyHostEnvironment>().BaseAddress) });
    }
}
