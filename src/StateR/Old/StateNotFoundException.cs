using System;

namespace StateR
{
    public class StateNotFoundException : Exception
    {
        public StateNotFoundException(Type type)
            : base($"State not found for type '{type.Name}'. You must register an `IInitialState<{type.Name}>` with the service collection.")
        {
        }
    }
}