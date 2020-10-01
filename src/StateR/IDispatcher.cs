using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IDispatcher
    {
        Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken) where TAction : IAction;
    }
    public class Dispatcher : IDispatcher
    {
        private readonly IInterceptorsManager _interceptorsManager;
        private readonly IReducersManager _reducersManager;
        private readonly IAfterEffectsManager _afterEffectsManager;
        private readonly IDispatchContextFactory _dispatchContextFactory;

        public Dispatcher(IDispatchContextFactory dispatchContextFactory, IInterceptorsManager interceptorsManager, IReducersManager reducersManager, IAfterEffectsManager afterEffectsManager)
        {
            _dispatchContextFactory = dispatchContextFactory ?? throw new ArgumentNullException(nameof(dispatchContextFactory));
            _interceptorsManager = interceptorsManager ?? throw new ArgumentNullException(nameof(interceptorsManager));
            _reducersManager = reducersManager ?? throw new ArgumentNullException(nameof(reducersManager));
            _afterEffectsManager = afterEffectsManager ?? throw new ArgumentNullException(nameof(afterEffectsManager));
        }

        public async Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken) where TAction : IAction
        {
            var dispatchContext = _dispatchContextFactory.Create(action);
            await _interceptorsManager.DispatchAsync(dispatchContext, cancellationToken);
            await _reducersManager.DispatchAsync(dispatchContext, cancellationToken);
            await _afterEffectsManager.DispatchAsync(dispatchContext, cancellationToken);
        }
    }

    public interface IActionInterceptor<TAction>
        where TAction : IAction
    {
        Task InterceptAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken);
    }

    public interface IDispatchContext<TAction>
        where TAction : IAction
    {
        TAction Action { get; set; }
        bool StopReduce { get; set; }
        bool StopInterception { get; set; }
        bool StopAfterEffect { get; set; }
    }

    public class DispatchContext<TAction> : IDispatchContext<TAction>
        where TAction : IAction
    {
        public DispatchContext(TAction action)
        {
            Action = action;
        }

        public TAction Action { get; set; }

        public bool StopReduce { get; set; }
        public bool StopInterception { get; set; }
        public bool StopAfterEffect { get; set; }
    }

    public interface IDispatchContextFactory
    {
        IDispatchContext<TAction> Create<TAction>(TAction action) where TAction : IAction;
    }

    public class DispatchContextFactory : IDispatchContextFactory
    {
        public IDispatchContext<TAction> Create<TAction>(TAction action)
            where TAction : IAction
            => new DispatchContext<TAction>(action);
    }

    public interface IInterceptorsMiddleware
    {
        Task BeforeInterceptorsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionInterceptor<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
        Task BeforeInterceptorAsync<TAction>(IDispatchContext<TAction> context, IActionInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterInterceptorAsync<TAction>(IDispatchContext<TAction> context, IActionInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterInterceptorsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionInterceptor<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
    }

    public interface IActionHandlerMiddleware
    {
        Task BeforeHandlersAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionHandler<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
        Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterHandlersAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionHandler<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
    }
    public interface IReducersMiddleware
    {
        Task BeforeReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task BeforeReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task BeforeNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
    }

    public interface IAfterEffectsMiddleware
    {
        Task BeforeAfterEffectsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IAfterEffects<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
        Task BeforeAfterEffectAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterAfterEffectAsync<TAction>(IDispatchContext<TAction> context, IAfterEffects<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction;
        Task AfterAfterEffectsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IAfterEffects<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction;
    }


    public interface IStatorMiddleware : IInterceptorsMiddleware, IActionHandlerMiddleware, IReducersMiddleware, IAfterEffectsMiddleware
    {
    }

    public interface IDispatchManager
    {
        Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken)
            where TAction : IAction;
    }
    public interface IInterceptorsManager : IDispatchManager { }
    public interface IReducersManager : IDispatchManager { }
    public interface IAfterEffectsManager : IDispatchManager { }

    public class InterceptorsManager : IInterceptorsManager
    {
        private readonly IEnumerable<IInterceptorsMiddleware> _middlewares;
        private readonly IServiceProvider _serviceProvider;

        public InterceptorsManager(IEnumerable<IInterceptorsMiddleware> middlewares, IServiceProvider serviceProvider)
        {
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var interceptors = _serviceProvider.GetServices<IActionInterceptor<TAction>>().ToList();
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeInterceptorsAsync(dispatchContext, interceptors, cancellationToken);
            }
            foreach (var interceptor in interceptors)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeInterceptorAsync(dispatchContext, interceptor, cancellationToken);
                }
                if (dispatchContext.StopInterception)
                {
                    break;
                }
                await interceptor.InterceptAsync(dispatchContext, cancellationToken);
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterInterceptorAsync(dispatchContext, interceptor, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterInterceptorsAsync(dispatchContext, interceptors, cancellationToken);
            }
        }
    }

    public class ReducersManager : IReducersManager
    {
        private readonly IEnumerable<IActionHandlerMiddleware> _middlewares;
        private readonly IServiceProvider _serviceProvider;

        public ReducersManager(IEnumerable<IActionHandlerMiddleware> middlewares, IServiceProvider serviceProvider)
        {
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var reducerHandlers = _serviceProvider.GetServices<IActionHandler<TAction>>().ToList();
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeHandlersAsync(dispatchContext, reducerHandlers, cancellationToken);
            }
            foreach (var handler in reducerHandlers)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeHandlerAsync(dispatchContext, handler, cancellationToken);
                }
                if (dispatchContext.StopReduce)
                {
                    break;
                }
                await handler.HandleAsync(dispatchContext, cancellationToken);
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterHandlerAsync(dispatchContext, handler, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterHandlersAsync(dispatchContext, reducerHandlers, cancellationToken);
            }
        }
    }

    public class AfterEffectsManager : IAfterEffectsManager
    {
        private readonly IEnumerable<IAfterEffectsMiddleware> _middlewares;
        private readonly IServiceProvider _serviceProvider;

        public AfterEffectsManager(IEnumerable<IAfterEffectsMiddleware> middlewares, IServiceProvider serviceProvider)
        {
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var afterEffects = _serviceProvider.GetServices<IAfterEffects<TAction>>().ToList();
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeAfterEffectsAsync(dispatchContext, afterEffects, cancellationToken);
            }
            foreach (var afterEffect in afterEffects)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeAfterEffectAsync(dispatchContext, afterEffect, cancellationToken);
                }
                if (dispatchContext.StopAfterEffect)
                {
                    break;
                }
                await afterEffect.HandleAfterEffectAsync(dispatchContext, cancellationToken);
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterAfterEffectAsync(dispatchContext, afterEffect, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterAfterEffectsAsync(dispatchContext, afterEffects, cancellationToken);
            }
        }
    }
}
