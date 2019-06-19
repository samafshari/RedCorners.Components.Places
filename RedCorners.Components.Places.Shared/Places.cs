using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedCorners.Components
{
    public interface IPlaces
    {
        Task<List<Place>> SearchAsync(string query);
        Task<List<Place>> SearchAsync(string query, double centerLatitude, double centerLongitude);
    }
}
