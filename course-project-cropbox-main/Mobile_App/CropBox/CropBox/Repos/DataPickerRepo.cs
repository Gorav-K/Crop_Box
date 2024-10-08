using CropBox.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Repos
{
    public class DataPickerRepo
    {
        public ObservableCollection<ReadingTypes> OwnerReadings = new ObservableCollection<ReadingTypes>() { ReadingTypes.Pitch, ReadingTypes.Roll, ReadingTypes.Vibration, ReadingTypes.Luminosity };
        public ObservableCollection<ReadingTypes> TechReadings = new ObservableCollection<ReadingTypes>() { ReadingTypes.WaterDepth, ReadingTypes.Temperature, ReadingTypes.Humidity, ReadingTypes.Moisture };
        public ObservableCollection<ReadingTypes> GetUserBasedOptions(UserTypes userTypes)
        {
            if (userTypes == UserTypes.Owner) return OwnerReadings;
            else if (userTypes == UserTypes.Technician) return TechReadings;
            return null;
        }
    }
}
