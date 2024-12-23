using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static System.Net.WebRequestMethods;
using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Devices.Display.Core;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace rEDH
{

   
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        App controller;
        private Image[] cardArray;
        public MainWindow(ApiWrangler wrangler, App controller)
        {
            this.InitializeComponent();
            this.controller = controller;

            initializeCardImages();
            setUpdateTime();
        }

        //-------Button Events--------------------------------------------
        async void generateButtonClick(object sender, RoutedEventArgs e)
        {
            currTaskProgressBar.Visibility = Visibility.Visible;
            loadingText.Visibility = Visibility.Visible;
            disableButtons();

            string format = formatComboBox.SelectedItem as string;
            string manaCurve = manacurveComboBox.SelectedItem as string;
            bool[] selectedColors = [(bool)whiteCheckBox.IsChecked, (bool)blueCheckBox.IsChecked, 
                (bool)blackCheckBox.IsChecked, (bool)redCheckBox.IsChecked, (bool)greenCheckBox.IsChecked];

            DeckDefinitions definition = new DeckDefinitions(selectedColors, format, manaCurve);

            Task<Card[]> deckTask;
            Card[] deckList = { };
            try
            {
                deckTask = controller.generateDeck(definition);
                deckList = deckTask.Result;

                await populateCardImages(deckList);

                await Task.Delay(10);

                generateFailText.Text = "";
            }
            catch (Exception ex)
            {
                generateFailText.Text = ex.Message;
            }



            currTaskProgressBar.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;
            enableButtons();
        }
        async void updateDatabaseButtonClick(object sender, RoutedEventArgs e)
        {
            currTaskProgressBar.Visibility = Visibility.Visible;
            loadingText.Visibility = Visibility.Visible;
            disableButtons();

            try
            {
                await controller.updateDatabase();
                await setUpdateTime();

            }
            catch (Exception ex)
            {
                lastUpdatedText.Text = ex.Message;
            }


            currTaskProgressBar.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;
            enableButtons();
        }
        async void nameSortButtonClick(object sender, RoutedEventArgs e)
        {
            currTaskProgressBar.Visibility = Visibility.Visible;
            loadingText.Visibility = Visibility.Visible;
            disableButtons();

            await populateCardImages(controller.sortByName());

            currTaskProgressBar.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;
            enableButtons();
        }
        async void cmcSortButtonClick(object sender, RoutedEventArgs e)
        {
            currTaskProgressBar.Visibility = Visibility.Visible;
            loadingText.Visibility = Visibility.Visible;
            disableButtons();

            await populateCardImages(controller.sortByCmc());


            currTaskProgressBar.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;
            enableButtons();
        }
        async void typeSortButtonClick(object sender, RoutedEventArgs e)
        {
            currTaskProgressBar.Visibility = Visibility.Visible;
            loadingText.Visibility = Visibility.Visible;
            disableButtons();

            await populateCardImages(controller.sortByType());

            currTaskProgressBar.Visibility = Visibility.Collapsed;
            loadingText.Visibility = Visibility.Collapsed;
            enableButtons();
        }
        async void exportTextButtonClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            //Save file as .txt
            savePicker.FileTypeChoices.Add(".txt", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "New Document";


            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            if(file != null)
            {

                Windows.Storage.CachedFileManager.DeferUpdates(file);

                //change "file contents" to the decklist.
                await Windows.Storage.FileIO.WriteTextAsync(file, controller.getDeckListString());

                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

                if(status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    //update gui success
                }
                else
                {
                    //update gui file save failed
                }
            }
            else
            {
                //update gui operation cancelled
            }
        }
        //---------------------------------------------

        public async Task setUpdateTime()
        {
            string timeUpdated = controller.getUpdateTime();
            
            if (timeUpdated != null) {
                lastUpdatedText.Text = "Last Updated: " + timeUpdated;
            }
            else
            {
                lastUpdatedText.Text = "Database not found";
            }
        }
        private void disableButtons()
        {
            generateButton.IsEnabled = false;
            databaseButton.IsEnabled = false;
            cmcsortButton.IsEnabled = false;
            namesortButton.IsEnabled = false;
            typesortButton.IsEnabled = false;
            exportButton.IsEnabled = false;
        }
        private void enableButtons()
        {
            generateButton.IsEnabled = true;
            databaseButton.IsEnabled = true;
            cmcsortButton.IsEnabled = true;
            namesortButton.IsEnabled = true;
            typesortButton.IsEnabled = true;
            exportButton.IsEnabled = true;
        }
        public async Task populateCardImages(Card[] cards)
        {
            if(cards == null)
            {
                return;
            }

            for(int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    try
                    {
                        cardArray[i].Source = new BitmapImage(new Uri(cards[i].image_uris.normal));
                    }
                    catch (Exception ex)
                    {
                        //do nothing
                    }
                }
                await Task.Delay(100);
            }
        }
        private void initializeCardImages()
        {
            cardArray = new Image[100];
            int row = 0;
            int col = 0;

            for(int i = 0; i < 100; i++)
            {
                cardArray[i] = new Image();
                //cardArray[i].Width = 672;

                cardArray[i].Stretch = Stretch.UniformToFill;

                cardArray[i].Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Assets\\null_image.jpg"));

                deckView.Children.Add(cardArray[i]);

                Grid.SetRow(cardArray[i], row);
                Grid.SetColumn(cardArray[i], col);

                col++;

                if(col == 5)
                {
                    col = 0;
                    row++;
                }
            }
        }

    }

}
