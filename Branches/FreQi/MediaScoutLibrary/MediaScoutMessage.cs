using System;
using System.Collections.Generic;
using System.Text;

namespace MediaScout
{
    public class MediaScoutMessage
    {
        public delegate void Message(String msg, MessageType mt, DateTime time);

        public enum MessageType
        {
            NormalOperation = 0,
            CompletedOperation = 1,
            Error = 2,
            FatalError = 3,
            ProcessSeries = 4,
            ProcessSeason = 5,
            ProcessEpisode = 6
        }
    }
}
