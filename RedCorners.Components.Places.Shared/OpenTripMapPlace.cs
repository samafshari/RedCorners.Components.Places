using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static RedCorners.Components.OpenTripMapClient;

namespace RedCorners.Components
{
    public class OpenTripMapSimpleFeature
    {
        public string XId { get; set; }
        public string Name { get; set; }
        public double Distance { get; set; }
        public string Rate { get; set; }
        public string Wikidata { get; set; }
        public string[] Kinds { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OSM { get; set; }
    }

    public class OpenTripMapGeoName
    {
        public string Name { get; set; }
        public string Country { get; set; }
        [JsonProperty("lat")] public double Latitude { get; set; }
        [JsonProperty("lon")] public double Longitude { get; set; }
        public int Population { get; set; }
        public string Timezone { get; set; }
    }

    public class OpenTripMapPlace
    {
        public string XId { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Rate { get; set; }
        public string OSM { get; set; }
        public string Wikidata { get; set; }
        public string[] Kinds { get; set; }
        public string OTM { get; set; }
        public string Wikipedia { get; set; }
        public string Image { get; set; }
        public Preview Preview { get; set; }
        public WikipediaExtracts WikipediaExtracts { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
