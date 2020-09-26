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
                var stateServiceType = iStateType.MakeGenericType(type);
                var stateLoggerDecoratorImplementationType = stateLoggerDecorator.MakeGenericType(type);
                builder.Services.Decorate(stateServiceType, stateLoggerDecoratorImplementationType);
            }
            return builder;
        }


        private class StateLoggerDecorator<TState> : IState<TState>
            where TState : StateBase
        {
            #region IState<TState> Facade

            private readonly IState<TState> _state;

            public TState Current => _state.Current;

            public void Set(TState state)
            {
                Console.WriteLine($"{nameof(Set)}: {state}");
                _state.Set(state);
            }

            public void Subscribe(Action stateHasChangedDelegate)
            {
                Console.WriteLine($"{nameof(Subscribe)}: {stateHasChangedDelegate}");
                _state.Subscribe(stateHasChangedDelegate);
            }

            public void Transform(Func<TState, TState> stateTransform)
            {
                Console.WriteLine($"{nameof(Transform)}: {stateTransform}");
                _state.Transform(stateTransform);
            }

            public void Unsubscribe(Action stateHasChangedDelegate)
            {
                Console.WriteLine($"{nameof(Unsubscribe)}: {stateHasChangedDelegate}");
                _state.Unsubscribe(stateHasChangedDelegate);
            }

            public void Notify()
            {
                Console.WriteLine($"{nameof(Notify)}");
                _state.Notify();
            }

            #endregion
        }
    }
}
