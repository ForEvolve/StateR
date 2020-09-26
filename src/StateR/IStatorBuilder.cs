using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IStatorBuilder
    {
        IServiceCollection Services { get; }
        ReadOnlyCollection<Type> States { get; }
        ReadOnlyCollection<Type> All { get; }
    }
}