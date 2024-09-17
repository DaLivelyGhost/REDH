
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
        private CardList cardList;
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
            this.cardList = new CardList();

            m_window = new MainWindow(apiWrangler, cardList, this);
            m_window.Activate();
  
        }   
        
        public async void generateDeck(bool white, bool blue, bool black, bool red, bool green)
        {
            bool[] checkboxes = { white, blue, black, red, green };
            
            //ask apiwrangler to query scryfall for cards
            Task<ApiList> apilisttask = apiWrangler.queryCards(checkboxes);
            ApiList list = await apilisttask;

            bool firstPage = true;

            //now that we have the list of cards, store them in the database.
            do
            {
                if(!firstPage && list.has_more)
                {
                    apilisttask = apiWrangler.QueryFromURI(list.next_page);
                    list = await apilisttask;
                    await Task.Delay(100);
                }

                foreach(Card c in list.data)
                {
                    databaseWrangler.addCard(c);
                }


            } while (list.has_more);
            System.Diagnostics.Debug.WriteLine("Done!");
            //now that they've been logged in the database, we can begin deck building algorithms

            //DECK BUILDING ALGORITHMS GO HERE

            //now display the list of card objects in the ui
        }
        public async void updateDatabase()
        {
            //get a byte array representing all the cards on scryfall
            Task<byte[]> byteArrayTask = apiWrangler.queryBulkData();
            byte[] bytes = await byteArrayTask;

            //get a local string to the assets folder
            string assetsDir = AppDomain.CurrentDomain.BaseDirectory + "Assets\\";
            System.Diagnostics.Debug.WriteLine(assetsDir);

            string filePath = assetsDir + "ScryFallBulk.json";

            //save all the cards on scryfall to the assets folder as a json
            File.WriteAllBytes(filePath, bytes);
            System.Diagnostics.Debug.WriteLine("Done!");

        }
        public async Task<Card> demoCard()
        {
            //query scryfall and deserialize json into card
            Task<Card> cardTask = apiWrangler.testQuery();
            Card newCard = await cardTask;

            //add card object to database and query card back for demo purposes
            databaseWrangler.addCard(newCard);
            Card databaseCard = databaseWrangler.getCardByName(newCard.name);
            
            return databaseCard;
        }


    }

}
