using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace XGENO.Framework.Converters
{
    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble(value).ToString("0") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ToDouble(value.ToString().Replace("%", ""));
        }
    }
}
