using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


namespace XGENO.UWP.Controls.FlipView
{
    public sealed class FlipViewItem : Control
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(FlipViewItem), new PropertyMetadata(null));
        public ImageSource ImageSource
        {
            get 
            { 
                return (ImageSource) GetValue(ImageSourceProperty); 
            }
            set 
            { 
                SetValue(ImageSourceProperty, value); 
            }
        }

        public static readonly DependencyProperty BlackMaskOpacityProperty = DependencyProperty.Register("BlackMaskOpacity", typeof(double), typeof(FlipViewItem), new PropertyMetadata(0.00d));
        public double BlackMaskOpacity
        {
            get 
            { 
                return (double) GetValue(BlackMaskOpacityProperty); 
            }
            set 
            { 
                SetValue(BlackMaskOpacityProperty, value); 
            }
        }


        public FlipViewItem()
        {
            this.DefaultStyleKey = typeof(FlipViewItem);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            VisualStateManager.GoToState(this, "Pressed", true);

        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, "Normal", true);

        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

    }
}
