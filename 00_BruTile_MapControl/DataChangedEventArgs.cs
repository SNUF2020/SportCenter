using System;

namespace SC
{
    public delegate void DataChangedEventHandler<T>(object sender, DataChangedEventArgs<T> e);

    public class DataChangedEventArgs<T>
    {
        public DataChangedEventArgs(Exception error, bool cancelled, Tile<T> tile)
        {
            Error = error;
            Cancelled = cancelled;
            Tile = tile;
        }

        public Exception Error { get; private set; }
        public bool Cancelled { get; private set; }
        public Tile<T> Tile { get; private set; }
    }
}
