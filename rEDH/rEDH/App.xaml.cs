
using Microsoft.Data.Sqlite;
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
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace rEDH
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

        private Window m_window;
        private DeckList deckList;
        private ApiWrangler apiWrangler;
        private DatabaseWrangler databaseWrangler;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
           
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.apiWrangler = new ApiWrangler();
            this.databaseWrangler = new DatabaseWrangler();
            this.deckList = new DeckList();

            m_window = new MainWindow(apiWrangler, this);
            m_window.Activate();
            
        }

        public async void generateDeck(bool white, bool blue, bool black, bool red, bool green)
        {

        }
        public async void updateDatabase()
        {
            databaseWrangler.refreshTables();

            //get a byte array representing all the cards on scryfall
            Task<byte[]> byteArrayTask = apiWrangler.queryBulkData();
            byte[] bytes = await byteArrayTask;

            //get a local string to the assets folder
            string assetsDir = AppDomain.CurrentDomain.BaseDirectory + "Assets\\";
            System.Diagnostics.Debug.WriteLine(assetsDir);

            string filePath = assetsDir + "ScryFallBulk.json";

            //save all the cards on scryfall to the assets folder as a json
            System.Diagnostics.Debug.WriteLine("File Writing Start!");
            File.WriteAllBytes(filePath, bytes);
            System.Diagnostics.Debug.WriteLine(" File Writing Done!");

            //deserialize the now downloaded data into useable card objects.
            Card[] cardList = JsonConvert.DeserializeObject<Card[]>(File.ReadAllText(filePath));

            //Delete the JSON object to save space
            File.Delete(filePath);

            //Catalog cards from the array
            System.Diagnostics.Debug.WriteLine("Cataloging Start!");
            databaseWrangler.addCardsBulk(cardList);
            System.Diagnostics.Debug.WriteLine("Cataloging Done!");

            //set time last updated
            string updateTime = DateTime.Now.ToString();

            databaseWrangler.setTimeUpdated(updateTime);
        }
        public string getUpdateTime()
        {
            return null;
        }
        //public async Task<Card> demoCard()
        //{
        //    //query scryfall and deserialize json into card
        //    Task<Card> cardTask = apiWrangler.testQuery();
        //    Card newCard = await cardTask;

        //    //add card object to database and query card back for demo purposes
        //    databaseWrangler.addCard(newCard);
        //    Card databaseCard = databaseWrangler.getCardByName(newCard.name);
            
        //    return databaseCard;
        //}


    }

}
