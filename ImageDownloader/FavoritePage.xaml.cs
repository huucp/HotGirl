using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ImageDownloader.Ultility;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace ImageDownloader
{
    public partial class FavoritePage : PhoneApplicationPage
    {
        public FavoritePage()
        {
            InitializeComponent();
        }

        private void FavoritePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists("Favorite"))
                {
                    MessageBox.Show("No image in favorite");
                    return;
                }
                string searchPath = Path.Combine("Favorite", "*.*");
                var listImage = myIsolatedStorage.GetFileNames(searchPath);
                foreach (var imagePath in listImage)
                {
                    var bi = new BitmapImage();
                    using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        bi.SetSource(fileStream);
                        AddImage(bi, imagePath);
                    }
                }
            }
        }

        private double leftHeight = 0;
        private double rightHeight = 0;

        private void AddImage(BitmapImage image, string path)
        {
            var cImage = new ClickImage();
            cImage.SetImage(image);
            cImage.Tap += delegate(object o, GestureEventArgs args)
            {
                var uri = new Uri("/ImageDetailPortraitPage.xaml?path=" + path, UriKind.Relative);
                NavigationService.Navigate(uri);
            };
            if (leftHeight > rightHeight)
            {
                RightColumn.Children.Add(cImage);
                rightHeight += CalculateImageActualHeight(image.PixelWidth, image.PixelHeight);

            }
            else
            {
                LeftColumn.Children.Add(cImage);
                leftHeight += CalculateImageActualHeight(image.PixelWidth, image.PixelHeight);
            }
            GlobalVariables.ListPath.Add(path);
        }
        private double CalculateImageActualHeight(double width, double height)
        {
            return LeftColumn.ActualWidth / width * height;
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            var uri = new Uri("/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}