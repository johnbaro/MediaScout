using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MediaScout.Helpers
{
    public static class StorageHelper
    {
        private static String CacheDirectory = System.Environment.CurrentDirectory + @"\Cache\";

        public static DateTime DefaultCacheTime = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));

        public static String GetCacheDirectory(String Subdir)
        {
            return Path.Combine(CacheDirectory, Subdir);
        }
    }
}
