﻿namespace StateR.Old
{
    public class BrowsableStateOptions
    {
        public int MaxHistoryLength { get; set; }

        public bool HasAMaxHistoryLength()
        {
            return MaxHistoryLength > 0;
        }
    }
}
