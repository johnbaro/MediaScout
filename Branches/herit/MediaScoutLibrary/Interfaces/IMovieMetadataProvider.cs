using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MediaScout.Interfaces
{
    [InheritedExport()]
    public interface IMovieMetadataProvider : IMediaMetadataProvider
    {

    }
}
