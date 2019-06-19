using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Components
{
    public class NominatimPlace
    {
        public string PlaceId { get; set; }
        public string OsmType { get; set; }
        public string OsmId { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string HouseNumber { get; set; }
    }
}
