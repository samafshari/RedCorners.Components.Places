using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace RedCorners.Components
{
    public class HerePlaces : IPlaces
    {
        class PlaceModel
        {
            public string Title { get; set; }
            public string HighlightedTitle { get; set; }
            public string Vicinity { get; set; }
            public string HighlightedVicinity { get; set; }
            public double[] Position { get; set; }
            public string Category { get; set; }
            public string CategoryTitle { get; set; }
            public string Href { get; set; }
            public string Type { get; set; }
            public string ResultType { get; set; }
            public string Id { get; set; }
            public double Distance { get; set; }
            public double[] Bbox { get; set; }
        }

        class PlacesModel
        {
            public PlaceModel[] Results { get; set; }
        }

        class Cache
        {
            public int Priority { get; set; }
            public string Query { get; set; }
            public List<HerePlace> Results { get; set; }
        }

        public string AppId { get; set; }
        public string AppCode { get; set; }

        bool _useCitApi = false;
        public bool UseCitApi
        {
            get => _useCitApi;
            set
            {
                if (_useCitApi == value) return;
                _useCitApi = value;
                client = null;
            }
        }

        const string ProdApiUrl = "https://places.api.here.com/places/v1/";
        const string CitApiUrl = "https://places.cit.api.here.com/places/v1/";

        public string ApiUrl => UseCitApi ? CitApiUrl : ProdApiUrl;

        RestClient client = null;

        readonly List<Cache> cache = new List<Cache>();
        const int CacheSize = 10;
        public bool UseCache { get; set; } = true;

        public HerePlaces()
        {

        }

        public HerePlaces(string appId, string appCode)
        {
            this.AppId = appId;
            this.AppCode = appCode;
        }

        public Task<List<Place>> SearchAsync(string query)
        {
            throw new ArgumentException("The HERE API requires you to specify a center. Please provide the centerLatitude and centerLongitude parameters.");
        }

        public async Task<List<Place>> SearchAsync(string query, double centerLatitude, double centerLongitude)
        {
            return
                (await SearchRawAsync(query, centerLatitude, centerLongitude))
                .Select(x => HerePlaceToPlace(x))
                .ToList();
        }

        public async Task<List<HerePlace>> SearchRawAsync(string query, double centerLatitude, double centerLongitude)
        {
            if (string.IsNullOrWhiteSpace(AppId))
                throw new ArgumentNullException("Please specify the AppId for the HERE API");
            if (string.IsNullOrWhiteSpace(AppCode))
                throw new ArgumentNullException("Please specify the AppCode for the HERE API");

            var url = $"autosuggest?at={centerLatitude},{centerLongitude}&q={query}&app_id={AppId}&app_code={AppCode}";

            if (client == null) client = new RestClient(ApiUrl);

            var hit = cache.FirstOrDefault(x => x.Query == url);
            if (hit != null || !UseCache)
            {
                hit.Priority++;
                return hit.Results;
            }

            var request = new RestRequest(url, Method.GET);
            var response = await client.ExecuteTaskAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK) return null;

            var parser = new HtmlParser();
            string CleanText(string raw)
            {
                if (raw == null) return raw;
                raw = raw
                    .Replace("<br/>", "\n")
                    .Replace("<b>", "")
                    .Replace("</b>", "");
                return string.Join(", ", parser.ParseFragment(raw, null).Select(el => el.Text()));
            }

            var results = JsonConvert.DeserializeObject<PlacesModel>(response.Content)?.Results
               .Where(x => x.Vicinity != null && x.Title != null && x.Position != null && x.Position.Length == 2)
               .Select(x => new HerePlace
               {
                   Category = x.Category,
                   HighlightedTitle = CleanText(x.HighlightedTitle),
                   Href = x.Href,
                   Latitude = x.Position?[0] ?? 0,
                   Longitude = x.Position?[1] ?? 0,
                   Title = CleanText(x.Title),
                   Type = x.Type,
                   Vicinity = CleanText(x.Vicinity)
               })
               .ToList();

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

        public static Place HerePlaceToPlace(HerePlace item)
        {
            return new Place
            {
                Name = item.Title,
                Address = item.Vicinity,
                Url = item.Href,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                HasCoordinates = true
            };
        }
    }
}
