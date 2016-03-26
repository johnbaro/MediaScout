using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaScout.Interfaces;
using MediaScoutGUI.Views;
using System.Windows;

namespace MediaScoutGUI.ViewModels
{
    public class DropTVViewModel : ITab
    {
        private UIElement _view = new DropTVView();

        public string Name
        {
            get { return "Drop TV"; }
        }


        public System.Windows.UIElement View { get { return _view; } }
    }
}
