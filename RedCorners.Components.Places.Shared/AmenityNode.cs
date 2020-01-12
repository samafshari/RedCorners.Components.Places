using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Components
{
    public class AmenityNode
    {
        public string Id { get; set; }
        public string Amenity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Dictionary<string, string> Tags { get; set; }
    }
}
