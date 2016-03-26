using System;
using System.Collections.Generic;
using System.Text;

namespace MediaScout.Providers
{
    public interface IMovieMetadataProvider : IMetadataProvider
    {
        Movie[] Search(String MovieName);
        Movie Get(String MovieID);
    }
}
