using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;


namespace XGENO.Framework.Controls
{
    [TemplatePart(Name = "canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "textblock", Type = typeof(TextBlock))]
    [ContentProperty(Name = "Text")]
    public sealed class MarqueeTextBlock : Control
    {
        static string CANVAS_NAME = "canvas";
        static string RECT_GEOMETRY_CANVAS_NAME = "rectanglegeometeryClipCanvas";
        static string TEXTBLOCK_NAME = "textblock";

        static string VISUALSTATE_STOPPED = "visualstateStopped";
        static string VISUALSTATE_MARQUEE = "visualstateMarquee";


        public MarqueeTextBlock() : base()
        {
            this.DefaultStyleKey = typeof(MarqueeTextBlock);

            FontSize_OnChanged(this, FontSizeProperty);
            RegisterPropertyChangedCallback(FontSizeProperty, FontSize_OnChanged);
        }

        void FontSize_OnChanged(DependencyObject sender, DependencyProperty dp)
        {
            // Set minimum height
            var displayInfo = DisplayInformation.GetForCurrentView();
            var minimumHeight = ((this.FontSize / 72.0d) * displayInfo.LogicalDpi) / displayInfo.RawPixelsPerViewPixel;
            this.MinHeight = minimumHeight;
        }

        #region Properties
        private static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(MarqueeTextBlock), new PropertyMetadata(null,
            (sender, e) =>
            {
                var control = (MarqueeTextBlock)sender;

                control.StartMarqueeAnimationIfNeeded();
            }));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static readonly DependencyProperty IsStoppedProperty = DependencyProperty.Register(nameof(IsStopped), typeof(bool), typeof(MarqueeTextBlock), new PropertyMetadata(false,
            (sender, e) =>
            {
                var control = (MarqueeTextBlock)sender;

                control.StartMarqueeAnimationIfNeeded();
            }));
        public bool IsStopped
        {
            get { return (bool)GetValue(IsStoppedProperty); }
            set { SetValue(IsStoppedProperty, value); }
        }

        private static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(nameof(EasingFunction), typeof(EasingFunctionBase), typeof(MarqueeTextBlock), new PropertyMetadata(null,
            (sender, e) =>
            {
                var control = (MarqueeTextBlock)sender;

                control.StartMarqueeAnimationIfNeeded();
            }));
        public EasingFunctionBase EasingFunction
        {
            get { return (EasingFunctionBase)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }

        private static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(MarqueeTextBlock), new PropertyMetadata(TimeSpan.FromSeconds(4),
            (sender, e) =>
            {
                var control = (MarqueeTextBlock)sender;

                control.StartMarqueeAnimationIfNeeded();
            }));
        public TimeSpan AnimationDuration
        {
            get { return (TimeSpan)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        private static readonly DependencyProperty AnimationSpeedRatioProperty = DependencyProperty.Register(nameof(AnimationSpeedRatio), typeof(double), typeof(MarqueeTextBlock), new PropertyMetadata(1.0d,
            (sender, e) =>
            {
                var control = (MarqueeTextBlock)sender;

                control.StartMarqueeAnimationIfNeeded();
            }));
        public double AnimationSpeedRatio
        {
            get { return (double)GetValue(AnimationSpeedRatioProperty); }
            set { SetValue(AnimationSpeedRatioProperty, value); }
        }

        private static readonly DependencyProperty IsTickerProperty = DependencyProperty.Register(nameof(IsTicker), typeof(bool), typeof(MarqueeTextBlock), new PropertyMetadata(false,
            (sender, e) =>
            {
                var control = (MarqueeTextBlock)sender;

                control.StartMarqueeAnimationIfNeeded();
            }));
        public bool IsTicker
        {
            get { return (bool)GetValue(IsTickerProperty); }
            set { SetValue(IsTickerProperty, value); }
        }
        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            StopMarqueeAnimation(useTransitions: false);

            this.SizeChanged += MarqueeTextControl_SizeChanged;   // NOTE: This event will start animation, if needed
        }

        private void MarqueeTextControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var a = DisplayInformation.GetForCurrentView();
            var b = this.FontSize;

            StopMarqueeAnimation();
            StartMarqueeAnimationIfNeeded();
        }

        private void StopMarqueeAnimation(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, VISUALSTATE_STOPPED, useTransitions);
        }

        private void StartMarqueeAnimationIfNeeded(bool useTransitions = true)
        {
            var canvas = (Canvas)GetTemplateChild(CANVAS_NAME);
            if (canvas == null)
                return;

            // Change clip rectangle for new canvas size
            var rectanglegeometryClipCanvas = (RectangleGeometry)GetTemplateChild(RECT_GEOMETRY_CANVAS_NAME);
            if (rectanglegeometryClipCanvas != null)
                rectanglegeometryClipCanvas.Rect = new Rect(0.0d, 0.0d, canvas.ActualWidth, canvas.ActualHeight);

            if (this.IsStopped)
            {
                StopMarqueeAnimation(useTransitions);
                return;
            }

            // Add an animation handler
            var textblock = (TextBlock)GetTemplateChild(TEXTBLOCK_NAME);
            if (textblock != null)
            {
                // Animation is only needed if 'textblock' is larger than canvas
                if (textblock.ActualWidth > canvas.ActualWidth)
                {
                    var visualstateGroups = VisualStateManager.GetVisualStateGroups(canvas).First();
                    var visualstateMarquee = visualstateGroups.States.Single(l => l.Name == VISUALSTATE_MARQUEE);
                    var storyboardMarquee = new Storyboard()
                    {
                        AutoReverse = !IsTicker,
                        Duration = this.AnimationDuration,
                        RepeatBehavior = RepeatBehavior.Forever,
                        SpeedRatio = this.AnimationSpeedRatio
                    };

                    if (IsTicker)
                    {
                        var animationMarquee = new DoubleAnimationUsingKeyFrames
                        {
                            AutoReverse = storyboardMarquee.AutoReverse,
                            Duration = storyboardMarquee.Duration,
                            RepeatBehavior = storyboardMarquee.RepeatBehavior,
                            SpeedRatio = storyboardMarquee.SpeedRatio,
                        };
                        var frame1 = new DiscreteDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                            Value = canvas.ActualWidth
                        };
                        var frame2 = new EasingDoubleKeyFrame
                        {
                            KeyTime = KeyTime.FromTimeSpan(storyboardMarquee.Duration.TimeSpan),
                            Value = -textblock.ActualWidth,
                            EasingFunction = this.EasingFunction
                        };

                        animationMarquee.KeyFrames.Add(frame1);
                        animationMarquee.KeyFrames.Add(frame2);

                        storyboardMarquee.Children.Add(animationMarquee);

                        Storyboard.SetTarget(animationMarquee, textblock.RenderTransform);
                        Storyboard.SetTargetProperty(animationMarquee, "(TranslateTransform.X)");
                    }
                    else
                    {
                        var animationMarquee = new DoubleAnimation
                        {
                            AutoReverse = storyboardMarquee.AutoReverse,
                            Duration = storyboardMarquee.Duration,
                            RepeatBehavior = storyboardMarquee.RepeatBehavior,
                            SpeedRatio = storyboardMarquee.SpeedRatio,
                            From = 0.0d,
                            To = -textblock.ActualWidth,
                            EasingFunction = this.EasingFunction
                        };

                        storyboardMarquee.Children.Add(animationMarquee);

                        Storyboard.SetTarget(animationMarquee, textblock.RenderTransform);
                        Storyboard.SetTargetProperty(animationMarquee, "(TranslateTransform.X)");
                    }

                    visualstateMarquee.Storyboard = storyboardMarquee;

                    VisualStateManager.GoToState(this, VISUALSTATE_MARQUEE, useTransitions);
                }
            }
        }
    }
}
