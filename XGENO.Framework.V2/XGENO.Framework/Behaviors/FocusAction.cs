using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XGENO.Framework.Behaviors
{
    public class FocusAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            Control ui;

            if (TargetObject != null)
                ui = TargetObject;
            else
                ui = sender as Control;

            if (ui != null)
                ui.Focus(FocusState.Programmatic);

            return null;
        }

        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register(nameof(TargetObject), typeof(Control), typeof(FocusAction), new PropertyMetadata(null));
        public Control TargetObject
        {
            get { return (Control)GetValue(TargetObjectProperty); }
            set { SetValue(TargetObjectProperty, value); }
        }
    }
}
