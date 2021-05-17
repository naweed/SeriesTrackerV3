using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.Web.Http;
using XGENO.Framework.Helpers;

namespace XGENO.UWP.Controls.Images
{
    public class ImageEx : Control
    {

        private Rectangle mainImageCtrl;
        private RelativePanel placeholderCtrl;
        private RelativePanel placeholderSectionCtrl;
        private Rectangle progressbarCtrl;

        //ImageSource Property
        public string ImageSource
        {
            get
            {
                return GetValue(ImageEx.ImageSourceProperty).ToString();
            }
            set
            {
                SetValue(ImageEx.ImageSourceProperty, value);
            }
        }

        public static string GetImageSource(DependencyObject obj)
        {
            return obj.GetValue(ImageSourceProperty).ToString();
        }

        public static void SetImageSource(DependencyObject obj, string value)
        {
            obj.SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.RegisterAttached("ImageSource", typeof(string), typeof(ImageEx), new PropertyMetadata(null, OnImageSourceChanged));

        //FallbackImageSource Property
        public string FallbackImageSource
        {
            get
            {
                return GetValue(ImageEx.FallbackImageSourceProperty).ToString();
            }
            set
            {
                SetValue(ImageEx.FallbackImageSourceProperty, value);
            }
        }

        public static string GetFallbackImageSource(DependencyObject obj)
        {
            return obj.GetValue(FallbackImageSourceProperty).ToString();
        }

        public static void SetFallbackImageSource(DependencyObject obj, string value)
        {
            obj.SetValue(FallbackImageSourceProperty, value);
        }

        public static readonly DependencyProperty FallbackImageSourceProperty = DependencyProperty.RegisterAttached("FallbackImageSource", typeof(string), typeof(ImageEx), new PropertyMetadata("ms-appx:///XGENO.UWP.Controls.Images/Assets/TBA_Poster.png"));


        //CornerRounding Property
        public double CornerRounding
        {
            get
            {
                return (double)GetValue(ImageEx.CornerRoundingProperty);
            }
            set
            {
                SetValue(ImageEx.CornerRoundingProperty, value);
            }
        }

        public static double GetCornerRounding(DependencyObject obj)
        {
            return (double)obj.GetValue(CornerRoundingProperty);
        }

        public static void SetCornerRounding(DependencyObject obj, double value)
        {
            obj.SetValue(CornerRoundingProperty, value);
        }

        public static readonly DependencyProperty CornerRoundingProperty = DependencyProperty.RegisterAttached("CornerRounding", typeof(double), typeof(ImageEx), new PropertyMetadata(0.0d));



        //FrameBackground Property
        public Brush FrameBackground
        {
            get
            {
                return (Brush)GetValue(ImageEx.FrameBackgroundProperty);
            }
            set
            {
                SetValue(ImageEx.FrameBackgroundProperty, value);
            }
        }

        public static Brush GetFrameBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(FrameBackgroundProperty);
        }

        public static void SetFrameBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(FrameBackgroundProperty, value);
        }

        public static readonly DependencyProperty FrameBackgroundProperty = DependencyProperty.RegisterAttached("FrameBackground", typeof(Brush), typeof(ImageEx), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 51, 51, 51))));

        //ImageStretch Property
        public Stretch ImageStretch
        {
            get
            {
                return (Stretch)GetValue(ImageEx.ImageStretchProperty);
            }
            set
            {
                SetValue(ImageEx.ImageStretchProperty, value);
            }
        }

        public static Stretch GetImageStretch(DependencyObject obj)
        {
            return (Stretch)obj.GetValue(ImageStretchProperty);
        }

        public static void SetImageStretch(DependencyObject obj, Stretch value)
        {
            obj.SetValue(ImageStretchProperty, value);
        }

        public static readonly DependencyProperty ImageStretchProperty = DependencyProperty.RegisterAttached("ImageStretch", typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.UniformToFill));


        //ImageSource Changed Event
        private static async void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ImageEx;

            await ctrl.LoadImageAsync();
        }

        private async Task LoadImageAsync()
        {

            if (mainImageCtrl == null || placeholderCtrl == null || progressbarCtrl == null)
                return;

            placeholderSectionCtrl.Opacity = 1f;
            mainImageCtrl.Opacity = 0f;

            string cachedImageSource = "";

            //Web Image - extract using Caching
            Progress<HttpProgress> progressCallback = new Progress<HttpProgress>(OnSendRequestProgress);
            try
            {
                cachedImageSource = await WebCacheHelper.GetWebImage(this.ImageSource, progressCallback);
            }
            catch(Exception ex)
            {
                try
                {
                    cachedImageSource = await WebCacheHelper.GetWebImage(this.FallbackImageSource, progressCallback);
                }
                catch
                {
                    cachedImageSource = "ms-appx:///XGENO.UWP.Controls.Images/Assets/TBA_Poster.png";
                }                
            }


            var imageSource = new BitmapImage(new Uri(cachedImageSource, UriKind.Absolute));

            var imageBrush = new ImageBrush();
            imageBrush.Stretch = this.ImageStretch;
            imageBrush.ImageSource = imageSource;
            mainImageCtrl.Fill = imageBrush;

            //Fade Out Placemholder Section Control
            var phBoard = new Storyboard();
            var fadeOutAnimation = new DoubleAnimation() { To = 0f, BeginTime = TimeSpan.FromMilliseconds(0), Duration = TimeSpan.FromMilliseconds(100) };
            fadeOutAnimation.SetValue(Storyboard.TargetPropertyProperty, "(UIElement.Opacity)");
            Storyboard.SetTarget(fadeOutAnimation, placeholderSectionCtrl);
            phBoard.Children.Add(fadeOutAnimation);
            phBoard.Begin();


            //Fade In Image Control
            var imageBoard = new Storyboard();
            var fadeAnimation = new DoubleAnimation(){ To = 1f, BeginTime = TimeSpan.FromMilliseconds(0), Duration = TimeSpan.FromMilliseconds(500) };
            fadeAnimation.SetValue(Storyboard.TargetPropertyProperty, "(UIElement.Opacity)");
            Storyboard.SetTarget(fadeAnimation, mainImageCtrl);
            imageBoard.Children.Add(fadeAnimation);
            imageBoard.Begin();
            
        }

        private void OnSendRequestProgress(HttpProgress obj)
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () =>
                {
                    try
                    {
                        progressbarCtrl.Width = 44 * obj.BytesReceived / obj.TotalBytesToReceive.Value;
                    }
                    catch
                    {
                    }
                });
        }

        public ImageEx()
        {
            this.DefaultStyleKey = typeof(ImageEx);

            this.Loaded += ImageEx_Loaded;
        }

        void ImageEx_Loaded(object sender, RoutedEventArgs e)
        {
            DrawControl();
        }

        async Task DrawControl()
        {
            placeholderCtrl = GetTemplateChild("PlaceholderImage") as RelativePanel;
            placeholderSectionCtrl = GetTemplateChild("pnlPlaceholderSection") as RelativePanel;
            progressbarCtrl = GetTemplateChild("prgProgress") as Rectangle;
            mainImageCtrl = GetTemplateChild("rctImage") as Rectangle;

            placeholderCtrl.CornerRadius = new CornerRadius(CornerRounding);

            await LoadImageAsync();
        }

    }
}
