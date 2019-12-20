using RedCorners.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using RedCorners.Components;
using System.Linq;

namespace RedCorners.Demo.Places
{
    public class PlacesViewModel : BindableModel
    {
        IPlaces places = new HerePlaces(Vars.HereAppId, Vars.HereAppCode);

        public List<Place> Results { get; set; } = new List<Place>();

        string _query = "";
        public string Query
        {
            get => _query;
            set => SetProperty(ref _query, value);
        }

        public PlacesViewModel()
        {
            UpdateProvider(0);
            Status = Models.TaskStatuses.Success;
            UpdateProperties();
        }
         
        public Command SearchCommand => new Command(async () =>
        {
            if (places == null)
            {
                Results = new List<Place> { new Place { Name = "No Provider Selected." } };
                UpdateProperties();
                return;
            }

            Status = Models.TaskStatuses.Busy;
            UpdateProperties();

            if (places is NominatimPlaces)
                Results = await places.SearchAsync(Query);
            else
                Results = await places.SearchAsync(Query, Vars.CenterLatitude, Vars.CenterLongitude);

            if (places is GooglePlaces g)
            {
                var p = Results[0].Tag as GooglePlace;
                if (p != null)
                await g.QueryAddressComponentsAsync(p.PlaceId);
            }
            if (Results == null)
                Results = new List<Place> { new Place { Name = "Null" } };
            Status = Models.TaskStatuses.Success;
            UpdateProperties();
        });

        public Command<int> PlaceChangeCommand => new Command<int>(i =>
        {
            UpdateProvider(i);
        });

        void UpdateProvider(int i)
        {
            // 0=mapkit
            // 1=here
            // 2=google
            // 3=osm
            if (i == 0)
            {
#if __IOS__
                places = new MapKitPlaces();
#else
                places = null;
#endif
            }
            else if (i == 1)
            {
                places = new HerePlaces(Vars.HereAppId, Vars.HereAppCode);
            }
            else if (i == 2)
            {
                places = new GooglePlaces(Vars.GoogleApiKey);
            }
            else if (i == 3)
            {
                places = new NominatimPlaces();
            }
            else if (i == 4)
            {
#if __IOS__
                places = new MapKitLocalSearchPlaces();
#else
                places = null;
#endif
            }
        }
    }
}
