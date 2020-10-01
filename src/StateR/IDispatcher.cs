using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IDispatcher
    {
        Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default) where TAction : IAction;
    }
    public class Dispatcher : IDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        public Dispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default) where TAction : IAction
        {
            // TODO: Move the DispatchContext creation to an injected factory for more flexibility
            var dispatchContext = new DispatchContext<TAction>(action);

            // TODO: cache interceptors+actionHandlers+afterEffects (+benchmark the gain)
            await DispatchInterceptors(dispatchContext, cancellationToken);
            DispatchActionHandlers(dispatchContext);
            await DispatchAfterEffects(dispatchContext, cancellationToken);
        }

        protected virtual async Task DispatchAfterEffects<TAction>(DispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var afterEffects = _serviceProvider.GetServices<IAfterEffects<TAction>>().ToList();
            foreach (var afterEffect in afterEffects)
            {
                if (dispatchContext.SkipAfterEffect)
                {
                    break;
                }
                await afterEffect.HandleAfterEffectAsync(dispatchContext, cancellationToken);
            }
        }

        protected virtual void DispatchActionHandlers<TAction>(DispatchContext<TAction> dispatchContext) where TAction : IAction
        {
            var actionHandlers = _serviceProvider.GetServices<IActionHandler<TAction>>().ToList();
            foreach (var handler in actionHandlers)
            {
                if (dispatchContext.SkipReduce)
                {
                    break;
                }
                handler.Handle(dispatchContext);
            }
        }

        protected virtual async Task DispatchInterceptors<TAction>(DispatchContext<TAction> dispatchContext, CancellationToken cancellationToken) where TAction : IAction
        {
            var interceptors = _serviceProvider.GetServices<IActionInterceptor<TAction>>().ToList();
            foreach (var interceptor in interceptors)
            {
                if (dispatchContext.SkipInterception)
                {
                    break;
                }
                await interceptor.InterceptAsync(dispatchContext, cancellationToken);
            }
        }
    }

    public class LoggingDispatcher : Dispatcher
    {
        public LoggingDispatcher(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override void DispatchActionHandlers<TAction>(DispatchContext<TAction> dispatchContext)
        {
            Console.WriteLine($"DispatchActionHandlers: '{dispatchContext.Action.GetName()}'");
            base.DispatchActionHandlers(dispatchContext);
        }

        protected override Task DispatchAfterEffects<TAction>(DispatchContext<TAction> dispatchContext, CancellationToken cancellationToken)
        {
            Console.WriteLine($"DispatchAfterEffects: '{dispatchContext.Action.GetName()}'");
            return base.DispatchAfterEffects(dispatchContext, cancellationToken);
        }

        protected override Task DispatchInterceptors<TAction>(DispatchContext<TAction> dispatchContext, CancellationToken cancellationToken)
        {
            Console.WriteLine($"DispatchInterceptors: '{dispatchContext.Action.GetName()}'");
            return base.DispatchInterceptors(dispatchContext, cancellationToken);
        }
    }

    public interface IActionInterceptor<TAction>
        where TAction : IAction
    {
        Task InterceptAsync(DispatchContext<TAction> context, CancellationToken cancellationToken);
    }

    public class DispatchContext<TAction>
        where TAction : IAction
    {
        public DispatchContext(TAction action)
        {
            Action = action;
        }

        public TAction Action { get; set; }

        public bool SkipReduce { get; set; }
        public bool SkipInterception { get; set; }
        public bool SkipAfterEffect { get; set; }
    }
}
