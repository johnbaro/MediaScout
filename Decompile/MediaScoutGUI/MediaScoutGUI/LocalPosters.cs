using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace MediaScoutGUI
{
	public class LocalPosters
	{
		private string poster;

		private string posterfilename;

		private BitmapImage thumb;

		private string res;

		public string PosterFileName
		{
			get
			{
				return this.posterfilename;
			}
			set
			{
				this.posterfilename = value;
			}
		}

		public BitmapImage Thumb
		{
			get
			{
				return this.thumb;
			}
			set
			{
				this.thumb = value;
			}
		}

		public string Poster
		{
			get
			{
				return this.poster;
			}
			set
			{
				this.poster = value;
				this.PosterFileName = this.poster.Substring(this.poster.LastIndexOf("\\") + 1);
			}
		}

		public string Resolution
		{
			get
			{
				return this.res;
			}
			set
			{
				this.res = value;
			}
		}

		public LocalPosters(string Filename)
		{
			if (File.Exists(Filename))
			{
				this.Poster = Filename;
				this.Thumb = new BitmapImage();
				this.Thumb.BeginInit();
				this.Thumb.CacheOption = BitmapCacheOption.OnLoad;
				this.Thumb.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
				this.Thumb.UriSource = new Uri(Filename);
				this.Thumb.EndInit();
				this.Resolution = this.Thumb.PixelWidth + "x" + this.Thumb.PixelHeight;
			}
		}
	}
}
