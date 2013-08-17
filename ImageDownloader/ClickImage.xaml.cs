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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageDownloader.ViewModel;
using Microsoft.Phone.Controls;

namespace ImageDownloader
{
    public partial class ClickImage : UserControl
    {
        public delegate void TapEventHandler(Uri sender);

        public event TapEventHandler Tapped;

        public void OnDownloadCompleted(Uri sender)
        {
            TapEventHandler handler = Tapped;
            if (handler != null) handler(sender);
        }

        ClickImageViewModel ViewModel = new ClickImageViewModel();
        public ClickImage()
        {
            InitializeComponent();
            DataContext = new ClickImageViewModel();
            ViewModel = (ClickImageViewModel)DataContext;


        }

        private Point center;
        private double initialScale;

        public void SetImage(BitmapImage image)
        {
            ViewModel.ImageSource = image;
        }

       
    }
}
