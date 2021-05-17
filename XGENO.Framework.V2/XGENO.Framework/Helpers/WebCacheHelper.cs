using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using XGENO.Framework.Extensions;
using Windows.Storage;
using Windows.Web.Http;

namespace XGENO.Framework.Helpers
{
    public static class WebCacheHelper
    {
        private const string CacheFolderName = "ImageCache";
        private static readonly Dictionary<string, Task> _concurrentTasks = new Dictionary<string, Task>();
        private static readonly SemaphoreSlim _cacheFolderSemaphore = new SemaphoreSlim(1);
        private static StorageFolder _cacheFolder;
        public static TimeSpan CacheDuration { get; set; }


        static WebCacheHelper()
        {
            CacheDuration = TimeSpan.FromHours(24*60);
        }

        public static async Task ClearAsync(TimeSpan? duration = null)
        {
            duration = duration ?? TimeSpan.FromSeconds(0);
            DateTime expirationDate = DateTime.Now.Subtract(duration.Value);
            try
            {
                var folder = await GetCacheFolderAsync();

                foreach (var file in await folder.GetFilesAsync())
                {
                    try
                    {
                        if (file.DateCreated < expirationDate)
                        {
                            await file.DeleteAsync();
                        }
                    }
                    catch
                    {
                        // Just ignore errors for now
                    }
                }
            }
            catch
            {
                // Just ignore errors for now
            }
        }


        public static async Task<string> GetWebImage(string uri, Progress<HttpProgress> progressCallback = null)
        {
            if (uri.StartsWith("ms-appx:"))
                return uri;

            var theURI = new Uri(uri);

            string key = GetCacheFileName(theURI);
            Task busy;

            lock (_concurrentTasks)
            {
                if (_concurrentTasks.ContainsKey(key))
                {
                    busy = _concurrentTasks[key];
                }
                else
                {
                    busy = EnsureFileAsync(theURI, progressCallback);
                    _concurrentTasks.Add(key, busy);
                }
            }

            try
            {
                await busy;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                lock (_concurrentTasks)
                {
                    if (_concurrentTasks.ContainsKey(key))
                    {
                        _concurrentTasks.Remove(key);
                    }
                }
            }

            //return $"ms-appdata:///temp/{CacheFolderName}/{key}";
            return $"ms-appdata:///local/{CacheFolderName}/{key}";
        }

        private static string GetCacheFileName(Uri uri)
        {
            ulong uriHash = CreateHash64(uri);

            return $"{uriHash}.jpg";
        }

        private static ulong CreateHash64(Uri uri)
        {
            return CreateHash64(uri.Host + uri.PathAndQuery);
        }

        private static ulong CreateHash64(string str)
        {
            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(str);

            ulong value = (ulong)utf8.Length;
            for (int n = 0; n < utf8.Length; n++)
            {
                value += (ulong)utf8[n] << ((n * 5) % 56);
            }

            return value;
        }

        private static async Task<bool> IsFileOutOfDate(StorageFile file, DateTime expirationDate)
        {
            if (file != null)
            {
                var properties = await file.GetBasicPropertiesAsync();
                return properties.DateModified < expirationDate;
            }

            return true;
        }

        private static async Task<StorageFolder> GetCacheFolderAsync()
        {
            if (_cacheFolder == null)
            {
                await _cacheFolderSemaphore.WaitAsync();

                try
                {
                    //_cacheFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists);
                    _cacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(CacheFolderName, CreationCollisionOption.OpenIfExists);
                }
                catch
                {
                }
                finally
                {
                    _cacheFolderSemaphore.Release();
                }
            }

            return _cacheFolder;
        }
        
        private static async Task EnsureFileAsync(Uri uri, Progress<HttpProgress> progressCallback = null)
        {
            DateTime expirationDate = DateTime.Now.Subtract(CacheDuration);

            var folder = await GetCacheFolderAsync();

            string fileName = GetCacheFileName(uri);
            var baseFile = await folder.TryGetItemAsync(fileName) as StorageFile;
            if (await IsFileOutOfDate(baseFile, expirationDate))
            {
                baseFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                try
                {
                    await GetHttpStreamToStorageFileAsync(uri, baseFile, progressCallback);
                }
                catch
                {
                    await baseFile.DeleteAsync();
                    throw new Exception("URI Image not found");
                }
            }
        }

        public static async Task GetHttpStreamToStorageFileAsync(this Uri uri, StorageFile targetFile, Progress<HttpProgress> progressCallback = null)
        {
            var content = await GetHttpContentAsync(uri, progressCallback);

            using (content)
            {
                using (var fileStream = await targetFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await content.WriteToStreamAsync(fileStream);
                }
            }
        }

        private static async Task<IHttpContent> GetHttpContentAsync(Uri uri, Progress<HttpProgress> progressCallback = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException();
            }

            using (var httpClient = new HttpClient())
            {
                var requestMsg = new HttpRequestMessage(HttpMethod.Get, uri);

                if(progressCallback == null)
                    progressCallback = new Progress<HttpProgress>(OnSendRequestProgress);

                var tokenSource = new CancellationTokenSource();


                using (var response = await httpClient.SendRequestAsync(requestMsg).AsTask(tokenSource.Token, progressCallback))
                {
                    response.EnsureSuccessStatusCode();

                    return response.Content;
                }
            }
        }

        private static void OnSendRequestProgress(HttpProgress obj)
        {
            //this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            //    () => {
            //        btnBar.Minimum = 0;
            //        btnBar.Value = obj.BytesReceived;
            //        if (obj.TotalBytesToReceive != null)
            //            btnBar.Maximum = (double)obj.TotalBytesToReceive;
            //    });
        }
    }
}
