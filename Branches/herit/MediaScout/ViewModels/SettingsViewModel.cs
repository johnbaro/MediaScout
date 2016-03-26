using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaScout.Interfaces;
using System.Windows;
using MediaScoutGUI.Views;

namespace MediaScoutGUI.ViewModels
{
    public class SettingsViewModel : ITab
    {
        private UIElement _view = new SettingsView();

        public string Name
        {
            get { return "Settings"; }
        }

        public System.Windows.UIElement View { get { return _view; } }
    }
}
