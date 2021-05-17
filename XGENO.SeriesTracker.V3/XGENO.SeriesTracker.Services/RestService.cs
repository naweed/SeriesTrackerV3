using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using XGENO.Framework.Exceptions;
using XGENO.Framework.Extensions;
using XGENO.Framework.Helpers;
using XGENO.Framework.Services;
using XGENO.SeriesTracker.DataModels;

namespace XGENO.SeriesTracker.Services
{
    public class RestService
    {
        private RestClientHelper _restHelper;

        private string sourceURI = @"https://api-v2launch.trakt.tv/";

        public static List<JSONDataCache> CachedObjects;

        public RestService()
        {
            _restHelper = new RestClientHelper();

            AddDefaultHeaders();
        }


        public RestService(bool isOAuth, AppSettings _appSettings = null)
        {
            _restHelper = new RestClientHelper();

            if (!isOAuth)
            {
                _restHelper.AddHttpHeader("Authorization", "Bearer " + _appSettings.TraktAccessToken);
                AddDefaultHeaders();
            }
        }

        private void AddDefaultHeaders()
        {
            _restHelper.AddHttpHeader("trakt-api-version", "2");
            _restHelper.AddHttpHeader("trakt-api-key", AppSettings.TraktSyncClientID);
        }

        public async Task<T> GetAsync<T>(string resource, string parameters = "", int cacheDuration = 24)
        {
            JSONDataCache cachedData = null;

            //First check if Local Cache List is available - If not, Load
            if (cacheDuration > 0 && CachedObjects == null)
            {
                CachedObjects = await SQLiteService.Instance.GetAllCachedObjects();
            }

            //Construct the URL
            string uri = sourceURI + resource;

            if (!string.IsNullOrEmpty(parameters))
                uri += "?" + parameters;

            string cleanCacheKey = uri.CleanCacheKey();

            //Check for Cache only if Cached data can be used
            if (cacheDuration > 0)
            {
                if (CachedObjects.Where(_c => _c.Key == cleanCacheKey).Any())
                    cachedData = CachedObjects.Where(_c => _c.Key == cleanCacheKey).First();

                //If Cached Data exists
                if (cachedData != null)
                {
                    //If the cached data is still valid
                    if (cachedData.StartDate.AddHours(cacheDuration) > DateTime.Now)
                        return SerializationHelper.Deserialize<T>(cachedData.Content);
                }
            }

            //Check for internet connection and return cached data if possible
            var networkAvailableService = new NetworkAvailableService();
            bool IsInternetAvailable = await networkAvailableService.IsInternetAvailable();

            if (!IsInternetAvailable)
            {
                if (cachedData != null)
                {
                    return SerializationHelper.Deserialize<T>(cachedData.Content);
                }
                else
                {
                    throw new InternetConnectionException();
                }
            }

            //No Cache Found, or Cached data was not required, or Internet connection is also available
            var json = await _restHelper.GetAsync(uri);

            //Save the data to Cache if required
            if (cacheDuration > 0)
            {
                try
                {
                    JSONDataCache cacheData = new JSONDataCache();

                    cacheData.Key = cleanCacheKey;
                    cacheData.StartDate = DateTime.Now;
                    cacheData.Content = json;

                    //Save Cached data into SQLite
                    await SQLiteService.Instance.SaveDataCache(cacheData);

                    //Save Cached data into local list
                    CachedObjects.RemoveAll(_c => _c.Key == cleanCacheKey);
                    CachedObjects.Add(cacheData);
                }
                catch (Exception ex)
                {
                }
            }
            
            T result = SerializationHelper.Deserialize<T>(json);

            return result;
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string resource, T payload)
        {
            string uri = sourceURI + resource;

            var result = await _restHelper.PutAsync<T>(uri, payload);

            return result;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string resource, T payload)
        {
            string uri = sourceURI + resource;

            var result = await _restHelper.PostAsync<T>(uri, payload);

            return result;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string resource)
        {
            string uri = sourceURI + resource;

            var result = await _restHelper.DeleteAsync(uri);

            return result;
        }

    }
}
