using System;
using System.Drawing;

namespace ImdbProvider
{
	public class Person
	{
		public string Name
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}

		public Image Photo
		{
			get;
			set;
		}

		public DateTime Birthday
		{
			get;
			set;
		}

		public string Biography
		{
			get;
			set;
		}

		public string Nominations
		{
			get;
			set;
		}

		public string Character
		{
			get;
			set;
		}
	}
}
