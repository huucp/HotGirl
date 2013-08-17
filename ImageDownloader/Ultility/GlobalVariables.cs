using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ImageDownloader.Ultility
{
    public static class GlobalVariables
    {
        public static List<string> ListUrl = new List<string>();
        public static Dictionary<string, BitmapImage> ImageDict = new Dictionary<string, BitmapImage>();
        public static List<string> ListPath = new List<string>();
        public static Dictionary<string, BitmapImage> FavoriteDict = new Dictionary<string, BitmapImage>();
    }
}
