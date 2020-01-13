using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using RestSharp;
using Newtonsoft.Json;

namespace RedCorners.Components
{
    public class GooglePlaces : IPlaces
    {
        public string ApiKey { get; set; }
        public bool UseCache { get; set; } = true;
        public int Radius { get; set; } = 50000;
        const string ApiUrl = "https://maps.googleapis.com/maps/api/place/";

        RestClient client = null;

        class Cache
        {
            public int Priority { get; set; }
            public string Query { get; set; }
            public List<GooglePlace> Results { get; set; }
        }

        class AddressCache
        {
            public int Priority { get; set; }
            public string Query { get; set; }
            public GoogleAddressComponent[] Results { get; set; }
        }


        readonly List<AddressCache> addressCache = new List<AddressCache>();

        readonly List<Cache> cache = new List<Cache>();
        const int CacheSize = 10;

        public GooglePlaces()
        {

        }

        public GooglePlaces(string apiKey)
        {
            this.ApiKey = apiKey;
        }

        public async Task<List<Place>> SearchAsync(string query)
        {
            return
                (await SearchRawAsync(query, false, 0, 0))
                .Select(x => GooglePlaceToPlace(x))
                .ToList();
        }

        public async Task<List<Place>> SearchAsync(string query, double centerLatitude, double centerLongitude)
        {
            return
                (await SearchRawAsync(query, true, centerLatitude, centerLongitude))
                .Select(x => GooglePlaceToPlace(x))
                .ToList();
        }

        public async Task<List<GooglePlace>> SearchRawAsync(string query, bool aroundRegion, double centerLatitude, double centerLongitude)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new ArgumentNullException("Please specify the ApiKey for the Google API");

            string locationQuery = "";
            if (aroundRegion)
                locationQuery = $"location={centerLatitude},{centerLongitude}&radius={Radius}&";

            var url = "textsearch/json?" +
                $"query={query}&" +
                locationQuery +
                $"key={ApiKey}";

            if (client == null) client = new RestClient(ApiUrl);

            var hit = cache.FirstOrDefault(x => x.Query == url);
            if (hit != null || !UseCache)
            {
                hit.Priority++;
                return hit.Results;
            }

            var request = new RestRequest(url, Method.GET);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return null;

            var results = JsonConvert.DeserializeObject<GoogleSearchResults>(response.Content)?.results?
               .Select(x => new GooglePlace
               {
                   Address = x.formatted_address,
                   Icon = x.icon,
                   Id = x.id,
                   Latitude = x.geometry?.location?.lat ?? 0,
                   Longitude = x.geometry?.location?.lng ?? 0,
                   Name = x.name,
                   PlaceId = x.place_id,
                   Rating = x.rating,
                   Types = x.types,
                   UserRatingsTotal = x.user_ratings_total
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

        public async Task<GoogleAddressComponent[]> QueryAddressComponentsAsync(string placeId)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
                throw new ArgumentNullException("Please specify the ApiKey for the Google API");

            var url = "details/json?" +
                $"place_id={placeId}&" +
                $"key={ApiKey}";

            if (client == null) client = new RestClient(ApiUrl);

            var hit = addressCache.FirstOrDefault(x => x.Query == placeId);
            if (hit != null || !UseCache)
            {
                hit.Priority++;
                return hit.Results;
            }

            var request = new RestRequest(url, Method.GET);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return null;


            var results = JsonConvert.DeserializeObject<PlaceDetailsResponse>(response.Content)?.result?.address_components
               .Select(x => new GoogleAddressComponent
               {
                   LongName = x.long_name,
                   ShortName = x.short_name,
                   Types = x.types
               })
               ?.ToArray();

            while (addressCache.Count > CacheSize)
            {
                var leastValuedItem = addressCache.OrderByDescending(x => x.Priority).First();
                addressCache.Remove(leastValuedItem);
            }

            foreach (var item in addressCache)
            {
                item.Priority++;
            }

            addressCache.Add(new AddressCache
            {
                Query = url,
                Results = results
            });

            return results;
        }

        public static Place GooglePlaceToPlace(GooglePlace item)
        {
            return new Place
            {
                Address = item.Address,
                HasCoordinates = item.Latitude != 0 && item.Longitude != 0,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                Name = item.Name,
                Tag = item
            };
        }

        #region textsearch models
        class GoogleSearchResults
        {
            public List<Candidate> results { get; set; }
        }

        class Candidate
        {
            public string formatted_address { get; set; }
            public Geometry geometry { get; set; }
            public string name { get; set; }
            public string icon { get; set; }
            public string id { get; set; }
            public double rating { get; set; }
            public string[] types { get; set; }
            public int user_ratings_total { get; set; }
            public string place_id { get; set; }

        }

        class Geometry
        {
            public Location location { get; set; }
        }

        class Location
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
        #endregion

        #region place details models
        class PlaceDetailsResponse
        {
            public PlaceDetailsResult result { get; set; }
        }

        class PlaceDetailsResult
        {
            public address_component[] address_components { get; set; }
        }

        class address_component
        {
            public string long_name { get; set; }
            public string short_name { get; set; }
            public string[] types { get; set; }
        }

        public class GoogleAddressComponent
        {
            public string LongName { get; set; }
            public string ShortName { get; set; }
            public string[] Types { get; set; }

            public override string ToString() => LongName;
        }
        #endregion
    }
}
