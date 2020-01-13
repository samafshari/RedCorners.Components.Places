using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using RedCorners;
using RedCorners.Forms.GoogleMaps;
using System.Threading.Tasks;
using RedCorners.Components;

namespace RedCorners.Demo.Places
{
    public class OverpassViewModel : BindableModel
    {
        public OverpassViewModel()
        {
            Status = TaskStatuses.Success;
        }

        OverpassClient client = new OverpassClient();
        Dictionary<string, AmenityNode> Nodes = new Dictionary<string, AmenityNode>();
        public List<Pin> Items { get; set; } = new List<Pin>();

        volatile string lastRegion = "";
        DateTime lastFetch;
        MapRegion region;

        public Action<MapRegion> RegionChangeAction => (region) =>
        {
            this.region = region;
            
        };

        public Command MapIdledCommand => new Command(() =>
        {
            if (region == null) return;
            var newRegion = $"{region.FarLeft.GetHashCode()}{region.FarRight.GetHashCode()}{region.NearLeft.GetHashCode()}{region.NearRight.GetHashCode()}";
            if (lastRegion == newRegion) return;
            if (DateTime.Now - lastFetch < TimeSpan.FromSeconds(2)) return;
            lastRegion = newRegion;
            lastFetch = DateTime.Now;
            Task.Run(() => FetchAsync(region));
        });

        async Task FetchAsync(MapRegion region)
        {
            const double MaxRadius = 20;

            Console.WriteLine($"Querying {lastFetch}");
            AmenityNode[] nodes = null;

            var centerLat = 0.5 * (region.NearLeft.Latitude + region.FarRight.Latitude);
            var centerLng = 0.5 * (region.NearLeft.Longitude + region.FarRight.Longitude);

            if (MaxRadius < OverpassClient.GetDiameter(region.NearLeft.Latitude,
                region.NearLeft.Longitude,
                region.FarRight.Latitude,
                region.FarRight.Longitude))
            {
                nodes = await client.SearchAmenityNodes(new[] { "recycling" }, centerLat, centerLng, MaxRadius);
            }
            else
            {
                nodes = await client.SearchAmenityNodes(new[] { "recycling" },
                    region.NearLeft.Latitude,
                    region.NearLeft.Longitude,
                    region.FarRight.Latitude,
                    region.FarRight.Longitude);
            }
            foreach (var node in nodes)
            {
                Nodes[node.Id] = node;
            }

            var toVisualize = Nodes.Values.OrderBy(x => OverpassClient.GetDistance(
                centerLat,
                centerLng,
                x.Latitude,
                x.Longitude)).Take(1000).ToArray();

            App.Instance.RunOnUI(() =>
            {
                Items = toVisualize.Select(x => new Pin
                {
                    Label = "Pin",
                    Address = "Pin",
                    Position = new Position(x.Latitude, x.Longitude)
                }).ToList();
                UpdateProperties();
            });
        }


    }
}
