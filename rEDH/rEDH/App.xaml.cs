using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace rEDH
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            testQuery();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
  
        }
        public async void testQuery()
        {
            httpClient = new HttpClient();
            
            //----Headers
            var appInfo = new ProductInfoHeaderValue("rEDH", "0.1");
            var acceptInfo = new MediaTypeWithQualityHeaderValue("application/json");

            httpClient.DefaultRequestHeaders.UserAgent.Add(appInfo);
            httpClient.DefaultRequestHeaders.Accept.Add(acceptInfo);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.scryfall.com/cards/random");
            var response = await httpClient.SendAsync(request);


            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var newCard = JsonConvert.DeserializeObject<Card>(jsonString);
            }
            else
            {
                String error = response.StatusCode.ToString() + " " + response.ReasonPhrase.ToString();
                Console.WriteLine(error);
            }
        }
        private Window m_window;

        private HttpClient httpClient;
    }
    
    public static class Const
    {
        public const string searchURL = "https://api.scryfall.com/cards/search";
        public const string randomURL = "https://api.scryfall.com/cards/random";
    }

    [Serializable]
    public class Card
    {
        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("mana_cost")]
        public string mana_cost {  get; set; }

        [JsonPropertyName("cmc")]
        public float cmc {  get; set; }
    }
    [Serializable]
    public class CardList
    {
        
        public IEnumerable<Card> Cards { get; set; }
    }
}
