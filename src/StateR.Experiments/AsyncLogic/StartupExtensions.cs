using Microsoft.Extensions.DependencyInjection.Extensions;
using StateR.ActionHandlers;
using StateR.AsyncLogic;
using StateR.Reducers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateR.Experiments.AsyncLogic
{
    public static class StartupExtensions
    {
        public static IStatorBuilder AddAsyncOperations(this IStatorBuilder builder)
        {
            //Async Operations
            builder.AddTypes(new[] { typeof(StatusUpdated<>) });

            // Async Operation's Errors
            builder.Services.TryAddSingleton<IActionHandler<AsyncError.Occured>, ReducerHandler<AsyncError.State, AsyncError.Occured>>();
            builder.Services.TryAddSingleton<IReducer<AsyncError.Occured, AsyncError.State>, AsyncError.Reducers>();
            builder.Services.TryAddSingleton<IInitialState<AsyncError.State>, AsyncError.InitialState>();
            builder.Services.TryAddSingleton<IState<AsyncError.State>, Internal.State<AsyncError.State>>();
            return builder;
        }
    }
}
