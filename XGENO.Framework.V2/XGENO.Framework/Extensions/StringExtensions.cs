using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace XGENO.Framework.Extensions
{
    public static partial class StringExtensions
    {
        public static string FormatMinsToPeriod(this int timeInMins)
        {
            if (timeInMins <= 60)
                return timeInMins.ToString() + " MINS";

            if (timeInMins <= 24 * 60)
                return (timeInMins / 24).ToString() + " HRS, " + (timeInMins % 24).ToString() + " MINS";

            //Return full time
            return (timeInMins / (24 * 60)).ToString() + " DAYS, " + ((timeInMins % (24 * 60)) / 60).ToString() + " HRS, " + ((timeInMins % (24 * 60)) % 60).ToString() + " MINS";
        }

        public static string TimeAgo(this string baseTime)
        {
            return baseTime.TimeAgoFull().Replace("months", "m").Replace("hours", "h").Replace("days", "d").Replace("years", "y");
        }

        public static string TimeAgoFull(this string baseTime)
        {
            var _timeSpan = DateTime.Now - Convert.ToDateTime(baseTime);

            if (_timeSpan.TotalMinutes < 60)
            {
                return Convert.ToInt32(_timeSpan.TotalMinutes).ToString() + " months ago";
            }

            if (_timeSpan.TotalHours < 24)
            {
                return Convert.ToInt32(_timeSpan.TotalHours).ToString() + " hours ago";
            }

            if (_timeSpan.TotalDays < 365)
            {
                return Convert.ToInt32(_timeSpan.TotalDays).ToString() + " days ago";
            }

            return Convert.ToDouble(_timeSpan.TotalDays / 365).ToString("#") + " years ago";
        }

        public static string CleanFileName(this string input)
        {
            var cleanFileName = input.CleanCacheKey();

            return ((cleanFileName.Length > 100)? cleanFileName.Substring(0, 100) : cleanFileName);
        }

        public static string CleanCacheKey(this string uri)
        {
            string pattern = "[\\~#%&*{}/:<>?|\"-]";
            string replacement = " ";

            Regex regEx = new Regex(pattern);
            string sanitized = Regex.Replace(regEx.Replace(uri, replacement), @"\s+", "_");

            return sanitized;
        }

        public static string Age(this string baseTime)
        {
            try
            {
                var _timeSpan = DateTime.Now - Convert.ToDateTime(baseTime);

                if (_timeSpan.TotalDays < 365)
                {
                    return Convert.ToInt32(_timeSpan.TotalDays).ToString() + " DAYS";
                }

                return Convert.ToDouble(_timeSpan.TotalDays / 365).ToString("#") + " YRS";
            }
            catch
            {
                try
                {
                    return (baseTime + "/01/01").ToString().Age();
                }
                catch
                {

                }
            }

            return "N/A";
        }

        public static string ToUtcJSONDateTimeString(this DateTime theDate)
        {
            return theDate.ToUniversalTime().ToString("yyyy-MM-dd") + "T" + theDate.ToUniversalTime().ToString("hh:mm:ss") + ".000Z";
        }

    }
}
