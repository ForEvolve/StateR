﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StateR.ActionHandlers;
using StateR.Internal;
using System;
using System.Reflection;

namespace StateR.Validations.FluentValidation;

public class StateValidationDecorator<TState> : IState<TState>
    where TState : StateBase
{
    private readonly IState<TState> _next;
    private readonly IEnumerable<IValidator<TState>> _validators;
    private readonly ILogger _logger;

    public StateValidationDecorator(IState<TState> next, IEnumerable<IValidator<TState>> validators, ILogger<StateValidationDecorator<TState>> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Set(TState state)
    {
        var result = _validators
            .Select(validator => validator.Validate(state));
        if (result?.Any(validator => !validator.IsValid) ?? false)
        {
            var errors = result
                .Where(validator => !validator.IsValid)
                .SelectMany(validator => validator.Errors);
            var exception = new ValidationException(errors);
            _logger.LogError(
                exception,
                message: "A validation error occured on state {StateName}",
                args: typeof(TState).GetStatorName());
            throw exception;
        }
        _next.Set(state);
    }

    public TState Current => _next.Current;
    public void Notify() => _next.Notify();
    public void Subscribe(Action stateHasChangedDelegate)
        => _next.Subscribe(stateHasChangedDelegate);
    public void Unsubscribe(Action stateHasChangedDelegate)
        => _next.Unsubscribe(stateHasChangedDelegate);
}

public static class StateValidatorStartupExtensions
{
    public static IServiceCollection AddStateValidation(this IServiceCollection services, params Assembly[] assembliesToScan)
    {
        ArgumentNullException.ThrowIfNull(assembliesToScan, nameof(assembliesToScan));
        if (assembliesToScan.Length == 0) { throw new ArgumentOutOfRangeException(nameof(assembliesToScan)); }

        var allTypes = assembliesToScan
            .SelectMany(a => a.GetTypes());

        RegisterStateDecorator(services, allTypes);
        ActionHandlerDecorator(services);
        return services;
    }
    private static void ActionHandlerDecorator(IServiceCollection services)
    {
        Console.WriteLine("- Decorate<IActionHandlersManager>, ValidationExceptionActionHandlersManagerDecorator>()");
        services.Decorate<IActionHandlersManager, ValidationExceptionActionHandlersManagerDecorator>();

    }
    private static void RegisterStateDecorator(IServiceCollection services, IEnumerable<Type> types)
    {
        var states = TypeScanner.FindStates(types);
        Console.WriteLine("StateValidator:");
        foreach (var state in states)
        {
            Console.WriteLine($"- Decorate<IState<{state.GetStatorName()}>, StateValidationDecorator<{state.GetStatorName()}>>()");

            // Equivalent to: Decorate<IState<TState>, StateValidationDecorator<TState>>();
            var stateType = typeof(IState<>).MakeGenericType(state);
            var stateSessionDecoratorType = typeof(StateValidationDecorator<>).MakeGenericType(state);
            services.Decorate(stateType, stateSessionDecoratorType);
        }
    }
}

//public class ValidationExceptionActionHandlerDecorator<TAction> : IActionHandler<TAction>
//    where TAction : IAction
//{
//    private readonly IActionHandler<TAction> _next;
//    public ValidationExceptionActionHandlerDecorator(IActionHandler<TAction> next)
//    {
//        _next = next ?? throw new ArgumentNullException(nameof(next));
//    }

//    public async Task HandleAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
//    {
//        try
//        {
//            await _next.HandleAsync(context, cancellationToken);
//        }
//        catch (ValidationException ex)
//        {
//            await context.Dispatcher.DispatchAsync(new ReplaceValidationError(ex.Errors), cancellationToken);
//        }
//    }
//}

public class ValidationExceptionActionHandlersManagerDecorator : IActionHandlersManager
{
    private readonly IActionHandlersManager _next;
    public ValidationExceptionActionHandlersManagerDecorator(IActionHandlersManager next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task DispatchAsync<TAction>(IDispatchContext<TAction> dispatchContext)
        where TAction : IAction
    {
        try
        {
            await _next.DispatchAsync(dispatchContext);
        }
        catch (ValidationException ex)
        {
            await dispatchContext.Dispatcher.DispatchAsync(
                new ReplaceValidationError(ex.Errors),
                dispatchContext.CancellationToken
            );
        }
    }
}