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
        }

        //-------Button Events--------------------------------------------
        async void generateButtonClick(object sender, RoutedEventArgs e)
        {
            Task<Card[]> deckTask = controller.generateDeck((bool)whiteCheckBox.IsChecked, (bool)blueCheckBox.IsChecked, (bool)blackCheckBox.IsChecked, 
                                    (bool)redCheckBox.IsChecked, (bool)greenCheckBox.IsChecked);
            Card[] deckList = deckTask.Result;

            populateCardImages(deckList);
        }
        async void updateDatabaseButtonClick(object sender, RoutedEventArgs e)
        {
           controller.updateDatabase();
        }
        //-------End Button Events--------------------------------------

        public void setUpdateTime()
        {

        }

        public async void populateCardImages(Card[] cards)
        {
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
