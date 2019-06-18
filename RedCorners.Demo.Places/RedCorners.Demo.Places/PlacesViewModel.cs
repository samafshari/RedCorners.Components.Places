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
        Components.Places places = new Components.Places();

        string _query = "";
        public string Query
        {
            get => _query;
            set => SetProperty(ref _query, value);
        }

        string _results = "";
        public string Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        public PlacesViewModel()
        {

        }

        public Command SearchCommand => new Command(async () =>
        {
            var results = await places.SearchAsync(Query);
            Results = string.Join("\n", results.Select(x => x.ToString()));
        });
    }
}
