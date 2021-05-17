using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using XGENO.Framework.Enums;

namespace XGENO.Framework.Helpers
{
    public static class FileStorageHelper
    {
        private static async Task<Windows.Storage.StorageFile> GetIfFileExistsAsync(string key, Windows.Storage.StorageFolder folder)
        {
            Windows.Storage.StorageFile retval;

            try
            {
                retval = await folder.GetFileAsync(key);
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }

            return retval;
        }

        public static async Task<bool> FileExistsAsync(string key, Windows.Storage.StorageFolder folder)
        {
            return (await GetIfFileExistsAsync(key, folder)) != null;
        }



        public static async Task<Windows.Storage.StorageFile> GetIfFileExistsAsync(string key, StorageStrategy location = StorageStrategy.Local)
        {
            Windows.Storage.StorageFile retval;

            try
            {
                switch (location)
                {
                    case StorageStrategy.Local:
                        retval = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(key);
                        break;
                    case StorageStrategy.Roaming:
                        retval = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync(key);
                        break;
                    case StorageStrategy.Temporary:
                        retval = await Windows.Storage.ApplicationData.Current.TemporaryFolder.GetFileAsync(key);
                        break;
                    default:
                        throw new NotSupportedException(location.ToString());
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }

            return retval;
        }

        public static async Task<bool> FileExistsAsync(string key, StorageStrategy location = StorageStrategy.Local)
        {
            return (await GetIfFileExistsAsync(key, location)) != null;
        }
        


        public static async Task<Windows.Storage.StorageFile> CreateFileAsync(string key, StorageStrategy location = StorageStrategy.Local, Windows.Storage.CreationCollisionOption option = Windows.Storage.CreationCollisionOption.OpenIfExists)
        {
            switch (location)
            {
                case StorageStrategy.Local:
                    return await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(key, option);
                case StorageStrategy.Roaming:
                    return await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(key, option);
                case StorageStrategy.Temporary:
                    return await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(key, option);
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }
        


        public static async Task<bool> DeleteFileAsync(string key, StorageStrategy location = StorageStrategy.Local)
        {
            var _File = await GetIfFileExistsAsync(key, location);

            if (_File != null)
                await _File.DeleteAsync();

            return !(await FileExistsAsync(key, location));
        }

        public static async void DeleteFileFireAndForget(string key, StorageStrategy location)
        {
            await DeleteFileAsync(key, location);
        }


        
        public static async Task<T> ReadFileAsync<T>(string key, StorageStrategy location = StorageStrategy.Local)
        {
            try
            {
                // fetch file
                var _File = await GetIfFileExistsAsync(key, location);

                if (_File == null)
                    return default(T);

                // read content
                var _String = await Windows.Storage.FileIO.ReadTextAsync(_File);
                
                // convert to obj
                var _Result = SerializationHelper.Deserialize<T>(_String);
                
                return _Result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<string> ReadFileTextAsync(string key, StorageStrategy location = StorageStrategy.Local)
        {
            try
            {
                // fetch file
                var _File = await GetIfFileExistsAsync(key, location);

                if (_File == null)
                    return "";

                // read content
                var _String = await Windows.Storage.FileIO.ReadTextAsync(_File);
                
                return _String;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public static async Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategy location = StorageStrategy.Local)
        {
            // create file
            var _File = await CreateFileAsync(key, location, Windows.Storage.CreationCollisionOption.ReplaceExisting);

            // convert to string
            var _String = SerializationHelper.Serialize<T>(value);
            
            // save string to file
            await Windows.Storage.FileIO.WriteTextAsync(_File, _String);
            
            // result
            return await FileExistsAsync(key, location);
        }

        public static async void WriteFileFireAndForget<T>(string key, T value, StorageStrategy location = StorageStrategy.Local)
        {
            await WriteFileAsync(key, value, location);
        }


    }


}
