using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MediaScoutGUI
{
	public class GlassHelper
	{
		private struct Margins
		{
			public int Left;

			public int Right;

			public int Top;

			public int Bottom;

			public Margins(Thickness t)
			{
				this.Left = (int)t.Left;
				this.Right = (int)t.Right;
				this.Top = (int)t.Top;
				this.Bottom = (int)t.Bottom;
			}
		}

		public static bool IsGlassEnabled
		{
			get
			{
				return GlassHelper.DwmIsCompositionEnabled();
			}
		}

		[DllImport("dwmapi.dll", PreserveSig = false)]
		private static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref GlassHelper.Margins margins);

		[DllImport("dwmapi.dll", PreserveSig = false)]
		private static extern bool DwmIsCompositionEnabled();

		public static bool ExtendGlassFrame(Window window, Thickness margin)
		{
			if (!GlassHelper.DwmIsCompositionEnabled())
			{
				return false;
			}
			IntPtr handle = new WindowInteropHelper(window).Handle;
			if (handle == IntPtr.Zero)
			{
				throw new InvalidOperationException("The Window must be shown before extending glass.");
			}
			SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Red);
			solidColorBrush.Opacity = 0.5;
			window.Background = Brushes.Transparent;
			HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Colors.Transparent;
			GlassHelper.Margins margins = new GlassHelper.Margins(margin);
			GlassHelper.DwmExtendFrameIntoClientArea(handle, ref margins);
			return true;
		}

		public static void DisableGlassFrame(Window window)
		{
			IntPtr handle = new WindowInteropHelper(window).Handle;
			if (handle == IntPtr.Zero)
			{
				throw new InvalidOperationException("The Window must be shown before extending glass.");
			}
			HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Colors.Gray;
		}
	}
}
