using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedalTracker.Converters
{
    /// <summary>
    /// Team name : CropBox  
    /// Team number : F
    /// Winter 4/28/2023 
    /// 420-6A6-AB
    /// StringToColorConverter class is used to convert string hex values to Color verve versa
    /// </summary>
    internal class StringToColorConverter : IValueConverter
    {

        /// <summary>
        /// Convert method is used to convert string hex values to Color
        /// </summary>
        /// <param name="value">Value is the color value in the string</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>return color value </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = null;
            try
            {
                /*  Color.FromRgba is static method from Color class that allows you to convert 
                    string hex values to Color. Input must start with hash sign #.
                */
                color = Color.FromRgba(value as string);
                return color;
            }
            catch (Exception)
            {
                return color;
            }
        }
        /// <summary>
        /// ConvertBack method is used to convert Color to string hex values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>return the hex value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (Color)value;

            return color.ToHex();
        }
    }
}
