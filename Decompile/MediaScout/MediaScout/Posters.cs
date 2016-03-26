using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace MediaScout
{
	public class Posters
	{
		private string poster;

		private string posterfilename;

		private string thumb;

		private string res;

		public string Poster
		{
			get
			{
				return this.poster;
			}
			set
			{
				this.poster = value;
				this.posterfilename = this.poster.Substring(this.poster.LastIndexOf("/") + 1);
			}
		}

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

		public string Thumb
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

		public void SavePoster(string filepath)
		{
			this.savegraphic(this.poster, filepath);
		}

		public void SaveThumb(string filepath)
		{
			this.savegraphic(this.thumb, filepath);
		}

		private void savegraphic(string fileIn, string fileOut)
		{
			try
			{
				WebRequest webRequest = WebRequest.Create(fileIn);
				WebResponse response = webRequest.GetResponse();
				Image original = Image.FromStream(response.GetResponseStream());
				Bitmap bitmap = new Bitmap(original);
				bitmap.Save(fileOut, ImageFormat.Jpeg);
			}
			catch (System.Exception ex)
			{
				System.IO.File.Delete(fileOut);
				throw new System.Exception(ex.Message, ex.InnerException);
			}
		}
	}
}
