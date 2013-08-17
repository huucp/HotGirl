using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace ImageDownloader.Ultility
{
    public class ImageDownload
    {
        private static byte[] key = Convert.FromBase64String("cvkalIvWO8IeFKdPmpA07A==");
        private static byte[] iv = Convert.FromBase64String("cvkalIvWO8IeFKdPmpA07A==");

        public delegate void SuccessfullyEventHandler(BitmapImage sender);

        public event SuccessfullyEventHandler DownloadCompleted;

        public void OnDownloadCompleted(BitmapImage sender)
        {
            SuccessfullyEventHandler handler = DownloadCompleted;
            if (handler != null) handler(sender);
        }

        public delegate void ErrorEventHandler(object sender, string msg);

        public event ErrorEventHandler DownloadFailed;

        public void OnDownloadFailed(object sender, string msg)
        {
            ErrorEventHandler handler = DownloadFailed;
            if (handler != null) handler(sender, msg);
        }

        private string ImageUrl { get; set; }

        public ImageDownload(string url)
        {
            ImageUrl = url;
        }
        public void Process()
        {
            string filename = GenerateNameFromUrl(ImageUrl, ".jpg");
            DownloadRemoteImageFile(ImageUrl, filename);
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string GenerateNameFromUrl(string url,string extension)
        {
            return AESEcryption(url) + extension;
        }

        private static string AESEcryption(string imageUrl)
        {
            //using (SHA256 mySHA256 = new SHA256Managed())
            //{
            //    byte[] sha256Bytes = GetBytes(imageUrl);
            //    byte[] cryByte = mySHA256.ComputeHash(sha256Bytes);
            //    return GetString(cryByte);
            //}

            using (var ALG = new AesManaged())
            {
                // Defaults
                // CipherMode = CBC
                // Padding = PKCS7
                byte[] data = GetBytes(imageUrl);
                ALG.KeySize = 128;
                ALG.BlockSize = 128;
                ALG.Key = key;
                ALG.IV = iv;

                ICryptoTransform encryptor = ALG.CreateEncryptor();
                byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
                return GetString(encrypted);
            }

        }
        

        private string FindImageInStorage(string filename)
        {
            var fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
            return fileStorage.FileExists(filename) ? filename : null;            
        }

        private BitmapImage LoadImageInStorage(string cachedImagePath, string url)
        {
            var fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (cachedImagePath != null)
            {
                using (var fileStream = new IsolatedStorageFileStream(cachedImagePath, FileMode.Open, fileStorage))
                {

                    var bitmapImage = new BitmapImage();
                    try
                    {
                        bitmapImage.SetSource(fileStream);
                        GlobalVariables.ImageDict.Add(url, bitmapImage);
                        return bitmapImage;  
                    }
                    catch (Exception e)
                    {
                        fileStorage.DeleteFile(cachedImagePath);
                        return null;
                    }                                                             
                }
            }
            return null;
        }

        private void DownloadImageFromUrl(string url, string filename)
        {
            Uri urlUri;
            try
            {
                urlUri = new Uri(url);
            }
            catch (Exception e)
            {
                OnDownloadFailed(null, e.Message);
                return;
            }
            var client = new WebClient();

            client.OpenReadCompleted +=
                delegate(object o, OpenReadCompletedEventArgs args)
                {
                    if (args.Error != null || args.Cancelled)
                    {
                        if (args.Error != null) OnDownloadFailed(null,args.Error.Message);
                        else OnDownloadFailed(null,"Cancel download");
                        return;
                    }
                    try
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var bi = new BitmapImage();
                            bi.SetSource(args.Result);
                            GlobalVariables.ImageDict.Add(url,bi);                            
                            OnDownloadCompleted(bi);
                            WriteImageToLocal(filename,bi);
                        });

                    }
                    catch (Exception e)
                    {
                        OnDownloadFailed(e,e.Message);
                        throw;
                    }

                };
            client.OpenReadAsync(urlUri);
        }

        private void WriteImageToLocal(string path, BitmapImage image)
        {
            // Create virtual store and file stream. Check for duplicate tempJPEG files.
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (myIsolatedStorage.FileExists(path))
                {
                    return;
                }

                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile(path);

                //StreamResourceInfo sri = null;
                //var uri = new Uri(path, UriKind.Relative);
                //sri = Application.GetResourceStream(uri);

                //var bitmap = new BitmapImage();
                //bitmap.SetSource(sri.Stream);
                var wb = new WriteableBitmap(image);

                // Encode WriteableBitmap object to a JPEG stream.
                wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);

                fileStream.Close();
            }  
        }

        private void DownloadRemoteImageFile(string url, string filename)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                OnDownloadFailed(null, "URL invalid");
                return;
            }
            DownloadImageFromUrl(url, filename);

        }

        public BitmapImage FindInCacheOrLocal()
        {
            if (GlobalVariables.ImageDict.ContainsKey(ImageUrl))
            {
                var image = GlobalVariables.ImageDict[ImageUrl];                
                return image;
            }
            string filename = AESEcryption(ImageUrl);
            var cachedImagePath = FindImageInStorage(filename + ".jpg");
            return LoadImageInStorage(cachedImagePath, ImageUrl);
           // return null;
        }
    }

    public class ImageDownloadWorker
    {
        //public static BlockingQueue<ImageDownload> ListsJobs = new BlockingQueue<ImageDownload>();

        //private static readonly Lazy<ImageDownloadWorker> Lazy = new Lazy<ImageDownloadWorker>(() => new ImageDownloadWorker());

        //public static ImageDownloadWorker Instance { get { return Lazy.Value; } }

        private Thread backgroundWorker;

        private BlockingQueue<ImageDownload> ListsJobs = new BlockingQueue<ImageDownload>();

        public ImageDownloadWorker()
        {
            backgroundWorker = new Thread(MainProcess)
            {
                IsBackground = true,
                Name = "Image Download Worker",
            };

            backgroundWorker.Start();
        }

        public void AddDownload(ImageDownload request)
        {
            ListsJobs.Add(request);
        }

        public void ClearAll()
        {
            ListsJobs.ClearAll();
        }

        private void MainProcess()
        {
            while (true)
            {
                var currentJob = ListsJobs.Get();
                if (currentJob == null) continue;
                currentJob.Process();
            }
        }
    }

    public interface IRequest
    {
        object Process();
    }

    public class BlockingQueue<T>
    {
        private const int MaxWaitingCount = 1;
        private Queue<T> q = new Queue<T>();
        public bool ClearQueue { get; set; }

        public BlockingQueue(bool clearQueue = false)
        {
            ClearQueue = clearQueue;
        }

        public void Add(T element)
        {
            lock (q)
            {
                if (q.Count > MaxWaitingCount && ClearQueue)
                {
                    Console.WriteLine("clear queue");
                    q.Clear();
                }
                q.Enqueue(element);
                Monitor.PulseAll(q);
            }
        }

        public T Get()
        {
            lock (q)
            {
                while (IsEmpty())
                {
                    Monitor.Wait(q);
                }
                return q.Dequeue();
            }
        }

        private bool IsEmpty()
        {
            lock (q)
            {
                return q.Count == 0;
            }
        }

        public void ClearAll()
        {
            lock (q)
            {
                q.Clear();
            }
        }
    }

    public class BlockingStack<T>
    {
        private const int MaxWaitingCount = 1;
        private Stack<T> stack = new Stack<T>();
        public bool ClearStack { get; set; }

        public BlockingStack(bool clearStack = false)
        {
            ClearStack = clearStack;
        }

        public void Add(T element)
        {
            lock (stack)
            {
                if (stack.Count > MaxWaitingCount && ClearStack)
                {
                    Console.WriteLine("clear stack");
                    stack.Clear();
                }
                stack.Push(element);
                Monitor.PulseAll(stack);
            }
        }

        public T Get()
        {
            lock (stack)
            {
                while (IsEmpty())
                {
                    Monitor.Wait(stack);
                }
                return stack.Pop();
            }
        }

        public bool IsEmpty()
        {
            lock (stack)
            {
                return stack.Count == 0;
            }
        }
    }
}
