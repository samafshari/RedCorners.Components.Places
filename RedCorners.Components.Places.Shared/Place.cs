using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Components
{
    public class Place
    {
        public string Name { get; set; }
        //public string PhoneNumber { get; set; }
        public string Url { get; set; }
        //public bool IsCurrentLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool HasCoordinates { get; set; }
        public string Address { get; set; }
        //public string Street { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string PostalCode { get; set; }
        //public string Country { get; set; }

        public override string ToString()
        {
            return 
                $"Name: {Name}, " +
                //$"PhoneNumber: {PhoneNumber}, " +
                $"Url: {Url}, " +
                //$"IsCurrentLocation: {IsCurrentLocation}, " +
                $"Coordinates: ({Latitude}, {Longitude}), " +
                $"Address: {Address}";
        }
    }
}
