using Microsoft.Extensions.Options;
using StateR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LogStatesStartupExtensions
    {
        public static IStatorBuilder AddStateLogger(this IStatorBuilder builder)
        {
            var iStateType = typeof(IState<>);
            var stateLoggerDecorator = typeof(StateLoggerDecorator<>);
            foreach (var type in builder.States)
            {
                // Equivalent to: 
                // - Decorate<IState<TState>, StateLoggerDecorator<TState>>();
                var serviceType = iStateType.MakeGenericType(type);
                var implementationType = stateLoggerDecorator.MakeGenericType(type);
                builder.Services.Decorate(serviceType, implementationType);
            }
            return builder;
        }


        private class StateLoggerDecorator<TState> : IState<TState>
            where TState : StateBase
        {
            private readonly IState<TState> _state;

            public StateLoggerDecorator(IState<TState> state)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
            }

            public TState Current => _state.Current;

            public void Set(TState state)
            {
                Console.WriteLine($"{nameof(Set)} {Sanitize(state.ToString())}");
                _state.Set(state);
            }

            public void Subscribe(Action stateHasChangedDelegate)
            {
                Console.WriteLine($"{nameof(Subscribe)} to {stateHasChangedDelegate}");
                _state.Subscribe(stateHasChangedDelegate);
            }

            public void Transform(Func<TState, TState> stateTransform)
            {
                Console.WriteLine($"{nameof(Transform)}ing {Sanitize(Current.ToString())}");
                _state.Transform(stateTransform);
                Console.WriteLine($"{nameof(Transform)}ed {Sanitize(Current.ToString())}");
            }

            public void Unsubscribe(Action stateHasChangedDelegate)
            {
                Console.WriteLine($"{nameof(Unsubscribe)} to {stateHasChangedDelegate}");
                _state.Unsubscribe(stateHasChangedDelegate);
            }

            public void Notify()
            {
                Console.WriteLine($"{nameof(Notify)}");
                _state.Notify();
            }

            private string Sanitize(string input)
            {
                return input.Replace(Environment.NewLine, "\\n");
            }
        }
    }
}
