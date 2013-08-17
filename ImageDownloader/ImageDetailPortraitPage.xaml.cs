using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ImageDownloader.Ultility;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using GestureEventArgs = Microsoft.Phone.Controls.GestureEventArgs;

namespace ImageDownloader
{
    public partial class ImageDetailPortraitPage : PhoneApplicationPage
    {
        public ImageDetailPortraitPage()
        {
            InitializeComponent();            
        }

        private void FlipRight()
        {
            if (path!=null)
            {
                int indexPath = GlobalVariables.ListPath.IndexOf(path);
                if (indexPath == GlobalVariables.ListPath.Count - 1) indexPath = -1;
                string nextPath = GlobalVariables.ListPath[indexPath + 1];
                var uriPath = new Uri("/ImageDetailPortraitPage.xaml?path=" + nextPath, UriKind.Relative);
                NavigationService.Navigate(uriPath);
                return;
            }

            int index = GlobalVariables.ListUrl.IndexOf(url);
            if (index == GlobalVariables.ListUrl.Count - 1) index = -1;
            string nextUrl = GlobalVariables.ListUrl[index + 1];
            var uri = new Uri("/ImageDetailPortraitPage.xaml?url=" + nextUrl, UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void FlipLeft()
        {
            if (path != null)
            {
                int indexPath = GlobalVariables.ListPath.IndexOf(path);
                if (indexPath == 0) indexPath = GlobalVariables.ListPath.Count;
                string nextPath = GlobalVariables.ListPath[indexPath - 1];
                var uriPath = new Uri("/ImageDetailPortraitPage.xaml?path=" + nextPath, UriKind.Relative);
                NavigationService.Navigate(uriPath);
                return;
            }
            int index = GlobalVariables.ListUrl.IndexOf(url);
            if (index == 0) index = GlobalVariables.ListUrl.Count;
            string nextUrl = GlobalVariables.ListUrl[index - 1];
            var uri = new Uri("/ImageDetailPortraitPage.xaml?url=" + nextUrl, UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private string url;
        private string path;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string msg;
            if (NavigationContext.QueryString.TryGetValue("url", out msg))
            {
                url = msg;
                DownloadImage(url);
                DownloadBesideImage(url);
            }
            if (NavigationContext.QueryString.TryGetValue("path", out msg))
            {
                path = msg;
                LoadFavoriteImage();
            }
        }

        private void LoadFavoriteImage()
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            using (var fileStream = new IsolatedStorageFileStream(path, FileMode.Open, isoFile))
            {

                var bitmapImage = new BitmapImage();
                try
                {
                    bitmapImage.SetSource(fileStream);
                    DetailImage.Source = bitmapImage;
                }
                catch (Exception e)
                {
                    //isoFile.DeleteFile(path);
                    return;
                }
            }
        }

        private void DownloadBesideImage(string url)
        {
            int index = GlobalVariables.ListUrl.LastIndexOf(url);
            DownloadFutureImage(index == GlobalVariables.ListUrl.Count - 1
                                    ? GlobalVariables.ListUrl[0]
                                    : GlobalVariables.ListUrl[index + 1]);
            DownloadFutureImage(index == 0
                                    ? GlobalVariables.ListUrl[GlobalVariables.ListUrl.Count - 1]
                                    : GlobalVariables.ListUrl[index - 1]);
        }

        private void DownloadFutureImage(string url)
        {
            var download = new ImageDownload(url);
            var cacheImage = download.FindInCacheOrLocal();
            if (cacheImage != null)
            {
                return;
            }
            App.ImageWorker.AddDownload(download);
        }

        private void DownloadImage(string url)
        {
            var download = new ImageDownload(url);
            var cacheImage = download.FindInCacheOrLocal();
            if (cacheImage != null)
            {
                DetailImage.Source = cacheImage;
                LoadingProgressBar.Visibility = Visibility.Collapsed;
                return;
            }
            download.DownloadCompleted += (image) =>
                                              {
                                                  LoadingProgressBar.Visibility = Visibility.Collapsed;
                                                  DetailImage.Source = image;
                                              };
            App.ImageWorker.AddDownload(download);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {            
            base.OnBackKeyPress(e);

            if (path != null)
            {
                var uriPath = new Uri("/FavoritePage.xaml", UriKind.Relative);
                NavigationService.Navigate(uriPath);
                return;
            }
            var uri = new Uri("/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private Point firstPos;

        private Point center;
        private double initialScale;

        private void GestureListener_PinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            // store the initial rotation angle and scaling
            initialScale = ImageTransformation.ScaleX;
            // calculate the center for the zooming
            Point firstTouch = e.GetPosition(DetailImage, 0);
            Point secondTouch = e.GetPosition(DetailImage, 1);

            double centerX = firstTouch.X + (secondTouch.X - firstTouch.X) / 2.0;
            double centerY = firstTouch.Y + (secondTouch.Y - firstTouch.Y) / 2.0;

            center = new Point(centerX, centerY);

            _oldFinger1 = e.GetPosition(DetailImage, 0);
            _oldFinger2 = e.GetPosition(DetailImage, 1);
            _oldScaleFactor = 1;
        }

        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            // if its less that the original  size or more than 4x then does not apply
            if (initialScale * e.DistanceRatio > 4 ||
                (Math.Abs(initialScale - 1) > 0.01 && Math.Abs(e.DistanceRatio - 1) < 0.01) ||
                initialScale * e.DistanceRatio < 1)
                return;

            // if its original size then center it back
            if (e.DistanceRatio <= 1.08)
            {
                ImageTransformation.CenterY = 0;
                ImageTransformation.CenterY = 0;
                ImageTransformation.TranslateX = 0;
                ImageTransformation.TranslateY = 0;
            }

            var scaleFactor = e.DistanceRatio / _oldScaleFactor;

            var currentFinger1 = e.GetPosition(DetailImage, 0);
            var currentFinger2 = e.GetPosition(DetailImage, 1);

            var translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                ImagePosition,
                scaleFactor);

            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = e.DistanceRatio;

            UpdateImage(scaleFactor, translationDelta);

            ImageTransformation.CenterX = center.X;
            ImageTransformation.CenterY = center.Y;

            //// update the rotation and scaling
            //if (Orientation == PageOrientation.Landscape)
            //{
            //    // when in landscape we need to zoom faster, if not it looks choppy
            //    ImageTransformation.ScaleX = initialScale * (1 + (e.DistanceRatio - 1) * 2);
            //}
            //else
            //{
            //    ImageTransformation.ScaleX = initialScale * e.DistanceRatio;
            //}
            //ImageTransformation.ScaleY = ImageTransformation.ScaleX;
        }




        private void Image_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            // if is not touch enabled or the scale is different than 1 then doest not allow moving
            if (ImageTransformation.ScaleX <= 1.1)
                return;
            double centerX = ImageTransformation.CenterX;
            double centerY = ImageTransformation.CenterY;
            double translateX = ImageTransformation.TranslateX;
            double translateY = ImageTransformation.TranslateY;
            double scale = ImageTransformation.ScaleX;
            double width = DetailImage.ActualWidth;
            double height = DetailImage.ActualHeight;

            // verify limits to not allow the image to get out of area

            if (centerX - scale * centerX + translateX + e.HorizontalChange < 0 &&
            centerX + scale * (width - centerX) + translateX + e.HorizontalChange > width)
            {
                ImageTransformation.TranslateX += e.HorizontalChange;
            }

            if (centerY - scale * centerY + translateY + e.VerticalChange < 0 &&
            centerY + scale * (height - centerY) + translateY + e.VerticalChange > height)
            {
                ImageTransformation.TranslateY += e.VerticalChange;
            }
        }

        private void GestureListener_PinchCompleted(object sender, PinchGestureEventArgs e)
        {
        }

        // these two fully define the zoom state:
        private double TotalImageScale = 1d;
        private Point ImagePosition = new Point(0, 0);

        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;

        private void UpdateImage(double scaleFactor, Point delta)
        {
            TotalImageScale *= scaleFactor;
            if (TotalImageScale > 4) return;
            //ImagePosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);

            ImageTransformation.ScaleX = TotalImageScale;
            ImageTransformation.ScaleY = TotalImageScale;
            // ImageTransformation.TranslateX = ImagePosition.X;
            //ImageTransformation.TranslateY = ImagePosition.Y;
        }

        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            var newPos1 = new Point(
                currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

            var newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

            var newPos = new Point(
                (newPos1.X + newPos2.X) / 2,
                (newPos1.Y + newPos2.Y) / 2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }

        private void GestureListener_OnDoubleTap(object sender, GestureEventArgs e)
        {
            ImageTransformation.ScaleX = 1;
            ImageTransformation.ScaleY = 1;
            ImageTransformation.CenterY = 0;
            ImageTransformation.CenterY = 0;
            ImageTransformation.TranslateX = 0;
            ImageTransformation.TranslateY = 0;
        }

        private void GestureListener_DragStarted(object sender, DragStartedGestureEventArgs e)
        {
            firstPos = e.GetPosition(LayoutRoot);
        }

        private void GestureListener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            if ((ImageTransformation.ScaleX - 1.08) > 0 || (ImageTransformation.ScaleY - 1.08) > 0) return;
            var distance = firstPos.X - e.GetPosition(LayoutRoot).X;
            if (distance < -100)
            {
                FlipLeft();
            }
            else if (distance > 100)
            {
                FlipRight();
            }
        }

        private void SaveButton_OnClick(object sender, EventArgs e)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var download = new ImageDownload(url);
                var cacheImage = download.FindInCacheOrLocal();
                if (cacheImage == null)
                {
                    MessageBox.Show("Cannot save image!!!");
                    return;
                }
                var path = ImageDownload.GenerateNameFromUrl(url, ".jpg");
                if (!myIsolatedStorage.DirectoryExists("Favorite")) myIsolatedStorage.CreateDirectory("Favorite");
                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("Favorite\\" + path);

                var wb = new WriteableBitmap(cacheImage);

                wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                fileStream.Close();
            }
        }
    }
}