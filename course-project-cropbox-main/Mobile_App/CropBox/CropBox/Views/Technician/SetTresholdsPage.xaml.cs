using CropBox.Enums;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Plugin.LocalNotification;

namespace CropBox.Views.Technician;

/// <summary>
/// Team name : CropBox  
/// Team number : F
/// Winter 4/28/2023 
/// 420-6A6-AB
/// SetTresholdsPage class is used to set the thresholds for the sensors
/// </summary>
public partial class SetTresholdsPage : ContentPage
{
    public static float temperatureThreshold = 0;
    public static float humidityThreshold = 0;
    public static float waterDepthThreshold = 0;
    public static float moistureThreshold = 0;
    public static float telemetryInterval = 0;
    
    private Thread thread;
    /// <summary>
    /// Initialize the SetTresholdsPage
    /// </summary>
	public SetTresholdsPage()
	{
		InitializeComponent();
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }
    /// <summary>
    /// temperature_slider_ValueChanged is used to set the temperature threshold
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private void temperature_slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {

        temperatureThreshold = float.Parse(temperature_text.Text);
        temperature_text.Text = ((int)temperature_slider.Value).ToString();
        App.telemetryRepo.DynamicThresholds[ReadingTypes.Temperature.ToString()] = temperatureThreshold;
    }
    /// <summary>
    /// humidity_slider_ValueChanged is used to set the humidity threshold
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private void humidity_slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        humidityThreshold = float.Parse(humidity_text.Text);
        humidity_text.Text = ((int)humidity_slider.Value).ToString();
        App.telemetryRepo.DynamicThresholds[ReadingTypes.Humidity.ToString()] = humidityThreshold;
    }

    /// <summary>
    /// water_slider_ValueChanged is used to set the water threshold
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private void water_slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        waterDepthThreshold = (float)water_slider.Value;
        water_text.Text = ((int)water_slider.Value).ToString();
        App.telemetryRepo.DynamicThresholds[ReadingTypes.WaterDepth.ToString()] = waterDepthThreshold;
    }
    /// <summary>
    /// moisture_slider_ValueChanged is used to set the moisture threshold
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private void moisture_slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        moistureThreshold = float.Parse(moisture_text.Text);
        moisture_text.Text = ((int)moisture_slider.Value).ToString();
        App.telemetryRepo.DynamicThresholds[ReadingTypes.Moisture.ToString()] = moistureThreshold;
    }
    private void telemetryInterval_slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        telemetryInterval_text.Text = ((int)telemetryInterval_slider.Value).ToString();
    }
    private async Task UpdateDesiredProperties()
    {
        try
        {
            App.telemetryHelper.twin = await App.telemetryHelper.registryManager.GetTwinAsync(App.Settings.DeviceId);   // get twin
            TwinCollection desiredProperties = App.telemetryHelper.twin.Properties.Desired;                             // get desired properties
            
            desiredProperties[Thresholds.telemetryInterval.ToString()] = (int)telemetryInterval_slider.Value;           // set the desired property
            desiredProperties[Thresholds.temperatureThreshold.ToString()] = (int)temperature_slider.Value;              
            desiredProperties[Thresholds.humidityThreshold.ToString()] = (int)humidity_slider.Value;                    
            desiredProperties[Thresholds.waterDepthThreshold.ToString()] = (int)water_slider.Value;                     
            desiredProperties[Thresholds.moistureThreshold.ToString()] = (int)moisture_slider.Value;                    

            await App.telemetryHelper.registryManager.UpdateTwinAsync(App.Settings.DeviceId, App.telemetryHelper.twin, App.telemetryHelper.twin.ETag);  // update the twin    
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private async void UpdateReportedProperties(TwinCollection reportedProperties, TwinCollection desiredProperties)
    {
        foreach (Thresholds threshold in (Thresholds[])Enum.GetValues(typeof(Thresholds)))
        {
            try
            {
                if (!desiredProperties.Contains(threshold.ToString()))
                    throw new Exception("Desired properties does not contain property " + threshold.ToString());

                if (reportedProperties[threshold.ToString()] == null)
                    throw new Exception("Reported properties does not contain property " + threshold.ToString());
                var reportedProperty = new TwinCollection("{ \"" + threshold.ToString() + "\" : " + desiredProperties[threshold.ToString()] + " }");
                await App.telemetryHelper.deviceClient.UpdateReportedPropertiesAsync(reportedProperty);
            } 
            catch (Exception e)
            {
                var reportedProperty = new TwinCollection("{ \"" + threshold.ToString() + "\" : " + desiredProperties[threshold.ToString()] + " }");
                await App.telemetryHelper.deviceClient.UpdateReportedPropertiesAsync(reportedProperty);
            }
        }
    }

    private void SetSliderValues(TwinCollection desiredProperties)
    {
        foreach (Thresholds threshold in (Thresholds[])Enum.GetValues(typeof(Thresholds)))
        {
            try
            {
                if (!desiredProperties.Contains(threshold.ToString()))
                    throw new Exception("Desired properties does not contain property " + threshold.ToString());
                if (desiredProperties[threshold.ToString()] == null)
                    throw new Exception("Desired properties does not contain property " + threshold.ToString());
                switch (threshold)
                {
                    case Thresholds.temperatureThreshold:
                        temperature_slider.Value = (int)desiredProperties[threshold.ToString()];
                        temperature_text.Text = ((int)temperature_slider.Value).ToString();
                        break;
                    case Thresholds.humidityThreshold:
                        humidity_slider.Value = (int)desiredProperties[threshold.ToString()];
                        humidity_text.Text = ((int)humidity_slider.Value).ToString();
                        break;
                    case Thresholds.waterDepthThreshold:
                        water_slider.Value = (int)desiredProperties[threshold.ToString()];
                        water_text.Text = ((int)water_slider.Value).ToString();
                        break;
                    case Thresholds.moistureThreshold:
                        moisture_slider.Value = (int)desiredProperties[threshold.ToString()];
                        moisture_text.Text = ((int)moisture_slider.Value).ToString();
                        break;
                    case Thresholds.telemetryInterval:
                        telemetryInterval_slider.Value = (int)desiredProperties[threshold.ToString()];
                        telemetryInterval_text.Text = ((int)telemetryInterval_slider.Value).ToString();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        App.telemetryHelper.twin = await App.telemetryHelper.registryManager.GetTwinAsync(App.Settings.DeviceId);
        TwinCollection desiredProperties = App.telemetryHelper.twin.Properties.Desired;

        SetSliderValues(desiredProperties);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await UpdateDesiredProperties();

    }
    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            // Handle the network connection loss here
            // You can display an error message or perform any other necessary action
            DisplayAlert("Network Error", "Connection lost. Please check your network settings.", "OK");
        }
    }
}