using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace XGENO.Framework.Behaviors
{
    public class OpenFlyoutAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(nameof(TargetObject), typeof(Control), typeof(OpenFlyoutAction), new PropertyMetadata(null));
        public Control TargetObject
        {
            get
            {
                return (Control)GetValue(TargetObjectProperty);
            }
            set
            {
                SetValue(TargetObjectProperty, value);
            }
        }
        
        public object Execute(object sender, object parameter)
        {
            FlyoutBase.ShowAttachedFlyout(TargetObject ?? (FrameworkElement)sender);

            return null;
        }

    }
}
