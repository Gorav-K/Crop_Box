using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedalTracker.Repos
{
    /// <summary>
    /// Team name : CropBox  
    /// Team number : F
    /// Winter 4/28/2023 
    /// 420-6A6-AB
    /// ChartRepo class is used to Initialize the gauge chart
    /// </summary>
    public class ChartRepo
    {
        public static string PRIMARY_COLOR = "#0096FF";
        public static string SECONDARY_COLOR = "#00008B";
        /// <summary>
        /// GetSeries is used to get the series
        /// </summary>
        /// <param name="countries"></param>
        /// <returns></returns>
        public List<ISeries> GetSeries(List<object> countries)
        {
            throw new NotImplementedException(); // not implemented yet
        }

        /// <summary>
        /// GetGaugeSeries is used to get the gauge series 
        /// </summary>
        /// <param name="threshold"> Threshold is float represent the threshold value</param>
        /// <param name="value"> Value is float represent the value of the reading</param>
        /// <returns> Return IEnumerable ISeries< /returns>
        public static IEnumerable<ISeries> GetGaugeSeries(SKColor color, ObservableValue value) 
        {
            
            value.Value = value.Value < 0 ? -value.Value : value.Value;

            value.Value = Double.Round((double)value.Value, 3);
            GaugeBuilder gaugeBuilder = new GaugeBuilder();


            gaugeBuilder
            .WithInnerRadius(350)
            .WithBackgroundInnerRadius(350)
            .WithLabelsSize(0)
            .WithBackground(new SolidColorPaint(new SKColor(100, 181, 246, 90)))
            .AddValue(value, "Reading", color, SKColors.Black) // defines the value and the color 
            .BuildSeries();
            return gaugeBuilder.BuildSeries();
        }
    }
}
