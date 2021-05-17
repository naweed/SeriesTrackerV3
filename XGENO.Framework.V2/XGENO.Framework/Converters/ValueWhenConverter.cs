﻿using System;
using Windows.UI.Xaml.Data;

namespace XGENO.Framework.Converters
{
    public class ValueWhenConverter : IValueConverter
    {
        public object Value { get; set; }
        public object Otherwise { get; set; }
        public object When { get; set; }
        public object OtherwiseValueBack { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (object.Equals(value,parameter ?? When))
                    return Value;

                return Otherwise;
            }
            catch
            {
                return Otherwise;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (OtherwiseValueBack == null)
                throw new InvalidOperationException("Cannot ConvertBack if no OtherwiseValueBack is set!");
            try
            {
                if (object.Equals(value, Value))
                    return When;

                return OtherwiseValueBack;
            }
            catch
            {
                return OtherwiseValueBack;
            }
        }

    }
}
