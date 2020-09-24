# StateR

This is an **experiment** of a Redux-inspired .NET/C# 9.0 (preview) library.
It works well with Blazor and [MobileBlazorBindings](https://github.com/xamarin/MobileBlazorBindings),
including sharing states between the Xamarin part and the Blazor part of a hybrid app.
It should also work with any other .NET stateful client model.

This project uses:

-   C# 9 (preview) record classes to ensure immutability and to simplify reducers.
-   MediatR, under the hood, to mediate actions.
-   Dependency Injection to manage states

# How to install?

| Name                               | NuGet.org                                                                                                      | feedz.io                                                                                                                                                                                                                 |
| ---------------------------------- | -------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `dotnet add package StateR`        | [![NuGet.org](https://img.shields.io/nuget/vpre/StateR)](https://www.nuget.org/packages/StateR/)               | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Fstator%2Fshield%2FStateR%2Flatest)](https://f.feedz.io/forevolve/stator/packages/StateR/latest/download)               |
| `dotnet add package StateR.Blazor` | [![NuGet.org](https://img.shields.io/nuget/vpre/StateR.Blazor)](https://www.nuget.org/packages/StateR.Blazor/) | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Fstator%2Fshield%2FStateR.Blazor%2Flatest)](https://f.feedz.io/forevolve/stator/packages/StateR.Blazor/latest/download) |

# Counter sample

The following snippet represent a feature, surrounding a counter, where you can `Increment`, `Decrement`,
and `Set` the value of the `Count` property of that `Counter.State`. The snippet also include the
`InitialState` of that counter.

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

        public record Set(int Value) : IAction;
        public record Increment : IAction;
        public record Decrement : IAction;

        public class Reducers : IReducer<State, Set>, IReducer<State, Increment>, IReducer<State, Decrement>
        {
            public State Reduce(State state, Set action) => state with { Count = action.Value };
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

<hr />

<input @bind="SetTo" />
<button class="btn btn-secondary" @onclick="@(async () => await DispatchAsync(new Features.Counter.Set(SetTo)))">Set</button>

@code
{
    private int SetTo;
}
```

# The origin

I played around with a few other libraries that aim at the same goal (another project)
and I was not 100% satisfied with how they did stuff, so while playing around with
MobileBlazorBindings and the new hybrid apps, I tough that C# 9 records were a great fit
for this and I started experimenting and ended up with this project.

## The name

Why the name? StateR, pronounced **stator**, is written this way as a wink to MediatR.

### Definition of a stator

> The stator is the stationary part of a rotary system [...].
> Energy flows through a stator to or from the rotating component of the system.

Source: [wikipedia](https://en.wikipedia.org/wiki/Stator)

## Why Redux?

After hearing about it for a while, I read about it and found the concept brilliant.

## What is different from the other libraries?

This library is based on the concepts introduced by Redux, but .NET is not JavaScript and
.NET Core is built around Dependency Injection (DI), so I decided to take advantage of that.

There is no type and no DI in JavaScript, so it make sense that the folks there did not take
that into account when they built Redux.

# To be continued

...

# Found a bug or have a feature request?

Please open an issue and be as clear as possible; see _How to contribute?_ for more information.

# How to contribute?

If you would like to contribute to the project, first, thank you for your interest, and please read [Contributing to ForEvolve open source projects](https://github.com/ForEvolve/ForEvolve.DependencyInjection/tree/master/CONTRIBUTING.md) for more information.

## Contributor Covenant Code of Conduct

Also, please read the [Contributor Covenant Code of Conduct](https://github.com/ForEvolve/Toc/blob/master/CODE_OF_CONDUCT.md) that applies to all ForEvolve repositories.
