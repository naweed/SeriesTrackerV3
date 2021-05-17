using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XGENO.UWP.Controls.RatingControl
{
    public class RatingStar : Control
    {
        //RatingValue Property
        public string RatingValue
        {
            get
            {
                return GetValue(RatingStar.RatingValueProperty).ToString();
            }
            set
            {
                SetValue(RatingStar.RatingValueProperty, value);
            }
        }

        public static string GetRatingValue(DependencyObject obj)
        {
            return obj.GetValue(RatingValueProperty).ToString();
        }

        public static void SetRatingValue(DependencyObject obj, string value)
        {
            obj.SetValue(RatingValueProperty, value);
        }

        public static readonly DependencyProperty RatingValueProperty = DependencyProperty.RegisterAttached("RatingValue", typeof(string), typeof(RatingStar), new PropertyMetadata("0.0"));


        //FontMargin Property
        public Thickness FontMargin
        {
            get
            {
                return (Thickness)GetValue(RatingStar.FontMarginProperty);
            }
            set
            {
                SetValue(RatingStar.FontMarginProperty, value);
            }
        }

        public static Thickness GetFontMarginValue(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(FontMarginProperty);
        }

        public static void SetFontMarginValue(DependencyObject obj, Thickness value)
        {
            obj.SetValue(FontMarginProperty, value);
        }

        public static readonly DependencyProperty FontMarginProperty = DependencyProperty.RegisterAttached("FontMargin", typeof(Thickness), typeof(RatingStar), new PropertyMetadata(new Thickness(0)));


        public RatingStar()
        {
            this.DefaultStyleKey = typeof(RatingStar);

            /*
                <!-- Rating Star -->
                <fwRating:RatingStar x:Name="strRating"
                    RelativePanel.AlignRightWithPanel="True"
                    RelativePanel.AlignBottomWithPanel="True"
                    Width="40"
                    Height="40"
                    Margin="8"
                    Opacity="0.95"
                    FontSize="8"
                    FontFamily="Segoe UI"
                    RatingValue="{x:Bind Rating10Display}" />
            */
        }

    }
}
