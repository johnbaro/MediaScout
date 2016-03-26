using System;

namespace MediaScout
{
	public class MediaScoutMessage
	{
		public delegate void Message(string msg, MediaScoutMessage.MessageType mt, int level);

		public enum MessageType
		{
			Task,
			TaskResult,
			TaskError,
			Error,
			FatalError
		}
	}
}
