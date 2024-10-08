using CropBox.Enums;
using CropBox.Models;
using CropBox.Repos;
using CropBox.Services;
using Microsoft.Maui.Animations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;

namespace CropBox.Views.Shared;
/// <summary>
/// Team name : CropBox  
/// Team number : F
/// Winter 4/28/2023 
/// 420-6A6-AB
///  InteractionPage class is used to represent the interaction page of command and alerts
/// </summary>
public partial class InteractionPage : ContentPage
{
    public ObservableCollection<string> Options { get; set; } = new ObservableCollection<string>();
    public ObservableCollection<Reading> Readings { get; set; }
    private Thread thread;
    private Task task;
    private bool IsTreadActive = false;
    const  string allReading = "All";
    private string pickerSelectedItem;
    private string pickerRGBSelectedItem;

    /// <summary>
    /// Interaction page constructor is used to initialize the interaction page
    /// </summary>
    public InteractionPage()
    {
        InitializeComponent();
        Readings = new ObservableCollection<Reading>();
        picker.ItemsSource = new ObservableCollection<string>() {
                            allReading,
                            ReadingTypes.Sound.ToString(),
                            ReadingTypes.Motion.ToString(),
                            ReadingTypes.RGBLED.ToString(),
                            ReadingTypes.Lock.ToString(),
                            ReadingTypes.Door.ToString(),
                            ReadingTypes.Buzzer.ToString(),
                            ReadingTypes.Fan.ToString()

                        };
        picker.SelectedIndex = 0;
        pickerSelectedItem = picker.SelectedItem.ToString();
        cvAlerts.ItemsSource = Readings;

        pickerRGBLED.ItemsSource = new List<string>()
        {
            RGBLEDcolor.white.ToString(),
            RGBLEDcolor.red.ToString(),
            RGBLEDcolor.green.ToString(),
            RGBLEDcolor.blue.ToString(),
            RGBLEDcolor.orange.ToString()
        };
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void PickerItemChanged(object sender, EventArgs e)
    {
        pickerSelectedItem = picker.SelectedItem.ToString();
        if (pickerSelectedItem == allReading)
        {
            // get Pitch, Roll, Vibration, Sound, Motion, Buzzer, Door, Lock, Fan, RGBLED reading from the database and order by timeStap in descending order


            cvAlerts.ItemsSource = new ObservableCollection<Reading>(App.telemetryRepo.Readings.Where(x => 
            x.Type == ReadingTypes.Sound.ToString() || x.Type == ReadingTypes.Motion.ToString() ||
            x.Type == ReadingTypes.Buzzer.ToString() || x.Type == ReadingTypes.Door.ToString() ||
            x.Type == ReadingTypes.Lock.ToString() || x.Type == ReadingTypes.Fan.ToString() ||
            x.Type == ReadingTypes.RGBLED.ToString()).OrderByDescending(x => x.TimeStamp));
            
        }
        else
        {
            cvAlerts.ItemsSource = new ObservableCollection<Reading>(App.telemetryRepo.Readings.Where(x => x.Type == pickerSelectedItem).OrderByDescending(x => x.TimeStamp));
        }
    }

    /// <summary>
    /// swtLed_Toggled method is used to add led alert to the alerts list
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private async void swtLed_Toggled(object sender, ToggledEventArgs e)
    {
        //App.dummyRepo.Alerts.Insert(0, new Models.Command(CommandTypes.RGBLED.ToString(), swtLed.IsToggled ? "ON" : "OFF"));
        //await App.directMethodHelper.InvokeMethodAsync(ValidCommands.actuate_rgbled, swtLed.IsToggled);
    }

    private async void btnRGBLED_Clicked(object sender, EventArgs e)
    {
        await App.directMethodHelper.InvokeMethodAsync(ValidCommands.actuate_rgbled);
    }

    /// <summary>
    /// swtDoor_Toggled method is used to add door alert to the alerts list
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private async void swtDoor_Toggled(object sender, ToggledEventArgs e)
    {
        await App.directMethodHelper.InvokeMethodAsync(ValidCommands.actuate_lock, swtDoor.IsToggled);
    }

    /// <summary>
    ///  swtBuzzer_Toggled method is used to add buzzer alert to the alerts list
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private async void swtBuzzer_Toggled(object sender, ToggledEventArgs e)
    {
        await App.directMethodHelper.InvokeMethodAsync(ValidCommands.actuate_buzzer, swtBuzzer.IsToggled);
    }
    /// <summary>
    ///  swtFan_Toggled method is used to add fan alert to the alerts list
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private async void swtFan_Toggled(object sender, ToggledEventArgs e)
    {
        await App.directMethodHelper.InvokeMethodAsync(ValidCommands.actuate_fan, swtFan.IsToggled);
    }

    private void LoadSwitchStates()
    {

        Switch[] switches = new Switch[]
        {
            swtDoor,
            swtBuzzer,
            swtFan
        };

        string[] actuatorStates = new string[]
        {
            App.telemetryRepo.Readings.LastOrDefault(r => r.Type == ReadingTypes.RGBLED.ToString(), new Reading(ReadingTypes.RGBLED.ToString(), "unit", "")).Value,
            App.telemetryRepo.Readings.LastOrDefault(r => r.Type == ReadingTypes.Lock.ToString(), new Reading(ReadingTypes.Lock.ToString(), "unit", "")).Value,
            App.telemetryRepo.Readings.LastOrDefault(r => r.Type == ReadingTypes.Buzzer.ToString(), new Reading(ReadingTypes.Buzzer.ToString(), "unit", "")).Value,
            App.telemetryRepo.Readings.LastOrDefault(r => r.Type == ReadingTypes.Fan.ToString(), new Reading(ReadingTypes.Fan.ToString(), "unit", "")).Value
        };

        bool[] switchStates = new bool[actuatorStates.Length];

        for (int i = 0; i < actuatorStates.Length; i++)
        {
            if (actuatorStates[i] != null && actuatorStates[i] != string.Empty)
            {
                string parsedState = actuatorStates[i].ToString();
                if (parsedState == "on")
                    switches[i].IsToggled = true;
            }
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        if ((int)LoginPage.Route == (int)UserTypes.Owner)
        {
            pickerRGBLED.IsVisible = false;
            btnRGBLED.IsVisible = false;
            rgbLabel.IsVisible = false;
            swtFan.IsVisible = false;
            fanLabel.IsVisible = false;
        }
        else
        {
            pickerRGBLED.IsVisible = true;
            btnRGBLED.IsVisible = true;
            rgbLabel.IsVisible = true;
            swtFan.IsVisible = true;
            fanLabel.IsVisible = true;
        }

        if (IsTreadActive == false)
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;

            LoadSwitchStates();
            IsTreadActive = true;
            this.thread = new Thread(() =>
            {
                while (true)
                {
                    this.task = Device.InvokeOnMainThreadAsync(async () =>
                    {

                        // base on the pickerSelectedItem, we will filter the readings and order by timeStap in descending order
                        if (pickerSelectedItem == allReading)
                        {
                            cvAlerts.ItemsSource = new ObservableCollection<Reading>(App.telemetryRepo.Readings.Where(x =>
                            x.Type == ReadingTypes.Sound.ToString() || x.Type == ReadingTypes.Motion.ToString() ||
                            x.Type == ReadingTypes.Buzzer.ToString() || x.Type == ReadingTypes.Door.ToString() ||
                            x.Type == ReadingTypes.Lock.ToString() || x.Type == ReadingTypes.Fan.ToString() ||
                            x.Type == ReadingTypes.RGBLED.ToString()).OrderByDescending(x => x.TimeStamp));
                        }
                        else
                        {
                            cvAlerts.ItemsSource = new ObservableCollection<Reading>(App.telemetryRepo.Readings.Where(x => x.Type == pickerSelectedItem).OrderByDescending(x => x.TimeStamp));
                        }
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

    private async void pickerRGBLED_SelectedIndexChanged(object sender, EventArgs e)
    {
        // get the selected item from the picker
        pickerRGBSelectedItem = pickerRGBLED.SelectedItem.ToString();
        string payload;
        string onOff = "on";

        payload = "{\"value\": \"" + onOff + "\", \"color\": " + "\"" + pickerRGBSelectedItem + "\"" + "}";

        // actuate the RGBLED based on the selected item
        await App.directMethodHelper.InvokeMethodAsync(ValidCommands.actuate_rgbled, false, payload);
    }

    
}