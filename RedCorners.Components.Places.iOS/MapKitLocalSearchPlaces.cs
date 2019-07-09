using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MapKit;
using Foundation;
using CoreLocation;
using UIKit;
using System.Threading.Tasks;
using Contacts;

namespace RedCorners.Components
{
    public class MapKitLocalSearchPlaces : NSObject, IPlaces, IMKLocalSearchCompleterDelegate
    {
        MKLocalSearchCompleter completer;
        public MapKitLocalSearchPlaces()
        {
            completer = new MKLocalSearchCompleter();
            completer.Delegate = this;
        }

        public bool FetchCoordinates { get; set; } = true;
        public double DeltaLatitude { get; set; } = 0.25;
        public double DeltaLongitude { get; set; } = 0.25;

        public async Task<List<Place>> SearchAsync(string query)
        {
            completer.QueryFragment = query;
            return await Search();
        }

        public async Task<List<Place>> SearchAsync(string query, double centerLatitude, double centerLongitude)
        {
            var center = new CLLocationCoordinate2D(centerLatitude, centerLongitude);
            var span = new MKCoordinateSpan(DeltaLatitude, DeltaLongitude);
            completer.Region = new MKCoordinateRegion(center, span);
            return await SearchAsync(query);
        }

        async Task<List<Place>> Search()
        {
            while (completer.Searching)
                await Task.Delay(50);
            if (completer.Results == null || completer.Results.Length == 0)
                return new List<Place>();

            var results = new List<Place>();
            foreach (var item in completer.Results)
            {
                var result = new Place();

                result.Name = item.Title;
                result.Address = item.Subtitle;
                result.HasCoordinates = false;
                result.Tag = item;

                results.Add(result);
            }

            if (FetchCoordinates)
            {
                var geocoder = new CLGeocoder();
                foreach (var item in results)
                {
                    try
                    {
                        var placemarks = await geocoder.GeocodeAddressAsync(item.Address);
                        var placemark = placemarks.FirstOrDefault();
                        if (placemark != null)
                        {
                            //item.Name = placemark.Name;
                            if (placemark.Location != null)
                            {
                                item.HasCoordinates = true;
                                item.Latitude = placemark.Location.Coordinate.Latitude;
                                item.Longitude = placemark.Location.Coordinate.Longitude;
                            }
                            //if (placemark.PostalAddress != null)
                            //    item.Address = MapKitPlaces.GetAddress(placemark.PostalAddress);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return results;
        }
    }
}