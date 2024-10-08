using CropBox.Enums;
using CropBox.Repos;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace CropBox.Views.Owner;

/// <summary>
/// Team name : CropBox  
/// Team number : F
/// Winter 4/28/2023 
/// 420-6A6-AB
///  Map page is used to show the current location of the container
/// </summary>
public partial class MapPage : ContentPage
{
    private Thread thread;
    private Task task;
    const int DISTANCE_FROM_MIDDLE = 5;
    private bool IsTreadActive = false;
    /// <summary>
    ///  MapPage constructor is used to initialize the MapPage
    /// </summary>
    public MapPage()
	{
		InitializeComponent();
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {

    }
    /// <summary>
    /// OnNavigatedTo is used to show the current location of the container
    /// </summary>
    /// <param name="args"> args is NavigationEventArgs represent the event arguments</param>
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        if (IsTreadActive == false)
        {
            IsTreadActive = true;
            this.thread = new Thread(() =>
            {
                while (true)
                {
                    task = Device.InvokeOnMainThreadAsync(() =>
                    {
                        string coordinates = App.telemetryRepo.Readings.Last(r => r.Type == ReadingTypes.Coordinates.ToString()).Value;

                        JObject parsedCoordinates = JObject.Parse(coordinates);

                        string latitude = (string)parsedCoordinates[ReadingTypes.Latitude.ToString()];
                        string longitude = (string)parsedCoordinates[ReadingTypes.Longitude.ToString()];


                        var hanaLoc = new Location(float.Parse(latitude), float.Parse(longitude));

                        MapSpan mapSpan = MapSpan.FromCenterAndRadius(hanaLoc, Distance.FromKilometers(DISTANCE_FROM_MIDDLE));
                        map.MoveToRegion(mapSpan);
                        map.Pins.Add(new Pin
                        {
                            Label = "Your container's current location.",
                            Location = hanaLoc,
                        });
                    });
                    Thread.Sleep(App.telemetryHelper.TelemetryInterval);
                }
            });
            this.thread.Start();
        }

    }
    private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            // Handle the network connection loss here
            // You can display an error message or perform any other necessary action
            await DisplayAlert("Network Error", "Connection lost. Please check your network settings.", "OK");
        }
    }
}