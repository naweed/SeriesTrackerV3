using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using XGENO.Framework.Helpers;

namespace XGENO.Framework.Extensions
{
    public class ImageExtensions : DependencyObject
    {
        public static DependencyProperty CachedSourceProperty = DependencyProperty.RegisterAttached("CachedSource", typeof(string), typeof(ImageExtensions), new PropertyMetadata(null, OnCachedSourceChanged));

        public static void SetCachedSource(DependencyObject obj, string value)
        {
            obj.SetValue(CachedSourceProperty, value);
        }

        public static string GetCachedSource(DependencyObject obj)
        {
            return (string)obj.GetValue(CachedSourceProperty);
        }

        static async void OnCachedSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {

            if (args.NewValue != null && !string.IsNullOrEmpty(args.NewValue.ToString()))
            {
                var cachedSource = args.NewValue.ToString();

                Image theImage = sender as Image;

                ImageBrush theImageBrush = sender as ImageBrush;

                if (theImage != null)
                    SetSourceOnObject(theImage, cachedSource);
                else
                    SetSourceOnObject(theImageBrush, cachedSource);

            }
        }

        private static async Task SetSourceOnObject(object imgControl, string cachedSource)
        {
            try
            {
                //Web Image - extract using Caching
                string cachedImageSource = await WebCacheHelper.GetWebImage(cachedSource);

                var imageSource = new BitmapImage(new Uri(cachedImageSource, UriKind.Absolute));

                if (imgControl is Image)
                {
                    ((Image)imgControl).Source = imageSource;
                }
                else
                {
                    if (imgControl is ImageBrush)
                    {
                        ((ImageBrush)imgControl).ImageSource = imageSource;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            await Task.CompletedTask;
        }


    }
}
