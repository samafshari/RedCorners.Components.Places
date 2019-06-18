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
    public partial class Places
    {
        public async Task<List<Place>> SearchAsync(string query)
        {
            return (await SearchAsync(query, false, 0, 0)).ToList();
        }

        static async Task<IEnumerable<Place>> SearchAsync(
            string query, 
            bool aroundRegion,
            double centerLatitude, 
            double centerLongitude, 
            double deltaLatitude = 0.25,
            double deltaLongitude = 0.25)
        {
            var request = new MKLocalSearchRequest();
            request.NaturalLanguageQuery = query;

            if (aroundRegion)
            {
                var center = new CLLocationCoordinate2D(centerLatitude, centerLongitude);
                var span = new MKCoordinateSpan(deltaLatitude, deltaLongitude);
                request.Region = new MKCoordinateRegion(center, span);
            }

            var search = new MKLocalSearch(request);
            var response = await search.StartAsync();

            if (response == null) return null;
            if (response.MapItems == null) return null;

            return response.MapItems.Select(x => MKToPlace(x));
        }

        static Place MKToPlace(MKMapItem item)
        {
            return new Place
            {
                IsCurrentLocation = item.IsCurrentLocation,
                Name = item.Name,
                PhoneNumber = item.PhoneNumber,
                Url = item.Url?.AbsoluteString,
                Latitude = item.Placemark?.Location?.Coordinate.Latitude ?? 0,
                Longitude = item.Placemark?.Location?.Coordinate.Longitude ?? 0,
                HasCoordinates = item.Placemark?.Location?.Coordinate != null,
                Street = item.Placemark?.PostalAddress?.Street,
                City = item.Placemark?.PostalAddress?.City,
                State = item.Placemark?.PostalAddress?.State,
                PostalCode = item.Placemark?.PostalAddress?.PostalCode,
                Country = item.Placemark?.PostalAddress?.Country,
                Address = GetAddress(item.Placemark?.PostalAddress)
            };
        }

        static CNPostalAddressFormatter Formatter = new CNPostalAddressFormatter();
        static string GetAddress(CNPostalAddress address)
        {
            if (address == null) return null;
            return Formatter.GetStringFromPostalAddress(address);
        }
    }
}
