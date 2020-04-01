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
            var item = grdPlaces.SelectedItem as OpenTripMapPlace;
            if (item == null) return;

            var x = await client.GetXAsync(item.XId);
            grdX.SelectedObject = x;
            grdPreview.SelectedObject = x?.Preview;
            grdWiki.SelectedObject = x?.WikipediaExtracts;
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var geoname = await client.GetGeoNameAsync(Query);
                var places = await client.GetPlacesAroundAsync(
                    geoname.Latitude,
                    geoname.Longitude,
                    10000,
                    100,
                    2);
                grdPlaces.ItemsSource = places;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
