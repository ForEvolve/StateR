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

        // Validation action
        builder.AddTypes(new[] {
            typeof(AddValidationErrors),
            typeof(ReplaceValidationErrors),
            typeof(ValidationUpdaters),
            typeof(ValidationInitialState),
            typeof(ValidationState),
            typeof(ValidationFilter<,>),
        });
        builder.AddMiddlewares(new[] { typeof(ValidationFilter<,>) });

        // Validation interceptor and state
        builder.Services.TryAddSingleton(typeof(IActionFilter<,>), typeof(ValidationFilter<,>));

        // Scan for validators
        builder.Services.AddValidatorsFromAssemblies(assembliesToScan, ServiceLifetime.Singleton);

        return builder;
    }
}
