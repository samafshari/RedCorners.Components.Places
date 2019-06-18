using System;
using System.Collections.Generic;
using System.Text;
using RedCorners.Forms;
using Xamarin.Forms;

namespace RedCorners.Demo.Places
{
    public class App : Application2
    {
        public override Page GetFirstPage() =>
            new PlacesPage();
    }
}
