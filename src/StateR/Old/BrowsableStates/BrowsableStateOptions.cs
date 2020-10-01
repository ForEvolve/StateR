namespace StateR
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
