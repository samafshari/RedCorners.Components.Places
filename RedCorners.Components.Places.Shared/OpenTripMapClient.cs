using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RedCorners.Components
{
    public class OpenTripMapClient
    {
        public const string BaseUrl = "https://api.opentripmap.com/0.1/en/places/";
        public string ApiKey { get; set; }
        readonly RestClient client;

        public OpenTripMapClient(string apiKey)
        {
            client = new RestClient(BaseUrl);
            this.ApiKey = apiKey;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="radius">in meters</param>
        /// <param name="rate"></param>
        /// <returns></returns>
        public async Task<OpenTripMapSimpleFeature[]> GetFeaturesAsync(
            double radius, 
            double lat, 
            double lon,
            string name = null,
            string kinds = null,
            string rate = null,
            int? limit = null)
        {
            var request = new RestRequest("radius", Method.Get);
            request.AddQueryParameter("apikey", ApiKey);
            request.AddQueryParameter("radius", radius.ToString());
            request.AddQueryParameter("lat", lat.ToString());
            request.AddQueryParameter("lon", lon.ToString());
            if (name != null) request.AddQueryParameter("name", name);
            if (kinds != null) request.AddQueryParameter("kinds", kinds);
            if (rate != null) request.AddQueryParameter("rate", rate);
            if (limit != null) request.AddQueryParameter("limit", limit.ToString());
            request.AddQueryParameter("format", "json");
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var results = JsonConvert.DeserializeObject<SimpleFeature[]>(response.Content);
            return Convert(results);
        }

        public async Task<OpenTripMapSimpleFeature[]> GetFeaturesAsync(
            double lat_min,
            double lat_max,
            double lon_min,
            double lon_max,
            string name = null,
            string kinds = null,
            string rate = null,
            int? limit = null)
        {
            var request = new RestRequest("bbox", Method.Get);
            request.AddQueryParameter("apikey", ApiKey);
            request.AddQueryParameter("lon_min", lon_min.ToString());
            request.AddQueryParameter("lon_max", lon_max.ToString());
            request.AddQueryParameter("lat_min", lat_min.ToString());
            request.AddQueryParameter("lat_max", lat_max.ToString());
            if (name != null) request.AddQueryParameter("name", name);
            if (kinds != null) request.AddQueryParameter("kinds", kinds);
            if (rate != null) request.AddQueryParameter("rate", rate);
            if (limit != null) request.AddQueryParameter("limit", limit.ToString());
            request.AddQueryParameter("format", "json");
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var results = JsonConvert.DeserializeObject<SimpleFeature[]>(response.Content);
            return Convert(results);
        }

        public enum Props
        {
            Base,
            Address
        }

        public async Task<OpenTripMapSimpleFeature[]> GetFeaturesAsync(
            double radius,
            double lat,
            double lon,
            string name,
            Props props = Props.Base,
            string kinds = null,
            string rate = null,
            int? limit = null)
        {
            var request = new RestRequest("bbox", Method.Get);
            request.AddQueryParameter("apikey", ApiKey);
            request.AddQueryParameter("name", name);
            request.AddQueryParameter("radius", radius.ToString());
            request.AddQueryParameter("lon", lon.ToString());
            request.AddQueryParameter("lat", lat.ToString());
            request.AddQueryParameter("props", props.ToString().ToLower());
            if (kinds != null) request.AddQueryParameter("kinds", kinds);
            if (rate != null) request.AddQueryParameter("rate", rate);
            if (limit != null) request.AddQueryParameter("limit", limit.ToString());
            request.AddQueryParameter("format", "json");
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var results = JsonConvert.DeserializeObject<SimpleFeature[]>(response.Content);
            return Convert(results);
        }

        OpenTripMapSimpleFeature[] Convert(SimpleFeature[] results)
        {
            return results.Select(x => new OpenTripMapSimpleFeature
            {
                Distance = x.dist,
                Kinds = x.kinds?.Split(','),
                Latitude = x.point.lat,
                Longitude = x.point.lon,
                Name = x.name,
                OSM = x.osm,
                Rate = x.rate,
                Wikidata = x.wikidata,
                XId = x.xid
            }).ToArray();
        }

        public async Task<OpenTripMapGeoName> GetGeoNameAsync(string query)
        {
            var request = new RestRequest("geoname", Method.Get);
            request.AddQueryParameter("apikey", ApiKey);
            request.AddQueryParameter("name", query);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            return JsonConvert.DeserializeObject<OpenTripMapGeoName>(response.Content);
        }

        public async Task<OpenTripMapPlace> GetPlaceAsync(string xid)
        {
            var request = new RestRequest($"xid/{xid}", Method.Get);
            request.AddQueryParameter("apikey", ApiKey);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return null;

            var result = JsonConvert.DeserializeObject<X>(response.Content);
            return new OpenTripMapPlace
            {
                Address = result.address,
                Image = result.image,
                Latitude = result.point.lat,
                Longitude = result.point.lon,
                Name = result.name,
                OSM = result.osm,
                Preview = result.preview,
                Rate = result.rate,
                Wikipedia = result.wikidata,
                Kinds = result.kinds?.Split(','),
                OTM = result.otm,
                Wikidata = result.wikidata,
                WikipediaExtracts = result.wikipedia_extracts,
                XId = result.xid
            };
        }

        class SimpleFeature
        {
            public string xid { get; set; }
            public string name { get; set; }
            public double dist { get; set; }
            public string rate { get; set; }
            public string wikidata { get; set; }
            public string kinds { get; set; }
            public Point point { get; set; }
            public string osm { get; set; }
        }

        class X
        {
            public string xid { get; set; }
            public string name { get; set; }
            public Address address { get; set; }
            public string rate { get; set; }
            public string osm { get; set; }
            public string wikidata { get; set; }
            public string kinds { get; set; }
            public string otm { get; set; }
            public string wikipedia { get; set; }
            public string image { get; set; }
            public Preview preview { get; set; }
            public WikipediaExtracts wikipedia_extracts { get; set; }
            public Point point { get; set; }
        }

        public class Preview
        {
            public string Source { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
        }

        public class WikipediaExtracts
        {
            public string Title { get; set; }
            public string Text { get; set; }
            public string Html { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
            public string Road { get; set; }
            public string County { get; set; }
            public string Suburb { get; set; }
            public string Country { get; set; }
            [JsonProperty("postcode")] public string PostCode { get; set; }
            [JsonProperty("country_code")] public string CountryCode { get; set; }
        }

        class Point
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }
    }
}


//[
//    {
//        "xid": "Q4061036",
//        "name": "Aleksander Nevski kabel",
//        "dist": 19.47494208,
//        "rate": 3,
//        "wikidata": "Q4061036",
//        "kinds": "religion,churches,interesting_places,other_churches",
//        "point": {
//            "lon": 24.753611,
//            "lat": 59.43679
//        }
//    },
//    {
//        "xid": "W543642600",
//        "name": "Tammsaare park",
//        "dist": 109.91797108,
//        "rate": 7,
//        "osm": "way/543642600",
//        "wikidata": "Q12376309",
//        "kinds": "gardens_and_parks,urban_environment,cultural,interesting_places",
//        "point": {
//            "lon": 24.753122,
//            "lat": 59.435997
//        }
//    },
//    {
//        "xid": "W26903304",
//        "name": "Tallinna adventkirik",
//        "dist": 115.45212606,
//        "rate": 7,
//        "osm": "way/26903304",
//        "wikidata": "Q18625739",
//        "kinds": "religion,churches,interesting_places,other_churches",
//        "point": {
//            "lon": 24.752964,
//            "lat": 59.437954
//        }
//    },
//    {
//        "xid": "N3116991450",
//        "name": "A. H. Tammsaare",
//        "dist": 163.79116071,
//        "rate": 2,
//        "osm": "node/3116991450",
//        "wikidata": "Q12357982",
//        "kinds": "historic,monuments_and_memorials,interesting_places,monuments",
//        "point": {
//            "lon": 24.752844,
//            "lat": 59.435532
//        }
//    },
//    {
//        "xid": "N1453568182",
//        "name": "Russian Culture Centre",
//        "dist": 173.56152341,
//        "rate": 7,
//        "osm": "node/1453568182",
//        "wikidata": "Q18626770",
//        "kinds": "cultural,theatres_and_entertainments,interesting_places,other_theatres",
//        "point": {
//            "lon": 24.75333,
//            "lat": 59.438515
//        }
//    },
//    {
//        "xid": "R6522415",
//        "name": "Viru Gate",
//        "dist": 193.69515358,
//        "rate": 3,
//        "osm": "relation/6522415",
//        "wikidata": "Q12378759",
//        "kinds": "fortifications,defensive_walls,historic,interesting_places",
//        "point": {
//            "lon": 24.750261,
//            "lat": 59.436459
//        }
//    },
//    {
//        "xid": "W8073173",
//        "name": "Viru Hill",
//        "dist": 201.83468356,
//        "rate": 7,
//        "osm": "way/8073173",
//        "wikidata": "Q12378761",
//        "kinds": "mountain_peaks,gardens_and_parks,urban_environment,cultural,interesting_places,natural,geological_formations",
//        "point": {
//            "lon": 24.750439,
//            "lat": 59.436066
//        }
//    },
//    {
//        "xid": "N645948806",
//        "name": "Kinomaja",
//        "dist": 216.64104211,
//        "rate": 2,
//        "osm": "node/645948806",
//        "wikidata": "Q12367001",
//        "kinds": "cultural,cinemas,theatres_and_entertainments,interesting_places",
//        "point": {
//            "lon": 24.749929,
//            "lat": 59.437607
//        }
//    },
//    {
//        "xid": "N6495720485",
//        "name": "Hellemann Tower",
//        "dist": 221.37776637,
//        "rate": 2,
//        "osm": "node/6495720485",
//        "wikidata": "Q15434302",
//        "kinds": "fortifications,historic,interesting_places,fortified_towers",
//        "point": {
//            "lon": 24.749762,
//            "lat": 59.437473
//        }
//    },
//    {
//        "xid": "Q12360650",
//        "name": "Coca-Cola Plaza",
//        "dist": 225.49748879,
//        "rate": 2,
//        "wikidata": "Q12360650",
//        "kinds": "cinemas,cultural,theatres_and_entertainments,interesting_places",
//        "point": {
//            "lon": 24.756626,
//            "lat": 59.438229
//        }
//    },
//    {
//        "xid": "N710538469",
//        "name": "Hellemani Korts",
//        "dist": 226.62438608,
//        "rate": 2,
//        "osm": "node/710538469",
//        "wikidata": "Q15434302",
//        "kinds": "fortifications,historic,interesting_places,restaurants,foods,tourist_facilities,fortified_towers",
//        "point": {
//            "lon": 24.749683,
//            "lat": 59.437508
//        }
//    },
//    {
//        "xid": "W26873867",
//        "name": "Estonia Concert Hall",
//        "dist": 267.23419391,
//        "rate": 3,
//        "osm": "way/26873867",
//        "wikidata": "Q583997",
//        "kinds": "cultural,theatres_and_entertainments,interesting_places,other_theatres",
//        "point": {
//            "lon": 24.751268,
//            "lat": 59.434856
//        }
//    },
//    {
//        "xid": "R3519086",
//        "name": "Munkadetagune torn",
//        "dist": 275.44134321,
//        "rate": 3,
//        "osm": "relation/3519086",
//        "wikidata": "Q12370551",
//        "kinds": "towers,architecture,fortifications,historic,interesting_places,other_towers,fortified_towers",
//        "point": {
//            "lon": 24.749599,
//            "lat": 59.438412
//        }
//    },
//    {
//        "xid": "R2163949",
//        "name": "Estonia teatrihoone",
//        "dist": 280.62736025,
//        "rate": 7,
//        "osm": "relation/2163949",
//        "wikidata": "Q11704264",
//        "kinds": "cultural,theatres_and_entertainments,interesting_places,opera_houses",
//        "point": {
//            "lon": 24.751215,
//            "lat": 59.434734
//        }
//    },
//    {
//        "xid": "W26885900",
//        "name": "Peeter-Pauli Katedraal",
//        "dist": 286.00910495,
//        "rate": 7,
//        "osm": "way/26885900",
//        "wikidata": "Q2322737",
//        "kinds": "religion,churches,cathedrals,interesting_places,catholic_churches",
//        "point": {
//            "lon": 24.748943,
//            "lat": 59.438026
//        }
//    },