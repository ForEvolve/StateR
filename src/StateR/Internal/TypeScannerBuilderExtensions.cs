﻿using StateR.ActionHandlers;
using StateR.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StateR.Internal
{
    public static class TypeScannerBuilderExtensions
    {
        public static IStatorBuilder ScanTypes(this IStatorBuilder builder)
        {
            return builder
                .FindStates()
                .FindActions()
                .FindUpdaters()
                .FindActionHandlers()
            ;
        }

        public static IStatorBuilder FindStates(this IStatorBuilder builder)
        {
            var states = builder.All
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(StateBase)));
            return builder.AddStates(states);
        }

        public static IStatorBuilder FindActions(this IStatorBuilder builder)
        {
            var actions = builder.All
                .Where(type => !type.IsAbstract && type
                .GetTypeInfo()
                .GetInterfaces()
                .Any(i => i == typeof(IAction))
            );
            return builder.AddActions(actions);
        }

        public static IStatorBuilder FindUpdaters(this IStatorBuilder builder)
        {
            var updaters = builder.All
                .Where(type => !type.IsAbstract && type
                .GetTypeInfo()
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUpdater<,>))
            );
            return builder.AddUpdaters(updaters);
        }
        public static IStatorBuilder FindActionHandlers(this IStatorBuilder builder)
        {
            var handlers= builder.All
                .Where(type => !type.IsAbstract && type
                .GetTypeInfo()
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IActionHandler<>))
            );
            return builder.AddUpdaters(handlers);
        }

        //public IStatorBuilder FindInterceptors(this IStatorBuilder builder)
        //{
        //    var iActionInterceptor = typeof(IInterceptor<>);
        //    return types.Where(type => !type.IsAbstract && type
        //        .GetTypeInfo()
        //        .GetInterfaces()
        //        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iActionInterceptor)
        //    );
        //}

        //public IStatorBuilder FindAfterEffects(this IStatorBuilder builder)
        //{
        //    var iAfterEffects = typeof(IAfterEffects<>);
        //    return types.Where(type => !type.IsAbstract && type
        //        .GetTypeInfo()
        //        .GetInterfaces()
        //        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iAfterEffects)
        //    );
        //}

    }
}
