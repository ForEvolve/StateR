using MediatR;
using MediatR.Pipeline;
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
            var iPipelineBehaviorType = typeof(IPipelineBehavior<,>);
            var actionLoggerType = typeof(ActionLogger<>);
            foreach (var type in builder.Actions)
            {
                // Equivalent to: 
                // - AddSingleton<IPipelineBehavior<TAction, Unit>, ActionLogger<TAction>>();
                var serviceType = iPipelineBehaviorType.MakeGenericType(type, StatorStartupExtensions.unitType);
                var implementationType = actionLoggerType.MakeGenericType(type);
                builder.Services.AddSingleton(serviceType, implementationType);
            }
            return builder;
        }

        public class ActionLogger<TAction> : IPipelineBehavior<TAction, Unit>
        {
            public async Task<Unit> Handle(TAction action, CancellationToken cancellationToken, RequestHandlerDelegate<Unit> next)
            {
                var actionName = action.GetType().Name;
                Console.WriteLine($"[ActionLogger] Begin {actionName}");
                var response = await next();
                Console.WriteLine($"[ActionLogger] End {actionName}");
                return response;
            }
        }
    }
}
