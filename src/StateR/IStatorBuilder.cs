using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IStatorBuilder
    {
        IServiceCollection Services { get; }
        List<Type> Actions { get; }
        List<Type> States { get; }
        List<Type> All { get; }
        IStatorBuilder AddTypes(IEnumerable<Type> types);
    }
}