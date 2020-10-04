using System;

namespace StateR.Old
{
    public record StateHistoryEntry<TState>(TState State, DateTime CreatedTime)
        where TState : StateBase;
}
