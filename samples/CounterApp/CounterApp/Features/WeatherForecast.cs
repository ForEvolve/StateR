using StateR;
using StateR.AfterEffects;
using StateR.AsyncLogic;
using StateR.Interceptors;
using StateR.Internal;
using StateR.Updaters;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace CounterApp.Features;
public class WeatherForecast
{
    public record State(ImmutableList<Forecast> Forecasts) : AsyncState;
    public record class Forecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    public class InitialState : IInitialState<State>
    {
        public State Value => new(ImmutableList.Create<Forecast>());
    }

    public record class Fetch : IAction;
    public record class Fetched(Forecast[] Forecasts) : IAction;
    public record class Reload : IAction;

    public class Updaters : IUpdater<Fetched, State>, IUpdater<Reload, State>
    {
        public State Update(Fetched action, State state)
            => state with { Forecasts = ImmutableList.Create(action.Forecasts) };

        public State Update(Reload action, State state)
            => state with { Status = AsyncOperationStatus.Idle };
    }

    public class ReloadEffect : IAfterEffects<Reload>
    {
        public async Task HandleAfterEffectAsync(IDispatchContext<Reload> context, CancellationToken cancellationToken)
        {
            await context.Dispatcher.DispatchAsync(new Fetch(), cancellationToken);
        }
    }

    public class FetchOperation : AsyncOperation<Fetch, State, Fetched>
    {
        private readonly HttpClient _http;
        public FetchOperation(IStore store, HttpClient http)
            : base(store)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        protected override async Task<Fetched> LoadAsync(Fetch action, State initalState, CancellationToken cancellationToken = default)
        {
            var forecasts = await _http.GetFromJsonAsync<Forecast[]>("sample-data/weather.json", cancellationToken);
            return new Fetched(forecasts ?? Array.Empty<Forecast>());
        }
    }

    public class Delays : IInterceptor<StatusUpdated<State>>
    {
        public async Task InterceptAsync(IDispatchContext<StatusUpdated<State>> context, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{context.Action.GetType().GetStatorName()}: {context.Action.status}");
            await Task.Delay(2000, cancellationToken);
        }
    }
}
