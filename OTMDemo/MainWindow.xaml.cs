using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RedCorners;
using RedCorners.Components;

namespace OTMDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenTripMapClient client;

        public string Query { get; set; }

        public string ApiKey
        {
            get => client.ApiKey;
            set => client.ApiKey = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            client = new OpenTripMapClient("");
            btnSearch.Click += BtnSearch_Click;
            grdPlaces.SelectionChanged += GrdPlaces_SelectionChanged;
        }

        private async void GrdPlaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = grdPlaces.SelectedItem as OpenTripMapSimpleFeature;
            if (item == null) return;

            var x = await client.GetPlaceAsync(item.XId);
            grdX.SelectedObject = x;
            grdPreview.SelectedObject = x?.Preview;
            grdWiki.SelectedObject = x?.WikipediaExtracts;
            browser.Navigate($"https://www.google.com/maps/@{x?.Latitude},{x?.Longitude},14z");
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var geoname = await client.GetGeoNameAsync(Query);
                var places = await client.GetFeaturesAsync(
                    10000,
                    geoname.Latitude,
                    geoname.Longitude);
                grdPlaces.ItemsSource = places;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
