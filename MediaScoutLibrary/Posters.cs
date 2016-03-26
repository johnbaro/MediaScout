using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Drawing.Imaging;

namespace MediaScout
{
    public class Posters
    {
        String poster;
        String thumb;
        String res;

        public String Poster
        {
            get { return poster; }
            set { poster = value; }
        }

        public String Thumb
        {
            get { return thumb; }
            set { thumb = value; }
        }

        public String Resolution
        {
            get { return res; }
            set { res = value; }
        }

        /// <summary>
        /// Saves the poster to a specified file
        /// </summary>
        /// <param name="filepath"></param>
        public void SavePoster(String filepath)
        {
            savegraphic(this.poster, filepath);
        }

        /// <summary>
        /// Saves the thumbnail to a specified file
        /// </summary>
        /// <param name="filepath"></param>
        public void SaveThumb(String filepath)
        {
            savegraphic(this.thumb, filepath);
        }

        private void savegraphic(String fileIn, String fileOut)
        {
            WebRequest requestPic = WebRequest.Create(fileIn);
            WebResponse responsePic = requestPic.GetResponse();
            System.Drawing.Image temp = System.Drawing.Image.FromStream(responsePic.GetResponseStream());

            temp.Save(fileOut);
        }
    }
}
