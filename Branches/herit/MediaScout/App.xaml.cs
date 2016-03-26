using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Collections.ObjectModel;
using MediaScout.Interfaces;

namespace MediaScoutGUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        private AggregateCatalog catalog = new AggregateCatalog();
        private CompositionContainer container;
        private AppHost host;

        [ImportMany(typeof(ITab))]
        public ObservableCollection<ITab> Tabs { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(System.IO.Path.GetDirectoryName(assembly.Location)));
            container = new CompositionContainer(catalog);

            container.ComposeParts(this);

            host = new AppHost();
            host.Show();
        }
    }
}
