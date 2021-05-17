using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace XGENO.Framework.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public string Format { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var format = (parameter as string) ?? Format;

            if (format == null)
                return value;
            
            if(string.IsNullOrWhiteSpace(language))
            {
                return string.Format(format, value);
            }    
                
            try
            {
                var culture = new CultureInfo(language);
                return string.Format(culture, format, value);
            }
            catch
            {
                return string.Format(format, value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
