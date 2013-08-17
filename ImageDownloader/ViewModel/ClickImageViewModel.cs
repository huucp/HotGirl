using System.Windows.Media.Imaging;

namespace ImageDownloader.ViewModel
{
    public class ClickImageViewModel:ViewModelBase
    {
        private BitmapImage imageSource = null;
        public BitmapImage ImageSource
        {
            get { return imageSource; }
            set
            {
                if (imageSource == value) return;
                imageSource = value;
                NotifyPropertyChanged("ImageSource");
            }
        }
    }
}
