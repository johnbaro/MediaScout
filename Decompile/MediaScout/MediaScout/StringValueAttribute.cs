using System;

namespace MediaScout
{
	public class StringValueAttribute : System.Attribute
	{
		private string _value;

		public string Value
		{
			get
			{
				return this._value;
			}
		}

		public StringValueAttribute(string value)
		{
			this._value = value;
		}
	}
}
