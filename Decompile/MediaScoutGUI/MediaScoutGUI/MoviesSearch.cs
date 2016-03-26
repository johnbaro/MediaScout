using MediaScout;
using MediaScoutGUI.GUITypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MediaScoutGUI
{
	public class MoviesSearch : INotifyPropertyChanged
	{
		private Movie movie;

		private MovieXML selectedMovie;

		private string[] titles;

		private MovieXML[] searchresults;

		private bool hassearch;

		private bool skip;

		private bool needsAttention;

		public event PropertyChangedEventHandler PropertyChanged;

		public Movie Movie
		{
			get
			{
				return this.movie;
			}
			set
			{
				this.movie = value;
				this.NotifyPropertyChanged("Movie");
			}
		}

		public MovieXML SelectedMovie
		{
			get
			{
				return this.selectedMovie;
			}
			set
			{
				this.selectedMovie = value;
				if (this.selectedMovie != null)
				{
					List<string> list = new List<string>();
					list.Add(this.selectedMovie.Title);
					if (!string.IsNullOrEmpty(this.selectedMovie.Alt_Title) && this.selectedMovie.Alt_Title != this.selectedMovie.Title)
					{
						list.Add(this.selectedMovie.Alt_Title);
					}
					this.Titles = list.ToArray();
				}
				this.SetAttentionAndSkip();
				this.NotifyPropertyChanged("SelectedMovie");
			}
		}

		public string[] Titles
		{
			get
			{
				return this.titles;
			}
			set
			{
				this.titles = value;
				this.NotifyPropertyChanged("Titles");
			}
		}

		public MovieXML[] SearchResults
		{
			get
			{
				return this.searchresults;
			}
			set
			{
				this.searchresults = value;
				if (this.searchresults != null && this.searchresults.Length > 0)
				{
					if (this.searchresults.Length > 1)
					{
						this.SetBestMatchOnTopInMovieResults(this.SearchResults, this.Movie, this.Movie.SearchTerm);
						this.SelectedMovie = this.searchresults[0];
					}
					else
					{
						this.SetAttentionAndSkip();
					}
					this.HasSearch = true;
				}
				else
				{
					this.HasSearch = false;
					this.SetAttentionAndSkip();
				}
				this.NotifyPropertyChanged("SearchResults");
			}
		}

		public bool HasSearch
		{
			get
			{
				return this.hassearch;
			}
			set
			{
				this.hassearch = value;
				this.NotifyPropertyChanged("HasSearch");
			}
		}

		public bool Skip
		{
			get
			{
				return this.skip;
			}
			set
			{
				this.skip = value;
				this.NotifyPropertyChanged("Skip");
			}
		}

		public bool NeedsAttention
		{
			get
			{
				return this.needsAttention;
			}
			set
			{
				this.needsAttention = value;
				this.NotifyPropertyChanged("NeedsAttention");
			}
		}

		private void NotifyPropertyChanged(string info)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		public void SetAttentionAndSkip()
		{
			if (this.hassearch && this.searchresults.Length != 0 && this.searchresults.Length <= 1 && this.SelectedMovie != null && !string.IsNullOrEmpty(this.selectedMovie.Title) && !string.IsNullOrEmpty(this.selectedMovie.Year))
			{
				this.NeedsAttention = false;
				this.Skip = false;
				return;
			}
			this.NeedsAttention = true;
			if (!this.hassearch || this.searchresults.Length == 0 || this.SelectedMovie == null || string.IsNullOrEmpty(this.selectedMovie.Title) || string.IsNullOrEmpty(this.selectedMovie.Year))
			{
				this.Skip = true;
				return;
			}
			this.Skip = false;
		}

		private void SetBestMatchOnTopInMovieResults(object[] results, Movie movie, string SearchTerm)
		{
			int num = 0;
			bool flag = false;
			int num2 = 0;
			if (movie.ID != null)
			{
				for (int i = 0; i < results.Length; i++)
				{
					MovieXML movieXML = (MovieXML)results[i];
					if (movie.ID == movieXML.ID)
					{
						num2 = num;
						flag = true;
						break;
					}
					num++;
				}
			}
			else if (movie.Year != null)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < results.Length; j++)
				{
					MovieXML movieXML2 = (MovieXML)results[j];
					if (!string.IsNullOrEmpty(movieXML2.Year))
					{
						if (movieXML2.Year == movie.Year)
						{
							if (movieXML2.Title == SearchTerm)
							{
								num2 = num;
								flag = true;
								break;
							}
							list.Add(num);
						}
					}
					else if (movieXML2.Title == SearchTerm)
					{
						num2 = num;
						flag = true;
						list.Add(num);
					}
					num++;
				}
				if (!flag)
				{
					foreach (int current in list)
					{
						if ((results[current] as MovieXML).Title.Contains(SearchTerm))
						{
							num2 = current;
							flag = true;
							break;
						}
					}
					if (!flag && list.Count > 0)
					{
						num2 = list[0];
						flag = true;
					}
				}
			}
			else
			{
				for (int k = 0; k < results.Length; k++)
				{
					MovieXML movieXML3 = (MovieXML)results[k];
					if (movieXML3.Title == SearchTerm)
					{
						num2 = num;
						flag = true;
						break;
					}
					if (movieXML3.Title.Contains(SearchTerm))
					{
						num2 = num;
						flag = true;
					}
				}
			}
			if (flag)
			{
				object obj = results[0];
				results[0] = results[num2];
				results[num2] = obj;
			}
		}

		public MoviesSearch(Movie m, MovieXML[] sr)
		{
			this.Movie = m;
			this.SearchResults = sr;
		}
	}
}
