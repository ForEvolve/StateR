namespace Microsoft.Extensions.DependencyInjection
{
    public interface IStatorBuilder
    {
        IServiceCollection Services { get; }
    }
}