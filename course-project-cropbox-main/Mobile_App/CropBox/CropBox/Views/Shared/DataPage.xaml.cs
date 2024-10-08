using CropBox.Enums;
using CropBox.Models;
using CropBox.Repos;
using CropBox.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using MedalTracker.Repos;
using Microsoft.Azure.Devices.Shared;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CropBox.Views.Shared;

/// <summary>
/// Team name : CropBox  
/// Team number : F
/// Winter 4/28/2023 
/// 420-6A6-AB
/// Data page is used to display the data of the sensors
/// </summary>
public partial class DataPage : ContentPage
{
    public static bool pickerUpdated = false;
    private const double CHART_SCALE = 1.25;
    private Thread thread;
    private Task task;
    private CancellationTokenSource cancellationTokenSource;
    private CancellationToken cancellationToken;

    private ReadingTypes readingType;
    ObservableValue threshold = new ObservableValue { Value = 50 };
    ObservableValue reading = new ObservableValue { Value = 80 };
    /// <summary>
    /// Options is used to store the options of the picker
    /// </summary>
    List<ISeries> series = new List<ISeries>();
    private ObservableCollection<ReadingTypes> Options = new ObservableCollection<ReadingTypes>();
    /// <summary>
    /// Series is used to set and get the series of the chart
    /// </summary>
    private List<ISeries> Series { get { return series; } set { series = value; } }

    /// <summary>
    /// DataPage constructor is used to initialize the DataPage
    /// </summary>
    public DataPage()
    {
        InitializeComponent();
        BindingContext = this;
        picker.ItemsSource = App.dataPickerRepo.GetUserBasedOptions(LoginPage.Route);
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <summary>
    /// PickerItemChanged method is used to set the chart series
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e"> e is EventArgs represent the event arguments</param>
    private async void PickerItemChanged(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        reading.Value = double.Parse(App.telemetryRepo.Readings.LastOrDefault(r => r.Type == picker.SelectedItem.ToString(), new Reading("", "unit", "0")).Value);

        App.telemetryHelper.twin = await App.telemetryHelper.registryManager.GetTwinAsync(App.Settings.DeviceId);
        TwinCollection desiredProperties = App.telemetryHelper.twin.Properties.Desired;

        string choice = picker.SelectedItem.ToString();
        threshold.Value = desiredProperties[char.ToLower(choice[0]) + choice.Substring(1) + "Threshold"];

        chart.Total = threshold.Value > reading.Value ? threshold.Value * CHART_SCALE: reading.Value * CHART_SCALE;
        thresholdLabel.Text = $"Current Threshold: {threshold.Value}";

        if (reading.Value < threshold.Value * App.telemetryHelper.CRITICAL_RANGE)
            chart.Series = ChartRepo.GetGaugeSeries(SKColors.YellowGreen, reading).ToList();
        else if (reading.Value < threshold.Value)
            chart.Series = ChartRepo.GetGaugeSeries(SKColors.Yellow, reading).ToList();
        else
            chart.Series = ChartRepo.GetGaugeSeries(SKColors.Red, reading).ToList();
    }

    /// <summary>
    ///  RefreshChart method is used to refresh the chart
    /// </summary>
    protected async Task RefreshCharts()
    {
        if (picker.SelectedItem == null) 
            picker.SelectedIndex= 0;
        reading.Value = double.Parse(App.telemetryRepo.Readings.LastOrDefault(r => r.Type == picker.SelectedItem.ToString(), new Reading("", "unit", "0")).Value);

        App.telemetryHelper.twin = await App.telemetryHelper.registryManager.GetTwinAsync(App.Settings.DeviceId);
        TwinCollection desiredProperties = App.telemetryHelper.twin.Properties.Desired;
        
        string choice = picker.SelectedItem.ToString();
        threshold.Value = desiredProperties[char.ToLower(choice[0]) + choice.Substring(1) + "Threshold"];


        chart.Total = threshold.Value > reading.Value ? threshold.Value * CHART_SCALE: reading.Value * CHART_SCALE;
        thresholdLabel.Text = $"Current Threshold: {threshold.Value}";

        if (reading.Value < threshold.Value * App.telemetryHelper.CRITICAL_RANGE)
            chart.Series = ChartRepo.GetGaugeSeries(SKColors.YellowGreen, reading).ToList();
        else if(reading.Value < threshold.Value)
            chart.Series = ChartRepo.GetGaugeSeries(SKColors.Yellow, reading).ToList();
        else
            chart.Series = ChartRepo.GetGaugeSeries(SKColors.Red, reading).ToList();

    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        var labelValues = new Label[] { lblFirstVal, lblSecondVal, lblThirdVal, lblFourthVal };

        UpdateReadings(labelValues, null, true);
        if (this.task.IsCompleted)
        {
            this.task.Dispose();
        }
    }
    private void UpdateReadings(Label[] labelValues, ReadingTypes[] readingTypes, bool clearFlag = false)
    {

        if (clearFlag)
        {
            for (int i = 0; i < labelValues.Length; i++)
                labelValues[i].Text = "";
            return;
        }
        for (int i = 0; i < labelValues.Length; i++)
        {
            string value = App.telemetryRepo.Readings.LastOrDefault(r => r.Type == readingTypes[i].ToString(), new Reading(readingTypes[i].ToString(), "unit", "")).ToString();
            labelValues[i].Text = value;
        }

    }
    /// <summary>
    /// OnNavigatedTo method that run when the page is navigated to
    /// </summary>
    /// <param name="args"> args is NavigatedToEventArgs represent the event arguments</param>
    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        var labels = new Label[] { lblFirst, lblSecond, lblThird, lblFourth };
        var labelValues = new Label[] { lblFirstVal, lblSecondVal, lblThirdVal, lblFourthVal };
        ReadingTypes[] readingTypes = new ReadingTypes[4];

        if ((int)LoginPage.Route == (int)UserTypes.Owner)
        {
            pickerUpdated = true;

            readingTypes = new ReadingTypes[] { ReadingTypes.Pitch, ReadingTypes.Roll, ReadingTypes.Vibration, ReadingTypes.Luminosity };

            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].Text = readingTypes[i].ToString();
            }
        }
        if ((int)LoginPage.Route == (int)UserTypes.Technician)
        {
            pickerUpdated = true;

            readingTypes = new ReadingTypes[] { ReadingTypes.WaterDepth, ReadingTypes.Temperature, ReadingTypes.Humidity, ReadingTypes.Moisture };

            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].Text = readingTypes[i].ToString();
            }
        }
            this.task = Device.InvokeOnMainThreadAsync(async () =>
            {

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (LoginPage.Route == UserTypes.Owner)
                    {
                        readingTypes = new ReadingTypes[] { ReadingTypes.Pitch, ReadingTypes.Roll, ReadingTypes.Vibration, ReadingTypes.Luminosity };
                        UpdateReadings(labelValues, readingTypes);
                    }
                    else if (LoginPage.Route == UserTypes.Technician)
                    {
                        readingTypes = new ReadingTypes[] { ReadingTypes.WaterDepth, ReadingTypes.Temperature, ReadingTypes.Humidity, ReadingTypes.Moisture };
                        UpdateReadings(labelValues, readingTypes);
                    }

                    await RefreshCharts();

                    await Task.Delay(App.telemetryHelper.TelemetryInterval);
                }

            });
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