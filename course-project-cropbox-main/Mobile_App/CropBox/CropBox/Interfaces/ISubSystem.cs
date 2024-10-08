using CropBox.Enums;
using CropBox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Interfaces
{
    /// <summary>
    /// Team name : CropBox  
    /// Team number : F
    /// Winter 4/28/2023 
    /// 420-6A6-AB
    /// ISubSystem interface is used to define the methods that will be used by the subsystems
    /// </summary>
    public interface ISubSystem
    {
        /// <summary>
        ///  GetSensorData method is used to get the sensor data
        /// </summary>
        /// <returns>return key pair value for the sensor</returns>
        public Task<List<Reading>> GetSensorData();
        /// <summary>
        /// SendCommand method is used to send command to the actuator
        /// </summary>
        /// <param name="command"> Commmand is esire status you want for the actuator  </param>
        /// <param name="actuatorType"> actuatortype is type actuator </param>
        public void SendCommand(string command, CommandTypes actuatorType);
    }
}
