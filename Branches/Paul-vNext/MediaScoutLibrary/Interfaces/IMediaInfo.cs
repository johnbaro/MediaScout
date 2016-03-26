using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaScout.Interfaces
{
    public interface IMediaInfo
    {
        String Title { get; set; }
        
        bool Save(String Filename="");
    }
}
