using System;
using System.Net;

namespace ImdbProvider
{
	public class RequestState
	{
		public WebRequest Request;

		public string MovieTitle;

		public RequestState()
		{
			this.Request = null;
			this.MovieTitle = string.Empty;
		}
	}
}
