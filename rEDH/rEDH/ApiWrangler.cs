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

        ProductInfoHeaderValue appInfo = new ProductInfoHeaderValue("rEDH", "0.3");
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

        public async Task<ApiList> queryCards(bool[] colorsToSearch)
        {
            /// <summary>
            ///api.scryfall.com/cards/search + ? order=name + & + q=
            ///</summary>
            string requestString = searchURL + "?" + orderToken + andToken + queryToken;

            bool multipleColors = false;

            //for each color checked by the user
            for(int i = 0; i < colorsToSearch.Length; i++)
            {
                //add a + inbetween colors in the query string
                if(multipleColors && colorsToSearch[i])
                {
                    requestString += "+";
                }


                if (colorsToSearch[i])
                {
                    requestString += "c%3A";

                    switch(i)
                    {
                        case 0:
                            requestString += "white";
                            multipleColors = true;
                            break;
                        case 1:
                            requestString += "blue";
                            multipleColors = true;
                            break;
                        case 2:
                            requestString += "black";
                            multipleColors = true;
                            break;
                        case 3:
                            requestString += "red";
                            multipleColors = true;
                            break;
                        case 4:
                            requestString += "green";
                            multipleColors = true;
                            break;
                    }
                }
            }
            
            //final query string
            System.Diagnostics.Debug.WriteLine(requestString);

            var request = new HttpRequestMessage(HttpMethod.Get, requestString);
            var response = await httpClient.SendAsync(request);

            if(response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                this.apiList = JsonConvert.DeserializeObject<ApiList>(jsonString);

                return this.apiList;
            }
            else
            {
                string error = response.StatusCode.ToString() + " " + response.ReasonPhrase.ToString();
                return null;
            }
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
                var jsonString = await response.Content.ReadAsStringAsync();
                BulkList bulkList = JsonConvert.DeserializeObject<BulkList>(jsonString);

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
                    return null;
                }
            }
            else
            {
                string error = response.StatusCode.ToString();
                return null;
            }
        }
        //Testing purposes only
        public async Task<Card> testQuery()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.scryfall.com/cards/random");
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Card newCard = JsonConvert.DeserializeObject<Card>(jsonString);
                
                return newCard;
                //demoDownloadCardArt(newCard, response);
                
            }
            else
            {
                string error = response.StatusCode.ToString() + " " + response.ReasonPhrase.ToString();
                Console.WriteLine(error);

                return null;
            }
        }
        //Testing purposes only
        public async void demoDownloadCardArt(Card card, HttpResponseMessage response)
        {
            //create a temporary URI from the string that represents the link to the card image
            var tempUri = new Uri(card.image_uris.normal);

            //lop off the unnecessary parts
            var uriWithoutQuery = tempUri.GetLeftPart(UriPartial.Path);
            var fileExtension = Path.GetExtension(uriWithoutQuery);

            //produce a directory path & file name for the image
            string fileName = "newCard";
            string directoryPath = "C:\\Programming_Practice\\RandomEDHDeck\\RandomEDHDeck\\rEDH\\rEDH";
            var path = Path.Combine(directoryPath, $"{fileName}{fileExtension}");
            //card.image_uris.setLocalAddress(path);

            var imageBytes = await httpClient.GetByteArrayAsync(tempUri);
            //await File.WriteAllBytesAsync(card.image_uris.getLocalAddress(), imageBytes);

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
