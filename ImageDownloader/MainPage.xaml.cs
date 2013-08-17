using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ImageDownloader.Ultility;
using Microsoft.Phone.Controls;
using ImageDownloader.ViewModel;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ImageDownloader
{
    public partial class MainPage : PhoneApplicationPage
    {
        private MainPageViewModel ViewModel = new MainPageViewModel();
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            DataContext = new MainPageViewModel();
            ViewModel = (MainPageViewModel)DataContext;



        }

        private int count = 0;
        private double leftHeight = 0;
        private double rightHeight = 0;



        private void DownloadImage(string url)
        {
            url = HttpUtility.UrlDecode(url);
            GlobalVariables.ListUrl.Add(url);
            var download = new ImageDownload(url);
            var cacheImage = download.FindInCacheOrLocal();
            if (cacheImage != null)
            {
                var cImage = new ClickImage();
                cImage.SetImage(cacheImage);
                cImage.Tap += delegate(object o, GestureEventArgs args)
                {
                    var uri = new Uri("/ImageDetailPortraitPage.xaml?url=" + url, UriKind.Relative);
                    NavigationService.Navigate(uri);
                };                
                if (leftHeight > rightHeight)
                {
                    RightColumn.Children.Add(cImage);
                    rightHeight += CalculateImageActualHeight(cacheImage.PixelWidth, cacheImage.PixelHeight);
                    
                }
                else
                {
                    LeftColumn.Children.Add(cImage);
                    leftHeight += CalculateImageActualHeight(cacheImage.PixelWidth, cacheImage.PixelHeight);
                }
                return;
            }
            download.DownloadCompleted += (image) =>
            {
                var cImage = new ClickImage();
                cImage.SetImage(image);
                cImage.Tap += delegate(object o, GestureEventArgs args)
                {
                    var uri = new Uri("/ImageDetailPortraitPage.xaml?url=" + url, UriKind.Relative);
                    NavigationService.Navigate(uri);
                };                
                if (leftHeight >rightHeight)
                {
                    RightColumn.Children.Add(cImage);
                    rightHeight += CalculateImageActualHeight(image.PixelWidth, image.PixelHeight);
                    
                }
                else
                {
                    LeftColumn.Children.Add(cImage);
                    leftHeight += CalculateImageActualHeight(image.PixelWidth, image.PixelHeight);
                }
            };
            download.DownloadFailed += (o, s) => Debug.WriteLine(s);
            App.ImageWorker.AddDownload(download);

        }

        private double CalculateImageActualHeight(double width, double height)
        {
            return LeftColumn.ActualWidth / width * height;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (NavigationService.CanGoBack)
            {
                while (NavigationService.RemoveBackEntry() != null)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            DownloadImage("http://2.bp.blogspot.com/-D4XBJLXnNdg/UcqNcEYFY1I/AAAAAAAAAnQ/h47aV1TASN4/s1600/hot-girl-midu-4.jpg");
            DownloadImage("http://true-x.net/upload/images/sexy_girl_1.jpg");
            DownloadImage("http://mebeats.com/cheff/files/2012/12/sexygirl.jpg");
            DownloadImage("http://imgs.mi9.com/uploads/female-celebrities/4700/lisha-cuthbert-sexy-girl-wallpaper_422_84355.jpg");
            DownloadImage("http://files.myopera.com/Trynity34/albums/11355962/sexy girl wallpaper -thewallpaperdb.blogspot.com- - (27).jpg");
            DownloadImage("http://imgs.mi9.com/uploads/female-celebrities/4700/lisha-cuthbert-sexy-girl-wallpaper_422_84356.jpg");
            DownloadImage("http://s3.amazonaws.com/rapgenius/filepicker%2FZL6bLsRvSianquoRDuFA_sexy_girl.jpg");
            
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/15-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/16-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/17-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/18-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/19-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/21-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/22-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/23-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/24-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/25-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/26-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/27-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/28-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/29-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/thumb_w/600/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/1-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/10-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/12-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/13-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/2-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/3-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/4-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/5-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/6-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/7-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/8-4aedf.jpg");
            DownloadImage("http://genk2.vcmedia.vn/DlBlzccccccccccccE5CT3hqq3xN9o/Image/2013/08/9-4aedf.jpg");
        }

        private void ApplicationBarMenuItem_OnClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FavoritePage.xaml", UriKind.Relative));
        }
    }
}