using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaScoutGUI.GUITypes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MediaScoutGUI
{
    public class MoviesSearch : INotifyPropertyChanged
    {

        #region INotifyPropertyChanged Members
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        
        #endregion

        #region Properties

        private Movie movie = null;
        public Movie Movie
        {
            get { return movie; }
            set
            {
                movie = value;
                NotifyPropertyChanged("Movie");
            }
        }

        private MediaScout.MovieXML selectedMovie = null;
        public MediaScout.MovieXML SelectedMovie
        {
            get { return selectedMovie; }
            set
            {
                selectedMovie = value;
                if (String.IsNullOrEmpty(SelectedMovie.Title) || String.IsNullOrEmpty(SelectedMovie.Year))
                {
                    NeedsAttention = true;
                    Skip = true;
                }
                else
                {
                    if(searchresults.Length == 1)
                        NeedsAttention = false;
                    Skip = false;
                }
                NotifyPropertyChanged("SelectedMovie");
            }
        }

        private String[] titles = null;
        public String[] Titles
        {
            get { return titles; }
            set
            {
                titles = value;
                NotifyPropertyChanged("Titles");
            }
        }

        private MediaScout.MovieXML[] searchresults;
        public MediaScout.MovieXML[] SearchResults
        {
            get { return searchresults; }
            set
            {
                searchresults = value;
                if (searchresults != null && searchresults.Length > 0)
                {
                    if (searchresults.Length > 1)
                    {
                        SetBestMatchOnTopInMovieResults(SearchResults, Movie, Movie.SearchTerm);
                        NeedsAttention = true;
                    }
                    else
                        NeedsAttention = false;
                    HasSearch = true;
                    Skip = false;
                }                
                else
                {
                    needsAttention = true;
                    HasSearch = false;
                    Skip = true;
                }
                NotifyPropertyChanged("SearchResults");
            }
        }

        private bool hassearch = false;
        public bool HasSearch
        {
            get { return hassearch; }
            set
            {
                hassearch = value;
                NotifyPropertyChanged("HasSearch");
            }
        }

        private bool skip = false;
        public bool Skip
        {
            get{ return skip; }
            set
            {
                skip = value;
                NotifyPropertyChanged("Skip");
            }
        }

        private bool needsAttention = false;
        public bool NeedsAttention
        {
            get { return needsAttention; }
            set
            {
                needsAttention = value;
                NotifyPropertyChanged("NeedsAttention");
            }
        }
        
        #endregion

        private void SetBestMatchOnTopInMovieResults(object[] results, Movie movie, String SearchTerm)
        {
            int i = 0;
            bool found = false;
            int foundindex = 0;

            if (movie.ID != null)
            {
                foreach (MediaScout.MovieXML m in results)
                {
                    if (movie.ID == m.ID)
                    {
                        foundindex = i;
                        found = true;
                        break;
                    }
                    i++;
                }
            }
            else
            {
                if (movie.Year != null)
                {
                    List<int> SortedByYear = new List<int>();
                    foreach (MediaScout.MovieXML m in results)
                    {
                        if (m.Year != null)
                        {
                            if (m.Year == movie.Year)
                            {
                                if (m.Title == SearchTerm)
                                {
                                    foundindex = i;
                                    found = true;
                                    break;
                                }
                                SortedByYear.Add(i);
                            }
                        }
                        else
                            SortedByYear.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        List<int> SortedByName = new List<int>();
                        foreach (int index in SortedByYear)
                        {
                            if ((results[index] as MediaScout.MovieXML).Title == SearchTerm)
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                            if ((results[index] as MediaScout.MovieXML).Year != null)
                                SortedByName.Add(index);
                        }
                        if (!found)
                        {
                            foreach (int index in SortedByName)
                            {
                                if ((results[index] as MediaScout.MovieXML).Title.Contains(SearchTerm))
                                {
                                    foundindex = index;
                                    found = true;
                                    break;
                                }
                            }
                            if (SortedByName.Count != 0 && !found)
                            {
                                foundindex = SortedByName[0];
                                found = true;
                            }
                        }
                        if (SortedByYear.Count != 0 && !found)
                        {
                            foundindex = SortedByYear[0];
                            found = true;
                        }
                    }
                }
                else
                {
                    List<int> SortedByName = new List<int>();
                    foreach (MediaScout.MovieXML m in results)
                    {
                        if (m.Title == SearchTerm)
                        {
                            foundindex = i;
                            found = true;
                            break;
                        }
                        SortedByName.Add(i);
                        i++;
                    }
                    if (!found)
                    {
                        foreach (int index in SortedByName)
                        {
                            if ((results[index] as MediaScout.MovieXML).Title.Contains(SearchTerm))
                            {
                                foundindex = index;
                                found = true;
                                break;
                            }
                        }
                        if (SortedByName.Count != 0 && !found)
                        {
                            foundindex = SortedByName[0];
                            found = true;
                        }
                    }
                }
            }

            if (found)
            {
                object temp;
                temp = results[0];
                results[0] = results[foundindex];
                results[foundindex] = temp;
            }
        }
        public MoviesSearch(Movie m, MediaScout.MovieXML[] sr)
        {
            Movie = m;            
            SearchResults = sr;            
        }

    }
}
