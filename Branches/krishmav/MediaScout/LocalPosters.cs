using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;

namespace MediaScoutGUI
{
    public class LocalPosters
    {
        String poster;
        String posterfilename;
        BitmapImage thumb;
        String res;

        public BitmapImage Thumb
        {
            get { return thumb; }
            set { thumb = value; }
        }

        public String Poster
        {
            get { return poster; }
            set { poster = value;
                posterfilename = poster.Substring(poster.LastIndexOf("\\")+1);
            }
        }
       
        public String Resolution
        {
            get { return res; }
            set { res = value; }
        }

        public LocalPosters(String Filename)
        {
            if (File.Exists(Filename))
            {
                Poster = Filename;

                Thumb = new BitmapImage();
                Thumb.BeginInit();
                Thumb.CacheOption = BitmapCacheOption.OnLoad;
                Thumb.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                Thumb.UriSource = new Uri(Filename);
                Thumb.EndInit();

                Resolution = Thumb.PixelWidth + "x" + Thumb.PixelHeight;

            }
        }
    }
}
