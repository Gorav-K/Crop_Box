using CropBox.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Models
{
    /// <summary>
    /// Team name : CropBox  
    /// Team number : F
    /// Winter 4/28/2023 
    /// 420-6A6-AB
    /// Reading class is used to store the reading data
    /// </summary>
    public class Reading
    {
        private string value;
        // for floats
        public const int PRECISION = 6;
        /// <summary>
        /// Type is used to store the reading type (Sensor)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Unit is used to store the unit of the reading type (Sensor)
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// Value is used to store the value of the reading type (Sensor)
        /// </summary>
        public string Value
        {
            get { return value; }

            set
            {
                if (value.Contains("value"))
                {
                    JObject parsedData = JObject.Parse(value);
                    this.value = (string)parsedData[nameof(value)];
                }
                else
                {
                    this.value = value;
                }

            }
        }

        /// <summary>
        /// Time stamp is used to store the time of the reading
        /// </summary>
        public DateTime TimeStamp { get; set; }


        public string FormattedReading { get { return $"{Value} {Unit}"; } }
        
        /// <summary>
        /// Reading constructor is used to initialize the reading object
        /// </summary>
        /// <param name="type">Type is string represent the type of the reading</param>
        /// <param name="unit">Unit is string represent the unit of the reading</param>
        /// <param name="value">Value is float represent the value of the reading</param>
        public Reading(string type, string unit, string value)
        {
            Type = type;
            Unit = unit;
            Value = value;
            TimeStamp = DateTime.Now;
        }
        public Reading()
        {
            Type = "";
            Unit = "";
            Value = "";
            TimeStamp = DateTime.Now; 
        }
        public override string ToString()
        {
            if (Value == "") return "Reading not available";

            string shortenedValue = Value.Length >= PRECISION ? Value.Substring(0, PRECISION) : Value;

            return $"{shortenedValue} {Unit}";
        }
    }
}
