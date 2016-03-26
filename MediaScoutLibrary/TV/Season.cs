using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Net;
namespace MediaScout
{
    public class Season
    {
        public SortedList<Int32, Episode> Episodes = new SortedList<int, Episode>();
        public Int32 ID;
        public String TVShowID;

        public Season(Int32 seasonNumber, String TVShowID)
        {
            ID = seasonNumber;
            this.TVShowID = TVShowID;
        }

        public Image poster;
        public Image banner;
        public String PosterUrl;
        public String tvid; //FreQi - Work item #2394

        public void FetchPoster()
        {
            try
            {
                // Create the requests.
                WebRequest requestPic = WebRequest.Create(PosterUrl);
                WebResponse responsePic = requestPic.GetResponse();
                poster = System.Drawing.Image.FromStream(responsePic.GetResponseStream());
            }
            catch
            {

            }
        }
    }
}
