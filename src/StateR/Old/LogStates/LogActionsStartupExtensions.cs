﻿using StateR;
using StateR.AfterEffects;
using StateR.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LogActionsStartupExtensions
    {
        public static IStatorBuilder AddActionLogger(this IStatorBuilder builder)
        {
            var actionInterceptorType = typeof(IActionInterceptor<>);
            var afterEffects = typeof(IActionAfterEffects<>);
            var actionLoggerType = typeof(ActionLogger<>);
            foreach (var type in builder.Actions)
            {
                // Equivalent to: 
                // - AddSingleton<IActionInterceptor<TAction>, ActionLogger<TAction>>();
                // - AddSingleton<IAfterEffects<TAction>, ActionLogger<TAction>>();
                var actionInterceptorServiceType = actionInterceptorType.MakeGenericType(type);
                var afterEffectsServiceType = afterEffects.MakeGenericType(type);
                var implementationType = actionLoggerType.MakeGenericType(type);
                builder.Services
                    .AddSingleton(actionInterceptorServiceType, implementationType)
                    .AddSingleton(afterEffectsServiceType, implementationType);
            }
            return builder;
        }

        public class ActionLogger<TAction> : IActionInterceptor<TAction>, IActionAfterEffects<TAction>
            where TAction : IAction
        {
            public Task InterceptAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
            {
                var actionName = context.Action.GetName();
                Console.WriteLine($"[ActionLogger] Begin {actionName}");
                return Task.CompletedTask;
            }

            public Task HandleAfterEffectAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
            {
                var actionName = context.Action.GetName();
                Console.WriteLine($"[ActionLogger] End {actionName}");
                return Task.CompletedTask;
            }
        }
    }
}