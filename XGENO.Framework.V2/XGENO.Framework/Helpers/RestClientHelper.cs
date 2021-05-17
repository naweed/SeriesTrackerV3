using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Newtonsoft.Json;
using Windows.Web.Http.Filters;
using XGENO.Framework.Common;
using XGENO.Framework.Extensions;
using XGENO.Framework.DataModels;
using HttpFilters;

namespace XGENO.Framework.Helpers
{
    public class WebApiException : Exception
    {
        public HttpStatusCode Status { get; set; }
    }

    public class RestClientHelper
    {
        private HttpClient httpClient;

        public RestClientHelper(bool isJson = true)
        {
            var filter = new HttpBaseProtocolFilter();
            filter.AutomaticDecompression = true;

            HttpRetryFilter retryFilter = new HttpRetryFilter(filter);

            httpClient = new HttpClient(retryFilter);

            if (isJson)
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            }
        }

        public void AddHttpHeader(string key, string value)
        {
            if (!httpClient.DefaultRequestHeaders.Keys.Contains(key))
                httpClient.DefaultRequestHeaders.Add(key, value);
            else
                httpClient.DefaultRequestHeaders[key] = value;
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string uri, T payload)
        {
            var dataToPost = SerializationHelper.Serialize<T>(payload);
            var content = new HttpStringContent(dataToPost, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            HttpResponseMessage response = await httpClient.PutAsync(new Uri(uri), content);

            response.EnsureSuccessStatusCode();

            //if (response.StatusCode != HttpStatusCode.Ok && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NoContent)
            //    throw new WebApiException { Status = response.StatusCode };

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string uri, T payload)
        {
            var dataToPost = SerializationHelper.Serialize<T>(payload);
            var content = new HttpStringContent(dataToPost, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(new Uri(uri), content);

            response.EnsureSuccessStatusCode();

            //if (response.StatusCode != HttpStatusCode.Ok && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NoContent)
            //    throw new WebApiException { Status = response.StatusCode };

            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string uri)
        {

            HttpResponseMessage response = await httpClient.DeleteAsync(new Uri(uri));

            response.EnsureSuccessStatusCode();

            //if (response.StatusCode != HttpStatusCode.Ok && response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.NoContent)
            //    throw new WebApiException { Status = response.StatusCode };

            return response;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            //Read Response
            string json = await GetAsync(uri);

            //Return the result
            T result = SerializationHelper.Deserialize<T>(json);

            return result;
        }

        public async Task<string> GetAsync(string uri)
        {
            int questionMarkIndex = uri.IndexOf("?", StringComparison.Ordinal);

            if (questionMarkIndex == -1)
            {
                uri += "?retryAfter=httpDate";
                //deltaSeconds
            }
            else
            {
                uri += "&retryAfter=httpDate";
            }

            //Extract response from URI
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));

            response.EnsureSuccessStatusCode();

            //if (response.StatusCode != HttpStatusCode.Ok)
            //    throw new WebApiException { Status = response.StatusCode };

            //Read Response
            string json = await response.Content.ReadAsStringAsync();

            return json;
        }





    }
}


