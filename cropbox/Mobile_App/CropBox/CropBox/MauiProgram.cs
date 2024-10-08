using CropBox.Services;
using CropBox.Views.Owner;
using CropBox.Views.Shared;
using CropBox.Views.Technician;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Reflection;

namespace CropBox;

public static class MauiProgram
{
    private static Thread thread;
    private static Task task;
    public static IServiceProvider Services { get; private set; }
    public static MauiApp CreateMauiApp()
	{
            var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseLocalNotification()
            .UseSkiaSharp()
			.UseMauiMaps()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream("CropBox.appsettings.json");

        var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();
        builder.Configuration.AddConfiguration(config);
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<InteractionPage>();
        builder.Services.AddSingleton<SetTresholdsPage>();
        builder.Services.AddSingleton<MapPage>();
        //builder.Services.AddSingleton<DataPage>();
        builder.Services.AddSingleton<NotificationHelper>();
#if DEBUG
        builder.Logging.AddDebug();
        var app = builder.Build();
        Services = app.Services;

#endif

        return app;
    }
}
