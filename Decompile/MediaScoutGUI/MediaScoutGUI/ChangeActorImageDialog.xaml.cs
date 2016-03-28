using MediaScout;
using MediaScout.Providers;
using MediaScoutGUI.Properties;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace MediaScoutGUI
{
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public partial class ChangeActorImageDialog : Window, IComponentConnector, IStyleConnector
	{
		private delegate void PostersLoadingHandler();

		private Posters[] posters;

		private ObservableCollection<LocalPosters> localPosters = new ObservableCollection<LocalPosters>();

		private ObservableCollection<Posters> Posters = new ObservableCollection<Posters>();

		private DispatchingCollection<ObservableCollection<Posters>, Posters> dispatchPosters;

		private TheMovieDBProvider tmdb;

		private TheTVDBProvider tvdb;

		private string ID;

		private string ActorsID;

		private string ActorsName;

		private bool IsMovieActor;

		public Posters selected;

		public LocalPosters selectedLocalPoster;

		public Person SelectedActor;

		public ImageWindowDecisionbType Decision = ImageWindowDecisionbType.Cancel;

		private bool jump = true;

		public ChangeActorImageDialog(string ID, string ActorsName, string ActorsID, bool IsMovieActor)
		{
			this.InitializeComponent();
			this.ID = ID;
			this.ActorsID = ActorsID;
			this.ActorsName = ActorsName;
			this.IsMovieActor = IsMovieActor;
			if (IsMovieActor)
			{
				this.tmdb = new TheMovieDBProvider(null);
			}
			else
			{
				this.tvdb = new TheTVDBProvider(null);
			}
			base.Title = "Select " + ActorsName + " Image";
			this.dispatchPosters = new DispatchingCollection<ObservableCollection<Posters>, Posters>(this.Posters, base.Dispatcher);
		}

		public void LoadPosters()
		{
			Thread thread = new Thread(new ThreadStart(delegate
            {
				if (this.ActorsID == null)
				{
					List<Person> list = this.IsMovieActor ? this.tmdb.SearchPerson(this.ActorsName) : this.tvdb.GetActors(this.ID);
					foreach (Person current in list)
					{
						if (this.ActorsName == current.Name)
						{
							this.SelectedActor = current;
						}
					}
				}
				this.ActorsID = this.SelectedActor.ID;
				this.posters = (this.IsMovieActor ? this.tmdb.GetPersonImage(this.ActorsID).ToArray() : null);
				this.AddPosters();
			}));
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		private void AddPosters()
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new ChangeActorImageDialog.PostersLoadingHandler(this.AddPosters));
				return;
			}
			if (this.posters == null || this.posters.Length == 0)
			{
				this.lblNoPosters.Content = "No Images found";
				return;
			}
			if (!this.btnDownloadAll.IsEnabled)
			{
				this.btnDownloadAll.IsEnabled = true;
			}
			if (!this.btnSelectPoster.IsEnabled)
			{
				this.btnSelectPoster.IsEnabled = true;
			}
			this.lblNoPosters.Visibility = Visibility.Collapsed;
			Posters[] array = this.posters;
			for (int i = 0; i < array.Length; i++)
			{
				Posters item = array[i];
				this.Posters.Add(item);
			}
		}

		private void AddLocalPosters()
		{
			if (!base.Dispatcher.CheckAccess())
			{
				base.Dispatcher.Invoke(DispatcherPriority.Normal, new ChangeActorImageDialog.PostersLoadingHandler(this.AddLocalPosters));
				return;
			}
			if (this.localPosters.Count == 0)
			{
				this.lbNoLocals.Visibility = Visibility.Visible;
				this.lbNoLocals.Content = "Add Local Images";
				return;
			}
			if (!this.btnSelectPoster.IsEnabled)
			{
				this.btnSelectPoster.IsEnabled = true;
			}
			this.lbNoLocals.Visibility = Visibility.Collapsed;
		}

		private void btnSelectPoster_Click(object sender, RoutedEventArgs e)
		{
			if (this.lbPosters.SelectedIndex != -1)
			{
				this.Decision = ImageWindowDecisionbType.PosterSelected;
				this.selected = (Posters)this.lbPosters.SelectedItem;
			}
			else
			{
				this.Decision = ImageWindowDecisionbType.LocalPosterSelected;
				this.selectedLocalPoster = (LocalPosters)this.lblocalPosters.SelectedItem;
			}
			base.DialogResult = new bool?(true);
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Decision = ImageWindowDecisionbType.Cancel;
			base.DialogResult = new bool?(false);
		}

		private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
		{
			this.Decision = ImageWindowDecisionbType.DownloadAll;
			base.DialogResult = new bool?(true);
		}

		private void HandleLinkClick(object sender, RoutedEventArgs e)
		{
			Hyperlink hyperlink = (Hyperlink)sender;
			string fileName = hyperlink.NavigateUri.ToString();
			Process.Start(new ProcessStartInfo(fileName));
			e.Handled = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Settings.Default.EnableGlassFrame)
			{
				this.GetBoundsForGlassFrame();
				HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
				hwndSource.AddHook(new HwndSourceHook(this.WndProc));
				this.SetGlassFrame(true);
			}
			this.lblocalPosters.ItemsSource = this.localPosters;
			this.LoadPosters();
			this.lbPosters.ItemsSource = this.dispatchPosters;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.localPosters.Clear();
		}

		public Rect GetBoundsForGlassFrame()
		{
			return VisualTreeHelper.GetContentBounds(this.lbPosters);
		}

		public bool SetGlassFrame(bool ExtendGlass)
		{
			bool result;
			if (ExtendGlass)
			{
				Rect boundsForGlassFrame = this.GetBoundsForGlassFrame();
				result = GlassHelper.ExtendGlassFrame(this, new Thickness(boundsForGlassFrame.Left, boundsForGlassFrame.Top, boundsForGlassFrame.Right, boundsForGlassFrame.Bottom));
			}
			else
			{
				GlassHelper.DisableGlassFrame(this);
				result = true;
			}
			return result;
		}

		public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			int num = 798;
			if (msg == num)
			{
				this.SetGlassFrame(GlassHelper.IsGlassEnabled);
				handled = true;
			}
			return IntPtr.Zero;
		}

		private void lbPosters_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lblocalPosters.SelectedIndex != -1 && this.jump)
			{
				this.jump = false;
				this.lblocalPosters.UnselectAll();
				return;
			}
			if (!this.jump)
			{
				this.jump = true;
			}
		}

		private void lblocalPosters_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.lbPosters.SelectedIndex != -1 && this.jump)
			{
				this.jump = false;
				this.lbPosters.UnselectAll();
				return;
			}
			if (!this.jump)
			{
				this.jump = true;
			}
		}

		private void btnDeleteAllPoster_Click(object sender, RoutedEventArgs e)
		{
			foreach (LocalPosters current in this.localPosters)
			{
				this.DeleteSelectedPoster(current, false);
			}
			this.localPosters.Clear();
			this.AddLocalPosters();
		}

		private void DeleteSelectedPoster(LocalPosters lp, bool remove)
		{
			string poster = lp.Poster;
			if (File.Exists(poster))
			{
				File.Delete(poster);
			}
			if (remove)
			{
				int index = this.localPosters.IndexOf(lp);
				this.localPosters.RemoveAt(index);
				if (this.localPosters.Count == 0)
				{
					this.AddLocalPosters();
				}
			}
		}

		private void btnDeletePoster_Click(object sender, RoutedEventArgs e)
		{
			this.DeleteSelectedPoster(this.lblocalPosters.SelectedItem as LocalPosters, true);
		}

		private void btnAddPoster_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.CheckPathExists = true;
			openFileDialog.Filter = "Image Files|*.jpg;*.tbn";
			if (openFileDialog.ShowDialog(this) == true)
			{
				string[] fileNames = openFileDialog.FileNames;
				string Filename;
				for (int i = 0; i < fileNames.Length; i++)
				{
					Filename = fileNames[i];
					try
					{
						base.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(delegate
						{
							this.localPosters.Add(new LocalPosters(Filename));
							this.AddLocalPosters();
						}));
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IStyleConnector.Connect(int connectionId, object target)
		{
			if (connectionId != 5)
			{
				return;
			}
			((Hyperlink)target).RequestNavigate += new RequestNavigateEventHandler(this.HandleLinkClick);
		}
	}
}
