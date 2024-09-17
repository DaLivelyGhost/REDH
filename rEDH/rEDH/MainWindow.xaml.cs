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
        public MainWindow(ApiWrangler wrangler, CardList cardList, App controller)
        {
            this.InitializeComponent();
            this.controller = controller;
        }
        async void generateButtonClick(object sender, RoutedEventArgs e)
        {
            controller.generateDeck((bool)whiteCheckBox.IsChecked, (bool)blueCheckBox.IsChecked, (bool)blackCheckBox.IsChecked, 
                                    (bool)redCheckBox.IsChecked, (bool)greenCheckBox.IsChecked);

            //Task<Card> taskCard = controller.demoCard();
            //Card newCard = await taskCard;
            //demoPopulate(newCard);
        }
        async void updateDatabaseButtonClick(object sender, RoutedEventArgs e)
        {
            controller.updateDatabase();
        }
        public void demoPopulate(Card card)
        {
            BitmapImage bit = new BitmapImage();

            bit.UriSource = new Uri(card.image_uris.normal);

            imageTestBlue.Source = bit;
        }



    }

}
