using System;
using System.Xml.Serialization;

namespace MediaScout
{
	public class Genre
	{
		[XmlText]
		public string name;

		public override string ToString()
		{
			return this.name;
		}
	}
}
