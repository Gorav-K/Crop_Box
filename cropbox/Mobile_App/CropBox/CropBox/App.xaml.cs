using CropBox.Config;
using CropBox.Enums;
using CropBox.Models;
using CropBox.Repos;
using CropBox.Services;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Extensions.DependencyInjection;

namespace CropBox;

public partial class App : Application
{
    public static Settings Settings { get; private set; }
    = MauiProgram.Services.GetService<IConfiguration>()
    .GetRequiredSection(nameof(Settings)).Get<Settings>();

    public static TelemetryRepo telemetryRepo;
    public static DataPickerRepo dataPickerRepo;

    public static DirectMethodHelper directMethodHelper;
    public static TelemetryHelper telemetryHelper;
    public static NotificationHelper notificationHelper;
    
    protected override void OnStart()
    {
        notificationHelper.RequestPushNotificationPermission();
    }
    public App()
	{
		InitializeComponent();
        MainPage = new AppShell();
        telemetryRepo= new TelemetryRepo();
        telemetryHelper = new TelemetryHelper();
        directMethodHelper = new DirectMethodHelper();
        notificationHelper = new NotificationHelper();
        dataPickerRepo = new DataPickerRepo();
    }
}
