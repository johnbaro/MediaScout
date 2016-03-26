using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;

namespace MediaScout.Interfaces
{
    [InheritedExport()]
    public interface ITab
    {
        String Name { get; }
        UIElement View { get; }
    }
}
