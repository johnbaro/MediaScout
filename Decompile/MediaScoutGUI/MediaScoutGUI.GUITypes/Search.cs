using MediaScoutGUI.Properties;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MediaScoutGUI.GUITypes
{
	public static class Search
	{
		public static string GetSearchTerm(string SearchTerm)
		{
			string replacement = " ";
			try
			{
				string searchTermFilters = Settings.Default.SearchTermFilters;
				SearchTerm = Regex.Replace(SearchTerm, searchTermFilters, replacement, RegexOptions.IgnoreCase);
				SearchTerm = Regex.Replace(SearchTerm, "\\s+", " ");
				SearchTerm = SearchTerm.Trim();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return SearchTerm;
		}
	}
}
