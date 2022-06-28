using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace RedCorners.Components
{
    public class NominatimPlaces : IPlaces
    {
        public string ApiUrl { get; set; } = "https://nominatim.openstreetmap.org/";
        public bool UseCache { get; set; } = true;

        RestClient client = null;

        class Cache
        {
            public int Priority { get; set; }
            public string Query { get; set; }
            public List<NominatimPlace> Results { get; set; }
        }

        readonly List<Cache> cache = new List<Cache>();
        const int CacheSize = 10;

        public NominatimPlaces()
        {

        }

        public NominatimPlaces(string apiUrl)
        {
            this.ApiUrl = ApiUrl;
        }

        public async Task<List<Place>> SearchAsync(string query)
        {
            return
                (await SearchRawAsync(query, false, 0, 0))
                .Select(x => NominatimPlaceToPlace(x))
                .ToList();
        }

        public async Task<List<Place>> SearchAsync(string query, double centerLatitude, double centerLongitude)
        {
            return
                (await SearchRawAsync(query, true, centerLatitude, centerLongitude))
                .Select(x => NominatimPlaceToPlace(x))
                .ToList();
        }

        public async Task<List<NominatimPlace>> SearchRawAsync(string query, bool aroundRegion, double centerLatitude, double centerLongitude)
        {
            if (string.IsNullOrWhiteSpace(ApiUrl))
                throw new ArgumentNullException("Please specify the ApiUrl");

            if (aroundRegion)
            {
                Console.WriteLine("Nominatim search around a location is not yet supported. Remove the coordinates and try again.");
                //aroundRegion = false;
            }

            string locationQuery = "";
            if (aroundRegion)
                locationQuery = $" near [{centerLatitude},{centerLongitude}]";

            var url = "search?" +
                $"q={query}{locationQuery}&" +
                $"format=geocodejson&" +
                $"addressdetails=1";

            if (client == null) client = new RestClient(ApiUrl);

            var hit = cache.FirstOrDefault(x => x.Query == url);
            if (hit != null || !UseCache)
            {
                hit.Priority++;
                return hit.Results;
            }

            var request = new RestRequest(url, Method.Get);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return null;

            var results = JsonConvert.DeserializeObject<NominatimResult>(response.Content)?.features?
               .Select(x => new NominatimPlace
               {
                   Country = x.properties.geocoding.country,
                   County = x.properties.geocoding.county,
                   HouseNumber = x.properties.geocoding.housenumber,
                   Label = x.properties.geocoding.label,
                   Latitude = x.geometry.coordinates[0],
                   Longitude = x.geometry.coordinates[1],
                   Name = x.properties.geocoding.name,
                   OsmId = x.properties.geocoding.osm_id,
                   OsmType = x.properties.geocoding.osm_type,
                   PlaceId = x.properties.geocoding.place_id,
                   PostCode = x.properties.geocoding.postcode,
                   State = x.properties.geocoding.state,
                   Street = x.properties.geocoding.street,
                   Type = x.properties.geocoding.osm_type
               })
               ?.ToList();

            while (cache.Count > CacheSize)
            {
                var leastValuedItem = cache.OrderByDescending(x => x.Priority).First();
                cache.Remove(leastValuedItem);
            }

            foreach (var item in cache)
            {
                item.Priority++;
            }

            cache.Add(new Cache
            {
                Query = url,
                Results = results
            });

            return results;
        }

        class NominatimGeometry
        {
            public double[] coordinates { get; set; }
        }

        class NominatimGeocoding
        {
            public string place_id { get; set; }
            public string osm_type { get; set; }
            public string osm_id { get; set; }
            public string parking { get; set; }
            public string label { get; set; }
            public string name { get; set; }
            public string street { get; set; }
            public string postcode { get; set; }
            public string state { get; set; }
            public string county { get; set; }
            public string country { get; set; }
            public string housenumber { get; set; }
            public Dictionary<string, string> admin { get; set; }
        }

        class NominatimProperties
        {
            public NominatimGeocoding geocoding { get; set; }
        }

        class NominatimFeature
        {
            public NominatimProperties properties { get; set; }
            public NominatimGeometry geometry { get; set; }
        }

        class NominatimResult
        {
            public NominatimFeature[] features { get; set; }
        }

        public static Place NominatimPlaceToPlace(NominatimPlace item)
        {
            return new Place
            {
                Address = item.Label,
                HasCoordinates = true,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                Name = item.Name,
                Tag = item
            };
        }

        public async Task<NominatimReverse> ReverseAsync(double lat, double lon)
        {
            var url = $"{ApiUrl}reverse?lat={lat}&lon={lon}&format=json";
            var client = new RestClient(url);
            var response = await client.ExecuteAsync(new RestRequest());
            var obj = JsonConvert.DeserializeObject<NominatimReverse>(response.Content);
            return obj;
        }
    }

    public class NominatimAddress
    {
        public string Road { get; set; }
        public string Hamlet { get; set; }
        public string Suburb { get; set; }
        public string Town { get; set; }
        public string City { get; set; }
        public string Neighbourhood { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        [JsonProperty("postcode")] public string PostCode { get; set; }
        public string Country { get; set; }
        [JsonProperty("country_code")] public string CountryCode { get; set; }
    }

    public class NominatimReverse
    {
        [JsonProperty("place_id")] public string PlaceId { get; set; }
        [JsonProperty("license")] public string License { get; set; }
        [JsonProperty("osm_type")] public string OsmType { get; set; }
        [JsonProperty("osm_id")] public string OsmId { get; set; }
        [JsonProperty("lat")] public double Latitude { get; set; }
        [JsonProperty("lon")] public double Longitude { get; set; }
        [JsonProperty("display_name")] public string DisplayName { get; set; }
        public NominatimAddress Address { get; set; }
    }
}
