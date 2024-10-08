using CropBox.Enums;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Services
{
    public class DirectMethodHelper
    {
        static ServiceClient serviceClient;
        public DirectMethodHelper()
        {
            serviceClient = ServiceClient.CreateFromConnectionString(App.Settings.HubConnectionString);
        }
        public async Task InvokeMethodAsync(ValidCommands cmd, bool isActuated = false, string payload = "")
        {
            try
            {
                var methodInvocation = new CloudToDeviceMethod(cmd.ToString())
                {
                    ResponseTimeout = TimeSpan.FromSeconds(30),
                };
                
                string onOff = isActuated ? "on" : "off";


                if (payload != string.Empty)
                    methodInvocation.SetPayloadJson(payload);
                else
                    methodInvocation.SetPayloadJson($"{{\"value\":\"{onOff}\"}}");

                // Invoke the direct method asynchronously and get the response from the simulated device.
                CloudToDeviceMethodResult response = await serviceClient.InvokeDeviceMethodAsync(App.Settings.DeviceId, methodInvocation);
                // show alert
                Console.WriteLine($"Executed command {cmd}\nResponse Status {response.Status}\nPayload{response.GetPayloadAsJson()}");
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                await Shell.Current.DisplayAlert("Method Exception", $"Exception {cmd}", "Ok");
            }
        }
    }
}
