using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace XGENO.SeriesTracker.Views
{
	public class DoubleWrapper : DependencyObject
	{
	   public double Value
	    {
	      get { return (double)GetValue(ValueProperty); }
	      set { SetValue(ValueProperty, value); }
	    }
	
	// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
	public static readonly DependencyProperty ValueProperty =
	    DependencyProperty.Register("Value", typeof(double), typeof(DoubleWrapper), new PropertyMetadata(0.0));
	
}

}
