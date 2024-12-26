using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Text.Json.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace rEDH
{
    /// <summary>
    /// Object that handles API handshaking
    /// </summary>
    public class ApiWrangler
    {
        //----Variables
        private HttpClient httpClient;
        private ApiList apiList;

        private const string searchURL = "https://api.scryfall.com/cards/search";
        private const string bulkURL = "https://api.scryfall.com/bulk-data";
        private const string randomURL = "https://api.scryfall.com/cards/random";

        //----Search Tokens
        private const string queryToken = "q=";
        private const string uniqueToken = "unique";
        private const string orderToken = "order=name";
        private const string andToken = "&";

        ProductInfoHeaderValue appInfo = new ProductInfoHeaderValue("rEDH", "0.6");
        MediaTypeWithQualityHeaderValue acceptInfo = new MediaTypeWithQualityHeaderValue("application/json");
        
        public ApiWrangler()
        {
            //singleton object. Always refer to this instance.
            this.httpClient = new HttpClient();

            //bulk data is returned as a concatenated list. this object interperets that data.
            this.apiList = new ApiList();

            //http request headers. these ones are the same for all requests.
            httpClient.DefaultRequestHeaders.UserAgent.Add(appInfo);
            httpClient.DefaultRequestHeaders.Accept.Add(acceptInfo);
        }
        

        //----Methods
        public async Task<string> testConnection()
        {
            var request = new HttpRequestMessage(HttpMethod.Connect, randomURL);
            var response = await httpClient.SendAsync(request);

            if(response.IsSuccessStatusCode)
            {
                return null;
            }

            string error = response.StatusCode.ToString();
            throw new Exception(error);
        }
        public async Task<ApiList> QueryFromURI(string queryString)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, queryString);
            var response = await httpClient.SendAsync(request);

            if(response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                this.apiList = JsonConvert.DeserializeObject<ApiList>(jsonString);

                return this.apiList;
            }
            else
            {
                string error = response.StatusCode.ToString();
                return null;
            }
        }
        public async Task<byte[]> queryBulkData()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, bulkURL);
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                //the returned object is a list of bulk objects.
                var jsonString = await response.Content.ReadAsStringAsync();
                BulkList bulkList = JsonConvert.DeserializeObject<BulkList>(jsonString);

                //Now we query the URI from the 1st bulk object in the list as an array of bytes
                //to be written to file.
                request = new HttpRequestMessage(HttpMethod.Get, bulkList.data[0].download_uri);
                response = await httpClient.SendAsync(request);

                if(response.IsSuccessStatusCode)
                {
                    var jsonByteArray = await response.Content.ReadAsByteArrayAsync();
                    return jsonByteArray;
                }
                else
                {
                    string error = response.StatusCode.ToString();
                    throw new Exception(error);
                }
            }
            else
            {
                string error = response.StatusCode.ToString();
                return null;
            }
        }

    }
    /// <summary>
    /// Bulk data is returned from scryfall as a list of 'bulk data' objects.
    /// The most important piece of these objects is their URI that links to a JSON list of over 30k cards
    /// </summary>
    public class BulkList()
    {
        [JsonProperty]
        public apiBulk[] data {  get; set; }
    }
    public class apiBulk()
    {
        [JsonProperty]
        public string id { get; set; }
        [JsonProperty]
        public string updated_at { get; set; }
        [JsonProperty]
        public string uri { get; set; }
        [JsonProperty]
        public string name { get; set; }
        [JsonProperty]
        public string download_uri { get; set; }
    }

    /// <summary>
    /// //JSON data is returned in list format.
    /// This object represents that list locally to be parsed into cards.
    /// </summary>
    public class ApiList()
    {
        [JsonPropertyName("total_cards")]
        public int total_cards {  get; set; }
        public bool has_more {  get; set; }
        public string next_page { get; set; }
        public Card[] data { get; set; }
    }
}
