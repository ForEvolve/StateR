using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StateR.Pipeline;
using System.Reflection;

namespace StateR.Validations.FluentValidation;

public static class StartupExtensions
{
    public static IStatorBuilder AddFluentValidation(this IStatorBuilder builder, params Assembly[] assembliesToScan)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(assembliesToScan, nameof(assembliesToScan));

        // Add state, actions, and updaters
        builder
            .AddState<ValidationState, ValidationInitialState>()
            .AddAction(typeof(AddValidationErrors))
            .AddAction(typeof(ReplaceValidationErrors))
            .AddUpdaters(typeof(ValidationUpdaters))
        ;

        // Validation interceptor
        builder.Services.TryAddSingleton(typeof(IActionFilter<,>), typeof(ValidationFilter<,>));

        // Scan for validators
        builder.Services.AddValidatorsFromAssemblies(assembliesToScan, ServiceLifetime.Singleton);

        return builder;
    }
}
