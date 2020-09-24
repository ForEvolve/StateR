using System;

namespace StateR
{
    public record StateHistoryEntry<TState>(TState State, DateTime CreatedTime)
        where TState : StateBase;
}
