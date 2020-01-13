using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RedCorners.Components
{
    public class OverpassClient
    {
        public static readonly string[] DefaultInterpreterUrls = new[]
        {
            "https://lz4.overpass-api.de/api/interpreter",
            "https://z.overpass-api.de/api/interpreter",
            "http://overpass.openstreetmap.fr/api/interpreter",
            "https://overpass.kumi.systems/api/interpreter"
        };

        RestClient[] clients;

        public OverpassClient() 
        {
            InitializeClients(DefaultInterpreterUrls);
        }

        public OverpassClient(string[] urls)
        {
            InitializeClients(urls);
        }

        void InitializeClients(string[] urls)
        {
            clients = urls.Select(x => new RestClient(x)).ToArray();
        }

        static Random Random = new Random();

        RestClient GetClient()
        {
            return clients[Random.Next(clients.Length)];
        }

        public async Task<AmenityNode[]> SearchAmenityNodes(string[] amenities, double plat, double plng, double qlat, double qlng, int? timeout = 10)
        {
            var minLat = Math.Min(plat, qlat);
            var minLng = Math.Min(plng, qlng);
            var maxLat = Math.Max(plat, qlat);
            var maxLng = Math.Max(plng, qlng);

            var amenityQuery = "[\"amenity\"~\"" + string.Join("|", amenities) + "\"]";
            var timeoutQuery = timeout.HasValue ? $"[timeout:{timeout}]" : string.Empty;
            var query = $"[out:json]{timeoutQuery};node {amenityQuery}({minLat},{minLng},{maxLat},{maxLng}); out body; >; out skel qt;";
            return await SearchAsync(query);
        }

        public async Task<AmenityNode[]> SearchAmenityNodes(string[] amenities, double lat, double lng, double radius, int? timeout = 10)
        {
            var amenityQuery = "[\"amenity\"~\"" + string.Join("|", amenities) + "\"]";
            var timeoutQuery = timeout.HasValue ? $"[timeout:{timeout}]" : string.Empty;
            var query = $"[out:json]{timeoutQuery};node {amenityQuery}(around: {radius*1000.0}, {lat},{lng}); out body; >; out skel qt;";
            return await SearchAsync(query);
        }

        async Task<AmenityNode[]> SearchAsync(string query)
        {
            var request = new RestRequest(Method.POST);
            request.AddParameter("text/plain", query, ParameterType.RequestBody);
            var response = await GetClient().ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return null;
            var overpassResponse = JsonConvert.DeserializeObject<OverpassResponse>(response.Content)?.elements;
            if (overpassResponse == null) return null;
            return overpassResponse.Select(x => new AmenityNode
            {
                Amenity = x.tags?.FirstOrDefault(y => y.Key == "amenity").Value,
                Id = x.id,
                Latitude = x.lat,
                Longitude = x.lon,
                Tags = x.tags
            }).ToArray();
        }

        class OverpassResponse
        {
            public OverpassObject[] elements { get; set; }
        }

        class OverpassObject
        {
            public string type { get; set; } //= "node"
            public string id { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public Dictionary<string, string> tags { get; set; }
        }

        public static double GetDiameter(double plat, double plng, double qlat, double qlng)
        {
            var minLat = Math.Min(plat, qlat);
            var minLng = Math.Min(plng, qlng);
            var maxLat = Math.Max(plat, qlat);
            var maxLng = Math.Max(plng, qlng);

            return GetDistance(minLat, minLng, maxLat, maxLng);
        }

        public static double GetDistance(double latitude, double longitude, double otherLatitude, double otherLongitude) //km
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 0.001 * 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
