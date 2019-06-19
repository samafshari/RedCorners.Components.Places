using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Components
{
    public class Place
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool HasCoordinates { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// This contains the original place object from the provider. Can be a MKMapItem, GooglePlace, HerePlace, NominatimPlace, etc.
        /// </summary>
        public object Tag { get; set; }

        public override string ToString()
        {
            return 
                $"Name: {Name}, " +
                //$"PhoneNumber: {PhoneNumber}, " +
                //$"Url: {Url}, " +
                //$"IsCurrentLocation: {IsCurrentLocation}, " +
                $"Coordinates: ({Latitude}, {Longitude}), " +
                $"Address: {Address}";
        }
    }
}
