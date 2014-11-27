using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MobileConference.CustomAttribute
{
    /// <summary>
    /// Attribute which checked valid date
    /// </summary>
    public class DateFormatAttribute:ValidationAttribute
    {
        const char delimiter = '.';
        private readonly int[] dayInMonth = new[] {31,28,31,30,31,30,31,31,30,31,30,31};
        public override bool IsValid(object value)
        {
            //don't check if null (use required attribure)
            if (value == null) return true;

            string stringVal = value.ToString();
            string[] arr = stringVal.Split(delimiter);
            if (arr.Count() != 3) return false;

            int day;
            if (!int.TryParse(arr[0], out day)) return false;

            int month;
            if (!int.TryParse(arr[1], out month)) return false;

            int year;
            if (!int.TryParse(arr[2], out year)) return false;

            if (year < 1900) return false;
            if (month < 1 || month > 12) return false;
            if (day < 1) return false;
            bool isLeapYearAndFebruary = (month == 2 && year/4.0 == Math.Ceiling(year/4.0));
            if (day > dayInMonth[month - 1] + (isLeapYearAndFebruary? 1 : 0)) return false;

            return true;
        }
    }
}