using Microsoft.Extensions.DependencyInjection;
using StateR.Internal;
using StateR.Pipeline;
using StateR.Updaters;
using System;
using System.Threading.Tasks;
using Xunit;

namespace StateR;
public class IntegrationTest
{
    public record class CounterState(int Count) : StateBase;
    public record class InitialCounterState : IInitialState<CounterState>
    {
        public CounterState Value => new(0);
    }
    public record class Increment(int Amount) : IAction<CounterState>;
    public record class Decrement(int Amount) : IAction<CounterState>;
    public class CounterUpdaters :
        IUpdater<Increment, CounterState>,
        IUpdater<Decrement, CounterState>
    {
        public CounterState Update(Increment action, CounterState state)
            => state with { Count = state.Count + action.Amount };
        public CounterState Update(Decrement action, CounterState state)
            => state with { Count = state.Count - action.Amount };
    }
    public class ValidateIncrementFilter : IActionFilter<Increment, CounterState>
    {
        public Task InvokeAsync(
            IDispatchContext<Increment, CounterState> context,
            ActionDelegate<Increment, CounterState> next,
            CancellationToken cancellationToken)
        {
            if (context.Action.Amount <= 0)
            {
                throw new ValidationException();
            }
            return next?.Invoke(context, cancellationToken);
        }
    }
    public class ValidationException : Exception { }

    public class IncrementTest : IntegrationTest
    {
        [Fact]
        public async Task Should_increment_the_CounterState_by_the_Increment_action_Amount()
        {
            // Arrange
            var services = Initialize();
            var state = services.GetRequiredService<IState<CounterState>>();
            var initialCount = state.Current.Count;
            Assert.Equal(0, initialCount);

            var cancellationToken = CancellationToken.None;
            var dispatcher = services.GetRequiredService<IDispatcher>();

            // Act
            await dispatcher.DispatchAsync(new Increment(2), cancellationToken);

            // Assert
            Assert.Equal(2, state.Current.Count);
        }
    }

    public class DecrementTest : IntegrationTest
    {
        [Fact]
        public async Task Should_decrement_the_CounterState_by_the_Increment_action_Amount()
        {
            // Arrange
            var services = Initialize();
            var state = services.GetRequiredService<IState<CounterState>>();
            var initialCount = state.Current.Count;
            Assert.Equal(0, initialCount);

            var cancellationToken = CancellationToken.None;
            var dispatcher = services.GetRequiredService<IDispatcher>();

            // Act
            await dispatcher.DispatchAsync(new Decrement(5), cancellationToken);

            // Assert
            Assert.Equal(-5, state.Current.Count);
        }
    }

    public class ValidateIncrementFilterTest : IntegrationTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Should_throw_a_ValidationException_when_Increment_is_smaller_or_equal_to_zero(int amount)
        {
            // Arrange
            var services = Initialize();
            var cancellationToken = CancellationToken.None;
            var dispatcher = services.GetRequiredService<IDispatcher>();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => dispatcher
                .DispatchAsync(new Increment(amount), cancellationToken));
        }
    }

    private IServiceProvider Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        return services.AddStateR()
            .AddState<CounterState, InitialCounterState>()
            .AddAction(typeof(Increment))
            .AddAction(typeof(Decrement))
            .AddUpdaters(typeof(CounterUpdaters))
            .AddActionFilter(typeof(ValidateIncrementFilter))
            .Apply()
            .BuildServiceProvider()
        ;
    }
}
