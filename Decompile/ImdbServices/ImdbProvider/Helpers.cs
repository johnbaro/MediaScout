using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ImdbProvider
{
	public static class Helpers
	{
		public static string StripHTML(this string s)
		{
			return Regex.Replace(s, "<.*?>", "");
		}

		public static string CleanTitle(this string sString)
		{
			return new Regex("\"").Replace(sString, "").HtmlDecode();
		}

		public static string CleanHtml(this string sString)
		{
			return HttpUtility.HtmlDecode(sString.Replace("|", "").Trim());
		}

		public static string HtmlDecode(this string s)
		{
			return HttpUtility.HtmlDecode(s).Trim();
		}

		public static string UrlEncode(this string s)
		{
			return HttpUtility.UrlEncode(s, Encoding.GetEncoding("ISO-8859-1"));
		}

		public static string CapitalizeAll(this string sString)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(sString);
		}
	}
}
