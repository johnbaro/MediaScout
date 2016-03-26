using System;
using System.Xml.Serialization;

namespace MediaScout
{
	public class Studio
	{
		[XmlText]
		public string name;

		public override string ToString()
		{
			return this.name;
		}
	}
}
