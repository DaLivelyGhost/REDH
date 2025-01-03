﻿
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
using Windows.Devices.SmartCards;


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
        private ApiWrangler apiWrangler;
        private DatabaseWrangler databaseWrangler;
        private DeckBuilder deckBuilder;
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
            this.deckBuilder = new DeckBuilder();

            m_window = new MainWindow(apiWrangler, this);
            m_window.Activate();
            
        }

        public async Task<Card[]> generateDeck(DeckDefinitions definition)
        {
            Task<Card[]> deckTask = deckBuilder.buildDeck(databaseWrangler, definition);
            Card[] cards = await deckTask;

            return cards;   
        }
        public async Task<string> updateDatabase()
        {
            //if database file does not exist yet
            if(!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Assets\\cardDatabase.db"))
            {
                //test the connection to Scryfall by querying a single random card.
                //if it fails, then we can't connect and should not build the database.
                //Task<string> connectionTestTask = apiWrangler.testConnection();
                //string connectionTest;
                //try
                //{
                //    connectionTest = await connectionTestTask;
                //}
                //catch (Exception ex)
                //{
                //    return ex.Message;
                //}

                databaseWrangler.createConnection();
            }
            //if the database file does exist
            else
            {
                //test the connection to Scryfall by querying a single random card.
                //if it fails, then we can't connect and should not build the database.
                //Task<string> connectionTestTask = apiWrangler.testConnection();
                //string connectionTest;
                //try
                //{
                //    connectionTest = await connectionTestTask;
                //}
                //catch (Exception ex)
                //{
                //    return ex.Message;
                //}


                databaseWrangler.refreshTables();
            }

            //get a byte array representing all the cards on scryfall
            Task<byte[]> byteArrayTask = apiWrangler.queryBulkData();
            byte[] bytes;
            try
            {
                bytes = await byteArrayTask;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

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
            
            return getUpdateTime();
        }
        public Card[] sortByName()
        {
            try
            {
                DeckList deckList = deckBuilder.getDeckList();
                deckList.nameSort();
                return (deckList.getDeck());
            }
            catch(Exception e)
            {

                return null; 
            }

        }
        public Card[] sortByCmc()
        {
            try
            {
                DeckList deckList = deckBuilder.getDeckList();
                deckList.cmcSort();
                return (deckList.getDeck());
            }
            catch (Exception e)
            {

                return null;
            }
        }
        //TODO come back and finish this after we fix the data atrophe happening from the database.
        public Card[] sortByType()
        {
            try
            {
                DeckList deckList = deckBuilder.getDeckList();
                deckList.typeSort();
                return (deckList.getDeck());
            }
            catch (Exception e)
            {

                return null; 
            }
        }
        public string getUpdateTime()
        {

            try
            {
                string updateTime = databaseWrangler.getTimeUpdated();

                if(updateTime == null || updateTime.Equals(""))
                {
                    return "Database not found, or not initialized yet.";
                }

                return "Last updated: " + updateTime;
            }
            catch
            {
                return "Database Not Found";
            }

        }
        public string getDeckListString()
        {
            string cards = "";

            DeckList deckList = deckBuilder.getDeckList();

            for(int i = 0; i < 100; i++)
            {
                cards += deckList.getCard(i).name;
                cards += "\n";
            }

            return cards;
        }

    }

}
