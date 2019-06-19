# RedCorners.Components.Places

NuGet: [https://www.nuget.org/packages/RedCorners.Components.Places](https://www.nuget.org/packages/RedCorners.Components.Places)

GitHub: [https://github.com/saeedafshari/RedCorners.Components.Places](https://github.com/saeedafshari/RedCorners.Components.Places)

This library provides a unified interface for performing a places search. It also contains the implementations for the following APIs:

- MapKit Places (available only on the iOS)
- HERE Places API
- Google Places API
- Open Street Maps (Nominatim) 

To perform a places search, you have to instantiate one of the implementations and call its `SearchAsync` method:

```c#
// MapKit API (iOS Only)
IPlaces places = new MapKitPlaces();

// Google Places API
IPlaces places = new GooglePlaces(Vars.GoogleApiKey);

// HERE API
IPlaces places = new HerePlaces(Vars.HereAppId, Vars.HereAppCode);

// OpenStreetMaps (Nominatim) API
IPlaces places = new NominatimPlaces();

// Query
IEnumerable<Place> results = await places.SearchAsync("IKEA");

// Query around a location
IEnumerable<Place> results = await places.SearchAsync("IKEA", 49.6232369, 6.0708212);
```

The results are returned as a list of `Place` objects:

```c#
public class Place
{
	public string Name { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public bool HasCoordinates { get; set; }
	public string Address { get; set; }
	public object Tag { get; set; }
}
```

If you need to access more provider-dependent fields, you can access the original object via the `Tag` property. Depending on which implementation you use, `Tag` contains a:

- `MKMapItem` (when `MapKitPlaces` is used)
- `GooglePlace` (when `GooglePlaces` is used)
- `HerePlace` (when `HerePlaces` is used)
- `NominatimPlace` (when `NominatimPlaces` is used)

Just cast it as the corresponding type and use it. e.g.: `(Place.Tag as NominatimPlace).OsmId`

These classes are defined as follows:

#### MkMapItem
See [https://docs.microsoft.com/en-us/dotnet/api/mapkit.mkmapitem?view=xamarin-ios-sdk-12](https://docs.microsoft.com/en-us/dotnet/api/mapkit.mkmapitem?view=xamarin-ios-sdk-12)

#### GooglePlace
```c#
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
```

#### HerePlace
```c#
public class HerePlace
{
	public string Title { get; set; }
	public string HighlightedTitle { get; set; }
	public string Vicinity { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string Category { get; set; }
	public string Href { get; set; }
	public string Type { get; set; }
}
```

#### NominatimPlace
```c#
public class NominatimPlace
{
	public string PlaceId { get; set; }
	public string OsmType { get; set; }
	public string OsmId { get; set; }
	public string Type { get; set; }
	public string Label { get; set; }
	public string Name { get; set; }
	public string Street { get; set; }
	public string PostCode { get; set; }
	public string County { get; set; }
	public string State { get; set; }
	public string Country { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public string HouseNumber { get; set; }
}
```