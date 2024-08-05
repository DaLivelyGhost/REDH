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

namespace rEDH
{
    public class ApiWrangler
    {
        //----Variables
        private HttpClient httpClient;

        public const string searchURL = "https://api.scryfall.com/cards/search";
        public const string randomURL = "https://api.scryfall.com/cards/random";

        ProductInfoHeaderValue appInfo = new ProductInfoHeaderValue("rEDH", "0.1");
        MediaTypeWithQualityHeaderValue acceptInfo = new MediaTypeWithQualityHeaderValue("application/json");
        
        
        //----Methods
        public async void testQuery(CardList cardList)
        {
            httpClient = new HttpClient();
          
            httpClient.DefaultRequestHeaders.UserAgent.Add(appInfo);
            httpClient.DefaultRequestHeaders.Accept.Add(acceptInfo);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.scryfall.com/cards/random");
            var response = await httpClient.SendAsync(request);


            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var newCard = JsonConvert.DeserializeObject<Card>(jsonString);

                cardList.addCard(newCard);
                demoDownloadCardArt(newCard, response);
                
            }
            else
            {
                String error = response.StatusCode.ToString() + " " + response.ReasonPhrase.ToString();
                Console.WriteLine(error);
            }
        }
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


            var imageBytes = await httpClient.GetByteArrayAsync(tempUri);
            await File.WriteAllBytesAsync(path, imageBytes);

        }

    }
}
