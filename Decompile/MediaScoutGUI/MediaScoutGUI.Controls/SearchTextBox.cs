using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MediaScoutGUI.Controls
{
	public class SearchTextBox : TextBox
	{
		public static DependencyProperty LabelTextProperty;

		public static DependencyProperty LabelTextColorProperty;

		public static DependencyProperty SearchModeProperty;

		private static DependencyPropertyKey HasTextPropertyKey;

		public static DependencyProperty HasTextProperty;

		private static DependencyPropertyKey IsMouseLeftButtonDownPropertyKey;

		public static DependencyProperty IsMouseLeftButtonDownProperty;

		public static DependencyProperty SearchEventTimeDelayProperty;

		public static readonly RoutedEvent SearchEvent;

		private DispatcherTimer searchEventDelayTimer;

		public event RoutedEventHandler Search
		{
			add
			{
				base.AddHandler(SearchTextBox.SearchEvent, value);
			}
			remove
			{
				base.RemoveHandler(SearchTextBox.SearchEvent, value);
			}
		}

		public string LabelText
		{
			get
			{
				return (string)base.GetValue(SearchTextBox.LabelTextProperty);
			}
			set
			{
				base.SetValue(SearchTextBox.LabelTextProperty, value);
			}
		}

		public Brush LabelTextColor
		{
			get
			{
				return (Brush)base.GetValue(SearchTextBox.LabelTextColorProperty);
			}
			set
			{
				base.SetValue(SearchTextBox.LabelTextColorProperty, value);
			}
		}

		public SearchMode SearchMode
		{
			get
			{
				return (SearchMode)base.GetValue(SearchTextBox.SearchModeProperty);
			}
			set
			{
				base.SetValue(SearchTextBox.SearchModeProperty, value);
			}
		}

		public bool HasText
		{
			get
			{
				return (bool)base.GetValue(SearchTextBox.HasTextProperty);
			}
			private set
			{
				base.SetValue(SearchTextBox.HasTextPropertyKey, value);
			}
		}

		public Duration SearchEventTimeDelay
		{
			get
			{
				return (Duration)base.GetValue(SearchTextBox.SearchEventTimeDelayProperty);
			}
			set
			{
				base.SetValue(SearchTextBox.SearchEventTimeDelayProperty, value);
			}
		}

		public bool IsMouseLeftButtonDown
		{
			get
			{
				return (bool)base.GetValue(SearchTextBox.IsMouseLeftButtonDownProperty);
			}
			private set
			{
				base.SetValue(SearchTextBox.IsMouseLeftButtonDownPropertyKey, value);
			}
		}

		static SearchTextBox()
		{
			SearchTextBox.LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(SearchTextBox));
			SearchTextBox.LabelTextColorProperty = DependencyProperty.Register("LabelTextColor", typeof(Brush), typeof(SearchTextBox));
			SearchTextBox.SearchModeProperty = DependencyProperty.Register("SearchMode", typeof(SearchMode), typeof(SearchTextBox), new PropertyMetadata(SearchMode.Instant));
			SearchTextBox.HasTextPropertyKey = DependencyProperty.RegisterReadOnly("HasText", typeof(bool), typeof(SearchTextBox), new PropertyMetadata());
			SearchTextBox.HasTextProperty = SearchTextBox.HasTextPropertyKey.DependencyProperty;
			SearchTextBox.IsMouseLeftButtonDownPropertyKey = DependencyProperty.RegisterReadOnly("IsMouseLeftButtonDown", typeof(bool), typeof(SearchTextBox), new PropertyMetadata());
			SearchTextBox.IsMouseLeftButtonDownProperty = SearchTextBox.IsMouseLeftButtonDownPropertyKey.DependencyProperty;
			SearchTextBox.SearchEventTimeDelayProperty = DependencyProperty.Register("SearchEventTimeDelay", typeof(Duration), typeof(SearchTextBox), new FrameworkPropertyMetadata(new Duration(new TimeSpan(0, 0, 0, 0, 500)), new PropertyChangedCallback(SearchTextBox.OnSearchEventTimeDelayChanged)));
			SearchTextBox.SearchEvent = EventManager.RegisterRoutedEvent("Search", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchTextBox));
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTextBox), new FrameworkPropertyMetadata(typeof(SearchTextBox)));
		}

		public SearchTextBox()
		{
			this.searchEventDelayTimer = new DispatcherTimer();
			this.searchEventDelayTimer.Interval = this.SearchEventTimeDelay.TimeSpan;
			this.searchEventDelayTimer.Tick += new EventHandler(this.OnSeachEventDelayTimerTick);
		}

		private void OnSeachEventDelayTimerTick(object o, EventArgs e)
		{
			this.searchEventDelayTimer.Stop();
			this.RaiseSearchEvent();
		}

		private static void OnSearchEventTimeDelayChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			SearchTextBox searchTextBox = o as SearchTextBox;
			if (searchTextBox != null)
			{
				searchTextBox.searchEventDelayTimer.Interval = ((Duration)e.NewValue).TimeSpan;
				searchTextBox.searchEventDelayTimer.Stop();
			}
		}

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			this.HasText = (base.Text.Length != 0);
			if (this.SearchMode == SearchMode.Instant)
			{
				this.searchEventDelayTimer.Stop();
				this.searchEventDelayTimer.Start();
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Border border = base.GetTemplateChild("PART_SearchIconBorder") as Border;
			if (border != null)
			{
				border.MouseLeftButtonDown += new MouseButtonEventHandler(this.IconBorder_MouseLeftButtonDown);
				border.MouseLeftButtonUp += new MouseButtonEventHandler(this.IconBorder_MouseLeftButtonUp);
				border.MouseLeave += new MouseEventHandler(this.IconBorder_MouseLeave);
			}
		}

		private void IconBorder_MouseLeftButtonDown(object obj, MouseButtonEventArgs e)
		{
			this.IsMouseLeftButtonDown = true;
		}

		private void IconBorder_MouseLeftButtonUp(object obj, MouseButtonEventArgs e)
		{
			if (!this.IsMouseLeftButtonDown)
			{
				return;
			}
			if (this.HasText && this.SearchMode == SearchMode.Instant)
			{
				base.Text = "";
			}
			if (this.HasText && this.SearchMode == SearchMode.Delayed)
			{
				this.RaiseSearchEvent();
			}
			this.IsMouseLeftButtonDown = false;
		}

		private void IconBorder_MouseLeave(object obj, MouseEventArgs e)
		{
			this.IsMouseLeftButtonDown = false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape && this.SearchMode == SearchMode.Instant)
			{
				base.Text = "";
				return;
			}
			if ((e.Key == Key.Return || e.Key == Key.Return) && this.SearchMode == SearchMode.Delayed)
			{
				this.RaiseSearchEvent();
				return;
			}
			base.OnKeyDown(e);
		}

		private void RaiseSearchEvent()
		{
			RoutedEventArgs e = new RoutedEventArgs(SearchTextBox.SearchEvent);
			base.RaiseEvent(e);
		}
	}
}
