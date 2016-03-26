using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaScout.Interfaces;
using MediaScoutGUI.Views;
using System.Windows;

namespace MediaScoutGUI.ViewModels
{
    public class ManageTVViewModel : ITab
    {
        private UIElement _view = new ManageTVView();


        public string Name
        {
            get { return "Manage TV"; }
        }


        public System.Windows.UIElement View { get { return _view; } }
    }
}
