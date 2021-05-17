using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace XGENO.UWP.Controls.FWButton
{
    public class MarginConverter : DependencyObject, IValueConverter
    {
        public IconPosition IconPosition
        {
            get { return (IconPosition)GetValue(IconPositionProperty); }
            set { SetValue(IconPositionProperty, value); }
        }
        public static readonly DependencyProperty IconPositionProperty =
            DependencyProperty.Register("IconPosition", typeof(IconPosition), typeof(MarginConverter), new PropertyMetadata(IconPosition.Left));


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (IconPosition)
            {
                case IconPosition.Left:
                    return new Thickness(double.Parse(value.ToString()), 0, 0, 0);
                case IconPosition.Right:
                    return new Thickness(0, 0, double.Parse(value.ToString()), 0);
                case IconPosition.Top:
                    return new Thickness(0, double.Parse(value.ToString()), 0, 0);
                case IconPosition.Bottom:
                    return new Thickness(0, 0, 0, double.Parse(value.ToString()));
                default:
                    return new Thickness(0, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
