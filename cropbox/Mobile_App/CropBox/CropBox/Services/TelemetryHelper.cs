using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using CropBox.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using CropBox.Enums;

namespace CropBox.Services
{
    /// <summary>
    /// Code taken from Azure IOT SDK example   
    /// </summary>
    public class TelemetryHelper
    {
        public DeviceClient deviceClient;
        public const int EVENT_CHECKPOINTS = 50;
        public double CRITICAL_RANGE = 0.8;
        BlobContainerClient storageClient;
        public EventProcessorClient processor;
        ConcurrentDictionary<string, int> partitionEventCount;
        public RegistryManager registryManager;
        private int telemetryInterval = 5000;
        public Twin twin;
        public int TelemetryInterval
        {
            get {

                SetTelemetryInterval();
                if (this.telemetryInterval <= 5000)
                    return 5000;
                return this.telemetryInterval;
            }
            set { this.telemetryInterval = value * 1000; }
        }
        

        private async void SetTelemetryInterval()
        {
            App.telemetryHelper.twin = await App.telemetryHelper.registryManager.GetTwinAsync(App.Settings.DeviceId);   // get twin
            TwinCollection desiredProperties = App.telemetryHelper.twin.Properties.Desired;                             // get desired properties
            App.telemetryHelper.TelemetryInterval = desiredProperties[Thresholds.telemetryInterval.ToString()];         // set the telemetry interval   
        }

        public TelemetryHelper()
        {
            storageClient = new BlobContainerClient(App.Settings.StorageConnectionString, App.Settings.BlobContainerName);
            processor = new EventProcessorClient(
                storageClient,
                App.Settings.ConsumerGroup,
                App.Settings.EventHubConnectionString,
                App.Settings.EventHubName);
            partitionEventCount = new ConcurrentDictionary<string, int>();

            deviceClient = DeviceClient.CreateFromConnectionString(App.Settings.DeviceConnectionString);
            registryManager = RegistryManager.CreateFromConnectionString(App.Settings.HubConnectionString);
        }
        public async Task Run()
        {
            async Task processEventHandler(ProcessEventArgs args)
            {
                try
                {
                    if (args.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    string partition = args.Partition.PartitionId;
                    byte[] eventBody = args.Data.EventBody.ToArray();
                    string data = Encoding.UTF8.GetString(args.Data.Body.ToArray());

                    Console.WriteLine("IoT data: " + data);
                    JObject parsedData = JObject.Parse(data);

                    App.telemetryRepo.Readings.Enqueue(new Reading((string)parsedData[nameof(Reading.Type).ToLower()], 
                        (string)parsedData[nameof(Reading.Unit).ToLower()], (string)parsedData[nameof(Reading.Value).ToLower()]));


                    int eventsSinceLastCheckpoint = partitionEventCount.AddOrUpdate(
                        key: partition,
                        addValue: 1,
                        updateValueFactory: (_, currentCount) => currentCount + 1);

                    if (eventsSinceLastCheckpoint >= EVENT_CHECKPOINTS)
                    {
                        await args.UpdateCheckpointAsync();
                        partitionEventCount[partition] = 0;
                    }
                }
                catch
                {
                }
            }
            Task processErrorHandler(ProcessErrorEventArgs args)
            {
                try
                {
                    Console.WriteLine("Error in the EventProcessorClient");
                    Console.WriteLine($"\tOperation: {args.Operation}");
                    Console.WriteLine($"\tException: {args.Exception}");
                    Console.WriteLine("");
                }
                catch
                {
                }

                return Task.CompletedTask;
            }
            async Task NotificationLoop(CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await App.notificationHelper.StartNotificationBackgroundTask();

                    await Task.Delay(App.notificationHelper.notificationInterval, cancellationToken);
                }
            }
            try
            { 
                using var cancellationSource = new CancellationTokenSource();
                //cancellationSource.CancelAfter(TimeSpan.FromSeconds(10));

                processor.ProcessEventAsync += processEventHandler;
                processor.ProcessErrorAsync += processErrorHandler;

                try
                {
                    var runEveryMinuteTask = NotificationLoop(cancellationSource.Token);
                    await processor.StartProcessingAsync(cancellationSource.Token);
                    await Task.Delay(Timeout.Infinite, cancellationSource.Token);

                    cancellationSource.Cancel();
                    await runEveryMinuteTask;
                }
                catch (TaskCanceledException)
                {
                    // This is expected if the cancellation token is
                    // signaled.
                }
                finally
                {
                    // This may take up to the length of time defined
                    // as part of the configured TryTimeout of the processor;
                    // by default, this is 60 seconds.

                    await processor.StopProcessingAsync();
                }
            }
            catch
            {
                // The processor will automatically attempt to recover from any
                // failures, either transient or fatal, and continue processing.
                // Errors in the processor's operation will be surfaced through
                // its error handler.
                //
                // If this block is invoked, then something external to the
                // processor was the source of the exception.
            }
            finally
            {
                // It is encouraged that you unregister your handlers when you have
                // finished using the Event Processor to ensure proper cleanup.  This
                // is especially important when using lambda expressions or handlers
                // in any form that may contain closure scopes or hold other references.

                processor.ProcessEventAsync -= processEventHandler;
                processor.ProcessErrorAsync -= processErrorHandler;
            }
        }
    }
}
