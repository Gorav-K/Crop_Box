using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Models
{
    /// <summary>
    /// Alert class is used to store the alert data
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Name is used to store the actuator name
        /// </summary>
        public string Name { get; set; }    // actuator name
        /// <summary>
        /// State is used to store the actuator status
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Alert constructor is used to initialize the alert object
        /// </summary>
        /// <param name="name"> Name is string represent the name Actuator </param>
        /// <param name="state">Actuated is string represent state of the actuator</param>        
        public Command()
        {
            Name = "";
            State = "";
        }
        public Command(string name, string state) 
        {
            Name = name;
            State = state;
        }
    }
}
