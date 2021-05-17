using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace XGENO.Framework.Controls
{
    public sealed class RingSlice : Control
    {
        //Foreground Property
        public static readonly DependencyProperty RingForegroundProperty = DependencyProperty.RegisterAttached("RingForeground", typeof(Brush), typeof(RingSlice), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        
        public Brush RingForeground
        {
            get 
            {
                return (Brush)GetValue(RingSlice.RingForegroundProperty); 
            }
            set 
            { 
                SetValue(RingSlice.RingForegroundProperty, value); 
            }
        }

        public static Brush GetRingForeground(DependencyObject obj)
        {
            return (Brush) obj.GetValue(RingForegroundProperty);
        }

        public static void SetRingForeground(DependencyObject obj, Brush value)
        {
            obj.SetValue(RingForegroundProperty, value);
        }

        //Background Property
        public Brush RingBackground
        {
            get 
            {
                return (Brush)GetValue(RingSlice.RingBackgroundProperty); 
            }
            set 
            {
                SetValue(RingSlice.RingBackgroundProperty, value); 
            }
        }

        public static Brush GetRingBackground(DependencyObject obj)
        {
            return (Brush) obj.GetValue(RingBackgroundProperty);
        }

        public static void SetRingBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(RingBackgroundProperty, value);
        }

        public static readonly DependencyProperty RingBackgroundProperty = DependencyProperty.RegisterAttached("RingBackground", typeof(Brush), typeof(RingSlice), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));

        //Thickness Property
        public double Thickness
        {
            get 
            {
                return Convert.ToDouble(GetValue(RingSlice.ThicknessProperty)); 
            }
            set 
            {
                SetValue(RingSlice.ThicknessProperty, value); 
            }
        }

        public static double GetThickness(DependencyObject obj)
        {
            return (double) obj.GetValue(ThicknessProperty);
        }

        public static void SetThickness(DependencyObject obj, double value)
        {
            obj.SetValue(ThicknessProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.RegisterAttached("Thickness", typeof(double), typeof(RingSlice), new PropertyMetadata(5));

        //BackgroundThickness Property
        public double BackgroundThickness
        {
            get
            {
                return Convert.ToDouble(GetValue(RingSlice.BackgroundThicknessProperty));
            }
            set
            {
                SetValue(RingSlice.BackgroundThicknessProperty, value);
            }
        }

        public static double GetBackgroundThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(BackgroundThicknessProperty);
        }

        public static void SetBackgroundThickness(DependencyObject obj, double value)
        {
            obj.SetValue(BackgroundThicknessProperty, value);
        }

        public static readonly DependencyProperty BackgroundThicknessProperty = DependencyProperty.RegisterAttached("BackgroundThickness", typeof(double), typeof(RingSlice), new PropertyMetadata(3));

        //Value Property
        public double Value
        {
            get 
            {
                return Convert.ToDouble(GetValue(RingSlice.ValueProperty)); 
            }
            set 
            {
                SetValue(RingSlice.ValueProperty, value); 
            }
        }

        public static double GetValue(DependencyObject obj)
        {
            return (double) obj.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject obj, double value)
        {
            obj.SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("Value", typeof(double), typeof(RingSlice), new PropertyMetadata(0, OnValueChanged));

        //DiplayText Property
        public string DisplayText
        {
            get
            {
                return GetValue(RingSlice.DisplayTextProperty).ToString();
            }
            set
            {
                SetValue(RingSlice.DisplayTextProperty, value);
            }
        }

        public static string GetDisplayText(DependencyObject obj)
        {
            return obj.GetValue(DisplayTextProperty).ToString();
        }

        public static void SetDisplayText(DependencyObject obj, string value)
        {
            obj.SetValue(DisplayTextProperty, value);
        }

        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.RegisterAttached("DisplayText", typeof(string), typeof(RingSlice), new PropertyMetadata("100"));


        //Value Changed Event
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as RingSlice;

            var pathRoot = ctrl.GetTemplateChild("foregroundPath") as Path;
            var pathBackground = ctrl.GetTemplateChild("backgroundPath") as Path;

            if (pathRoot == null || pathBackground == null) 
                return;

            ctrl.DrawControl();
        }

        //Constructor
        public RingSlice()
        {
            this.DefaultStyleKey = typeof(RingSlice);

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) 
                return;
            
            FontSize = 65;
            
            this.Loaded += RingSlice_Loaded;
            this.SizeChanged += RingSlice_SizeChanged;

        }

        //Methods
        void RingSlice_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawControl();
        }

        void RingSlice_Loaded(object sender, RoutedEventArgs e)
        {
            DrawControl();
        }

        void DrawControl()
        {
            try
            {
                var size = Math.Min(this.RenderSize.Height, this.RenderSize.Width);

                var pathRoot = GetTemplateChild("foregroundPath") as Path;
                var pathBackground = GetTemplateChild("backgroundPath") as Path;

                var strokeThickness = this.Thickness;
                var angle = Value * 360;
                var radius = size / 2 - strokeThickness;

                pathRoot.Width = radius * 2 + strokeThickness;
                pathRoot.Height = radius * 2 + strokeThickness;
                pathRoot.Margin = new Thickness(strokeThickness, strokeThickness, 0, 0);

                pathBackground.Width = radius * 2 + strokeThickness;
                pathBackground.Height = radius * 2 + strokeThickness;
                pathBackground.Margin = new Thickness(strokeThickness, strokeThickness, 0, 0);

                RenderBackground(size);

                RenderArc(size);
            }
            catch
            {

            }
        }

        void RenderBackground(double height)
        {
            var strokeThickness = this.Thickness;
            var radius = height / 2 - strokeThickness;

            var pathFigure = GetTemplateChild("backgroundPathFigure") as PathFigure;
            
            pathFigure.StartPoint = new Point(radius, 0);
            
            var arcSegment = GetTemplateChild("backgroundArcSegment") as ArcSegment;
            
            arcSegment.Point = new Point(radius - 0.001, 0);
            arcSegment.Size = new Size(radius, radius);
        }

        public void RenderArc(double height)
        {
            var strokeThickness = this.Thickness;
            var angle = Value * 360 / 100;
            var radius = height / 2 - strokeThickness;

            Point startPoint = new Point(radius, 0);
            Point endPoint = ComputeCartesianCoordinate(angle, radius);

            endPoint.X += radius;
            endPoint.Y += radius;

            bool largeArc = angle > 180.0;

            Size outerArcSize = new Size(radius, radius);

            var pathFigure = GetTemplateChild("foregroundPathFigure") as PathFigure;
            
            pathFigure.StartPoint = startPoint;

            endPoint.X -= Value == 0 ? 0 : 0.0001;

            var pathRoot = GetTemplateChild("foregroundPath") as Path;
            
            pathRoot.StrokeThickness = strokeThickness;

            var arcSegment = GetTemplateChild("foregroundArcSegment") as ArcSegment;
            
            arcSegment.Point = endPoint;            
            arcSegment.Size = outerArcSize;
            arcSegment.IsLargeArc = largeArc;
        }

        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }

    }
}
