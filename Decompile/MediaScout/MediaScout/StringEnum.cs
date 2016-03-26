using System;
using System.Collections;
using System.Reflection;

namespace MediaScout
{
	public static class StringEnum
	{
		private static System.Collections.Hashtable _stringValues = new System.Collections.Hashtable();

		public static string GetStringValue(System.Enum value)
		{
			string result = null;
			System.Type type = value.GetType();
			if (StringEnum._stringValues.ContainsKey(value))
			{
				result = (StringEnum._stringValues[value] as StringValueAttribute).Value;
			}
			else
			{
				System.Reflection.FieldInfo field = type.GetField(value.ToString());
				StringValueAttribute[] array = field.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
				if (array.Length > 0)
				{
					StringEnum._stringValues.Add(value, array[0]);
					result = array[0].Value;
				}
			}
			return result;
		}
	}
}
