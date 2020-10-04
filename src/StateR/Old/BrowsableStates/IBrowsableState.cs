using System.Collections.Generic;

namespace StateR.Old
{
    public interface IBrowsableState<TState> : IState<TState>
        where TState : StateBase
    {
        IReadOnlyCollection<StateHistoryEntry<TState>> History { get; }
        StateHistoryEntry<TState> Cursor { get; }
        bool Undo();
        bool Redo();
    }
}
