using CropBox.Enums;
using CropBox.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Repos
{
    public class TelemetryRepo
    {
        public static int UPDATE_INTERVAL = 10000;
        private const int LIMIT = 250;
        private const int PITCH_LIMIT = 75;
        private const int ROLL_LIMIT = 75;
        private const int VIBRATION_LIMIT = 200;
        private const int LUMINOSITY_LIMIT = 75;
        private LimitedQueue<Reading> readings;

        public LimitedQueue<Reading> Readings { get { return readings; } set { readings = value; } }
        public TelemetryRepo()
        { 
            readings = new LimitedQueue<Reading>(LIMIT);
        }
        public static Dictionary<string, double> CriticalThresholds = new Dictionary<string, double>()
        {
            { ReadingTypes.Pitch.ToString(), PITCH_LIMIT},
            { ReadingTypes.Roll.ToString(), ROLL_LIMIT },
            { ReadingTypes.Vibration.ToString(), VIBRATION_LIMIT },
            { ReadingTypes.Luminosity.ToString(), LUMINOSITY_LIMIT },
        };
        public Dictionary<string, double> DynamicThresholds = new Dictionary<string, double>()
        {
            { ReadingTypes.Temperature.ToString(), 0},
            { ReadingTypes.Humidity.ToString(), 0 },
            { ReadingTypes.Moisture.ToString(), 0 },
            { ReadingTypes.WaterDepth.ToString(), 0 },
        };
    }
}
