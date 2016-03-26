using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaScout.Interfaces;
using MediaScoutGUI.Views;
using System.Windows;
using System.ComponentModel.Composition;
using System.Collections.ObjectModel;

namespace MediaScoutGUI.ViewModels
{
    public class BulkTVViewModel : ITab
    {
        [ImportMany(typeof(ITVMetadataProvider))]
        public ObservableCollection<ITVMetadataProvider> TVProviders { get; set; }

        private UIElement _view = new BulkTVView();

        public BulkTVViewModel()
        {
            
        }

        public string Name
        {
            get { return "Bulk TV"; }
        }


        public System.Windows.UIElement View { 
            get { return _view; } 
        }
    }
}
