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
        public const string DefaultInterpreterUrl = "https://lz4.overpass-api.de/api/interpreter";
        readonly RestClient client;

        public OverpassClient(string url = DefaultInterpreterUrl)
        {
            client = new RestClient(url);
        }

        public async Task<AmenityNode[]> SearchAmenityNodes(string amenity, double plat, double plng, double qlat, double qlng, int? timeout = 10)
        {
            var minLat = Math.Min(plat, qlat);
            var minLng = Math.Min(plng, qlng);
            var maxLat = Math.Max(plat, qlat);
            var maxLng = Math.Max(plng, qlng);

            var timeoutQuery = timeout.HasValue ? $"[timeout:{timeout}]" : string.Empty;
            var query = $"[out:json]{timeoutQuery};node [amenity={amenity}]({minLat},{minLng},{maxLat},{maxLng}); out body; >; out skel qt;";
            var request = new RestRequest(Method.POST);
            request.AddParameter("text/plain", query, ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return null;
            var overpassResponse = JsonConvert.DeserializeObject<OverpassResponse>(response.Content)?.elements;
            if (overpassResponse == null) return null;
            return overpassResponse.Select(x => new AmenityNode
            {
                Amenity = amenity,
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
    }
}
