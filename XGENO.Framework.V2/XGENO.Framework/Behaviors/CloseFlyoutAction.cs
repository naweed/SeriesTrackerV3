using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace XGENO.Framework.Behaviors
{
    public class CloseFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            var flyout = sender as Flyout;
            flyout?.Hide();

            return null;
        }
    }

/*
public class CloseFlyoutAction : DependencyObject, IAction
{
    public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(nameof(TargetObject), typeof(Control), typeof(CloseFlyoutAction), new PropertyMetadata(null));
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
        var parent = TargetObject ?? sender as DependencyObject;

        while (parent != null)
        {
            if (parent is FlyoutPresenter)
            {
                ((parent as FlyoutPresenter).Parent as Popup).IsOpen = false;
                break;
            }
            else
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        return null;
    }

}
*/

}
