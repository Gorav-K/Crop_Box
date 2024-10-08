using Plugin.LocalNotification;
using Microsoft.Azure.Devices.Shared;
using CropBox.Enums;
using CropBox.Models;

namespace CropBox.Services
{
    public class NotificationHelper
    {
        public int notificationInterval = 60000;
        public Timer Timer;
        public async Task StartNotificationBackgroundTask()
        {
            try
            {
                ReadingTypes[] readingTypes = new ReadingTypes[] { ReadingTypes.Pitch, ReadingTypes.Roll, ReadingTypes.Vibration, ReadingTypes.Luminosity, ReadingTypes.WaterDepth, ReadingTypes.Temperature, ReadingTypes.Humidity, ReadingTypes.Moisture };
                Thresholds[] thresholds = new Thresholds[] { Thresholds.pitchThreshold, Thresholds.rollThreshold, Thresholds.vibrationThreshold, Thresholds.luminosityThreshold, Thresholds.waterDepthThreshold, Thresholds.temperatureThreshold, Thresholds.humidityThreshold, Thresholds.moistureThreshold };

                    App.telemetryHelper.twin = await App.telemetryHelper.registryManager.GetTwinAsync(App.Settings.DeviceId);   // get twin
                    TwinCollection desiredProperties = App.telemetryHelper.twin.Properties.Desired;                             // get desired properties

                    for (int i = 0; i < readingTypes.Length; i++)
                    {
                        double reading = double.Parse(App.telemetryRepo.Readings.LastOrDefault(r => r.Type == readingTypes[i].ToString(), new Reading("", "unit", "0")).Value);
                        if (reading >= (int)desiredProperties[thresholds[i].ToString()])
                        {
                            await SendNotification($"{readingTypes[i]} Over Threshold Value!", "", "");
                            return;
                        }
                        else if (reading >= App.telemetryHelper.CRITICAL_RANGE * (double)desiredProperties[thresholds[i].ToString()])
                        {
                            await SendNotification($"{readingTypes[i]} Within Critical Range!", "", "");
                            return;
                        }
                    }

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! " + e.Message);
            }
        }
        public async Task SendNotification(string title, string subtitle, string description)
        {
            var request = new NotificationRequest
            {
                Title = title,
                Subtitle = subtitle,
                Description = description,
            };
            await LocalNotificationCenter.Current.Show(request);
        }
        public void RequestPushNotificationPermission()
        {
            var activity = Platform.CurrentActivity;
            if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(activity.ApplicationContext, Android.Manifest.Permission.PostNotifications) != Android.Content.PM.Permission.Granted)
            {
                AndroidX.Core.App.ActivityCompat.RequestPermissions(activity, new[] { Android.Manifest.Permission.PostNotifications }, 0);
            }
        }
    }
}
