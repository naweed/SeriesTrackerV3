using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using XGENO.SeriesTracker.DataModels;

namespace XGENO.SeriesTracker.Views
{
    public static class Converters
    {
        public static Visibility IsVisible(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility ReverseVisible(bool value)
        {
            return !value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static string DisplayPercentage(double value)
        {
            return value.ToString("0") + "%";
        }

        public static Visibility IsVisibleOnCount(int value)
        {
            return (value > 0) ? Visibility.Visible : Visibility.Collapsed;
        }



    }

}
