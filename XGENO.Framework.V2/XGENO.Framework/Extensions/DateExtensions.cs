using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace XGENO.Framework.Extensions
{
    public static partial class DateExtensions
    {
        public static DateTime LastDayOfTheMonth(this DateTime theDate)
        {
            return new DateTime(theDate.Year, theDate.Month, DateTime.DaysInMonth(theDate.Year, theDate.Month));
        }

        public static DateTime LastDayOfNextMonth(this DateTime theDate)
        {
            var nextMonthStart = theDate.LastDayOfTheMonth().AddDays(1);

            return nextMonthStart.LastDayOfTheMonth();
        }

        public static bool IsLastDayOfTheMonth(this DateTime theDate)
        {
            return (theDate.ToString("yyyy-MM-dd") == theDate.LastDayOfTheMonth().ToString("yyyy-MM-dd"));
        }

        public static int DayOfTheWeek(this DateTime theDate)
        {
            return (int)theDate.DayOfWeek;
        }

        public static bool IsLastDayOfTheWeek(this DateTime theDate)
        {
            return (theDate.DayOfTheWeek() == 6);
        }

        public static DateTime ThisWeekEnd(this DateTime theDate)
        {
            return theDate.AddDays(7 - ((theDate.DayOfTheWeek() == (int)DayOfWeek.Sunday) ? 7 : theDate.DayOfTheWeek()));
        }

        public static DateTime NextWeekEnd(this DateTime theDate)
        {
            return theDate.AddDays(7).ThisWeekEnd();
        }

    }
}
