using System;
using System.Collections.Generic;
using System.Text;

namespace RedCorners.Components
{
    public class GooglePlace
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Icon { get; set; }
        public string Id { get; set; }
        public double Rating { get; set; }
        public string[] Types { get; set; }
        public int UserRatingsTotal { get; set; }
        public string PlaceId { get; set; }
    }
}
