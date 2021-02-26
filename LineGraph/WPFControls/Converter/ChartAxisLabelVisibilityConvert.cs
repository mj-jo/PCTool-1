using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Windows;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;

namespace WPFControls.Converter
{
    public class ChartAxisLabelVisibilityConvert : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //var height = 0d;
            //var chartHeight = (double)values[0];
            System.Windows.Controls.DataVisualization.Charting.Primitives.EdgePanel edgePanel = values[0] as System.Windows.Controls.DataVisualization.Charting.Primitives.EdgePanel;
            //ChartArea chartArea 
            LinearAxis linearAxis = values[1] as LinearAxis;
            string _value = values[2] as string;
            var value = System.Convert.ToDouble(_value.Replace('\'', ' '));
            double actualcenter = System.Convert.ToDouble((linearAxis.ActualMaximum - linearAxis.ActualMinimum) / 2);
            if (linearAxis.ActualMinimum < 0) actualcenter = System.Convert.ToDouble((linearAxis.ActualMaximum + linearAxis.ActualMinimum) / 2);
            actualcenter += System.Convert.ToDouble(linearAxis.ActualMinimum);
            if (linearAxis.ActualMinimum == value) return Visibility.Visible;
            if (linearAxis.ActualMaximum == value) return Visibility.Visible;
            if (actualcenter == value) return Visibility.Visible;
            //var range = (Range<double>)linearAxis.ActualRange;

            //if (range.HasData)
            //{
            //    if (range.Minimum > 0)
            //    {
            //        // Set labels to bottom
            //        height = 0;
            //    }
            //    else if (range.Maximum < 0)
            //    {
            //        // Set labels to top
            //        height = -chartHeight;
            //    }
            //    else
            //    {
            //        var rangeHeight = range.Maximum - range.Minimum;
            //        var pointsPerHeight = chartHeight / rangeHeight;
            //        height = range.Minimum * pointsPerHeight;
            //    }
            //}

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {      
            throw new NotImplementedException();
        }
    }
}
