# Stator

Stator is a simple, DI-oriented, Redux-inspired or Model-View-Update (MVU) **experiment** using C# 9.0.
It works well with Blazor and [MobileBlazorBindings](https://github.com/xamarin/MobileBlazorBindings),
including sharing states between the Xamarin part and the Blazor part of a hybrid mobile app.
It should also work with any other .NET stateful client model.

> As of 2021-04-08, the project does not support .NET Standard 2.0, only .NET 5+

This project uses:

-   C# 9 record classes to ensure immutability and to simplify reducers.
-   Dependency Injection to manage states (and everything else)

# How to install?

| Name                                           | NuGet.org                                                                                                                              | feedz.io                                                                                                                                                                                                                                         |
| ---------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `dotnet add package StateR`                    | [![NuGet.org](https://img.shields.io/nuget/vpre/StateR)](https://www.nuget.org/packages/StateR/)                                       | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Fstator%2Fshield%2FStateR%2Flatest)](https://f.feedz.io/forevolve/stator/packages/StateR/latest/download)                                       |
| `dotnet add package StateR.Blazor`             | [![NuGet.org](https://img.shields.io/nuget/vpre/StateR.Blazor)](https://www.nuget.org/packages/StateR.Blazor/)                         | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Fstator%2Fshield%2FStateR.Blazor%2Flatest)](https://f.feedz.io/forevolve/stator/packages/StateR.Blazor/latest/download)                         |
| `dotnet add package StateR.Blazor.Experiments` | [![NuGet.org](https://img.shields.io/nuget/vpre/StateR.Blazor.Experiments)](https://www.nuget.org/packages/StateR.Blazor.Experiments/) | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Fstator%2Fshield%2FStateR.Blazor.Experiments%2Flatest)](https://f.feedz.io/forevolve/stator/packages/StateR.Blazor.Experiments/latest/download) |
| `dotnet add package StateR.Experiments`        | [![NuGet.org](https://img.shields.io/nuget/vpre/StateR.Experiments)](https://www.nuget.org/packages/StateR.Experiments/)               | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Fstator%2Fshield%2FStateR.Experiments%2Flatest)](https://f.feedz.io/forevolve/stator/packages/StateR.Experiments/latest/download)               |

# Counter sample

The following snippet represent a feature, surrounding a counter, where you can `Increment`, `Decrement`,
and `Set` the value of the `Count` property of that `Counter.State`. The snippet also include the
`InitialState` of that counter.

```csharp
services
    .AddStateR(appAssembly)
    .AddAsyncOperations() // Add support for Redux thunk-like helpers
    .AddReduxDevTools() // Add support for Redux DevTools
    .Apply()
;
```

```csharp
using StateR;

namespace BlazorMobileHybridExperiments.Features
{
    public class Counter
    {
        public record State(int Count) : StateBase;

        public class InitialState : IInitialState<State>
        {
            public State Value => new State(0);
        }

        public record Increment : IAction;
        public record Decrement : IAction;

        public class Reducers : IReducer<State, Increment>, IReducer<State, Decrement>
        {
            public State Reduce(State state, Increment action) => state with { Count = state.Count + 1 };
            public State Reduce(State state, Decrement action) => state with { Count = state.Count - 1 };
        }
    }
}
```

Then from a Blazor component that inherits from `StatorComponent`, we can dispatch those actions.

```csharp
@page "/counter"
@inherits StateR.Blazor.StatorComponent
@inject IState<Features.Counter.State> CounterState

<h1>Counter</h1>
<p>Current count: @CounterState.Current.Count</p>
<button class="btn btn-primary" @onclick="@(async () => await DispatchAsync(new Features.Counter.Increment()))">+</button>
<button class="btn btn-primary" @onclick="@(async () => await DispatchAsync(new Features.Counter.Decrement()))">-</button>
```

> It is not needed to inherit from `StatorComponent`, a component (or any class) can manually subscribe to any `IState<T>`.

# The origin

I played around with a few other libraries and I was not 100% satisfied with how they did stuff.
So while playing around with MobileBlazorBindings and the new hybrid apps, I found that C# 9 records
were a great fit for this. I started experimenting with transforming immutable types (records) and
ended up creating this project.

## Origin of the name

The name `StateR`, pronounced `Stator`, is inspired by `MediatR` that was initially used under the hood to mediate the commands.

## Definition of a Stator

> The stator is the stationary part of a rotary system [...].
> Energy flows through a stator to or from the rotating component of the system.

Source: [wikipedia](https://en.wikipedia.org/wiki/Stator)

# Why Redux?

After hearing about it for years, I read about it and found the concept brilliant, experimented with it, and adopted the idea.

## What is different in this library?

This library is based on the concepts introduced by Redux, but .NET is not JavaScript and
.NET Core is built around Dependency Injection (DI), so I decided to take advantage of that.

There is no type and no real DI in JavaScript, so it make sense that the folks there did not take
that into account when they built Redux.

# Redux DevTools

I based the Redux DevTools implementation on [Fluxor](https://github.com/mrpmorris/Fluxor/blob/master/Source/Fluxor.Blazor.Web.ReduxDevTools/ReduxDevToolsInterop.cs),
which is a similar project, that I used once.
That helped me lower the implementation time to connect Stator with Redux DevTools; thanks.

# To be continued

...

# Found a bug or have a feature request?

Please open an issue and be as clear as possible; see _How to contribute?_ for more information.

# How to contribute?

If you would like to contribute to the project, first, thank you for your interest, and please read [Contributing to ForEvolve open source projects](https://github.com/ForEvolve/ForEvolve.DependencyInjection/tree/master/CONTRIBUTING.md) for more information.

## Contributor Covenant Code of Conduct

Also, please read the [Contributor Covenant Code of Conduct](https://github.com/ForEvolve/Toc/blob/master/CODE_OF_CONDUCT.md) that applies to all ForEvolve repositories.

# Test (my clipboard/notes)

## Coverlet code coverage

-   `dotnet test /p:CollectCoverage=true` => coverage.json
-   `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura` => coverage.cobertura.xml
-   `dotnet test --collect:"XPlat Code Coverage"` => TestResults/{GUID}/coverage.cobertura.xml

-   `dotnet tool install -g dotnet-reportgenerator-globaltool`
-   `reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html`

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
reportgenerator "-reports:test\**\coverage.cobertura.xml" -targetdir:coveragereport -reporttypes:Html
```
