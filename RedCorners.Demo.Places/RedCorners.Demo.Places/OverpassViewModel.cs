using System;
using System.Text;
using System.Linq;
using RedCorners.Forms;
using RedCorners.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using RedCorners;

namespace RedCorners.Demo.Places
{
    public class OverpassViewModel : BindableModel
    {
        public OverpassViewModel()
        {
            Status = TaskStatuses.Success;
        }
    }
}
