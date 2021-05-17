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
    public class TMDBRestService
    {
        private RestClientHelper _restHelper;

        private string sourceURI = @"https://api.themoviedb.org/3/";

        private readonly string _tmdbAPIKey = @"f3fd9203f4cf09be83fb7e3d1dc2198c";


        public TMDBRestService()
        {
            _restHelper = new RestClientHelper();
        }

        public async Task<T> GetAsync<T>(string resource, int cacheDuration = 1440)
        {
            JSONDataCache cachedData = null;

            //First check if Local Cache List is available - If not, Load
            if (cacheDuration > 0 && RestService.CachedObjects == null)
            {
                RestService.CachedObjects = await SQLiteService.Instance.GetAllCachedObjects();
            }

            //Construct the URL
            string uri = sourceURI + resource + "?api_key=" + _tmdbAPIKey;

            string cleanCacheKey = uri.CleanCacheKey();

            //Check for Cache only if Cached data can be used
            if (cacheDuration > 0)
            {
                if (RestService.CachedObjects.Where(_c => _c.Key == cleanCacheKey).Any())
                    cachedData = RestService.CachedObjects.Where(_c => _c.Key == cleanCacheKey).First();

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
                    RestService.CachedObjects.RemoveAll(_c => _c.Key == cleanCacheKey);
                    RestService.CachedObjects.Add(cacheData);
                }
                catch (Exception ex)
                {
                }
            }
            
            T result = SerializationHelper.Deserialize<T>(json);

            return result;
        }

        //response = HTTParty.get("https://api.themoviedb.org/3/configuration?api_key=#{TMDB_API_KEY}")
        //config = JSON.parse(response.body)
        //image_prefix = config['images']['secure_base_url']

        //response = HTTParty.get("https://api.themoviedb.org/3/person/#{tmdb_id}/images?api_key=#{TMDB_API_KEY}")
        //images = JSON.parse(response.body)
        //poster = image_prefix + 'w185' + images['profiles'].first['file_path']
        //http://image.tmdb.org/t/p/w185/3EdsBXcwZFX2Txjl4uLhLgTU8SK.jpg
        //https://image.tmdb.org/t/p/
    }
}
