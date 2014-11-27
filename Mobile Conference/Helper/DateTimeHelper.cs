using System;
using System.Linq;
using MobileConference.GlobalData;

namespace MobileConference.Helper
{
    public static class DateTimeHelper
    {
        private static readonly string[] monthNames = 
        {
            "января","февраля","марта","апреля","мая","июня","июля","августа","сентября","октября","ноября","декабря"
        };

        /// <summary>
        /// From format "dd.mm.yyyy"
        /// </summary>
        /// <param name="dateAsString">string in format dd.mm.yyyy</param>
        /// <returns>date for the string</returns>
        public static DateTime? ConvertToDate(this string dateAsString)
        {
            try
            {
                string[] partsOfDate = dateAsString.Split('.');
                int day = Int32.Parse(partsOfDate[0]);
                int month = Int32.Parse(partsOfDate[1]);
                int year = Int32.Parse(
                    partsOfDate[2]);
                return new DateTime(year, month, day);
            }
            catch
            {
                // if it's impossible to convert string into the date
                return null;
            }
        }

        /// <summary>
        /// From format "dd.mm.yyyy HH:mm"
        /// </summary>
        /// <param name="dateAsString">string in format dd.mm.yyyy HH:mm</param>
        /// <returns>date-time for the string</returns>
        public static DateTime? ConvertToDateTime(this string dateAsString)
        {
            string[] dateTimeParts = dateAsString.Trim().Split(' ');
            if (dateTimeParts.Count() == 1) return dateAsString.ConvertToDate();
            string[] hourMinute = dateTimeParts[1].Split(':');
            if (hourMinute.Count() != 2) return null;
            string[] partsOfDate = dateTimeParts[0].Split('.');
            if (partsOfDate.Count() != 3) return null;
            int year;
            int month;
            int day;
            int hour;
            int minute;
            if (!int.TryParse(hourMinute[0], out hour)) return null;
            if (!int.TryParse(hourMinute[1], out minute)) return null;
            if (!int.TryParse(partsOfDate[2], out year)) return null;
            if (!int.TryParse(partsOfDate[1], out month)) return null;
            if (!int.TryParse(partsOfDate[0], out day)) return null;
            return new DateTime(year, month, day, hour, minute, 0);
        }

        /// <summary>
        /// To format "dd.mm.yy"
        /// </summary>
        /// <param name="date">date, which need to translate into the string</param>
        /// <returns>string in format dd.mm.yyyy</returns>
        public static string ConvertToCustomString(this DateTime? date)
        {
            if (date == null) return String.Empty;
            return String.Format("{0}{1}.{2}{3}.{4}", (date.Value.Day < 10) ? "0" : "", date.Value.Day.ToString(),
                (date.Value.Month < 10) ? "0" : "", date.Value.Month.ToString(), date.Value.Year.ToString());
        }


        /// <summary>
        /// To format "dd.mm.yy HH:MM"
        /// </summary>
        /// <param name="date">date, which need to translate into the string</param>
        /// <returns>string in format dd.mm.yyyy HH:MM</returns>
        public static string ConvertToDateTimeString(this DateTime? date)
        {
            if (date == null) return String.Empty;
            return String.Format("{0} {1}{2}:{3}{4}", date.ConvertToCustomString(), (date.Value.Hour < 10) ? "0" : "",
                date.Value.Hour, (date.Value.Minute < 10) ? "0" : "", date.Value.Minute);
        }

        /// <summary>
        /// Convert date to format like "1 february"
        /// </summary>
        public static string ConvertToDayMonthString(this DateTime? date)
        {
            if (date == null) return String.Empty;
            return String.Format("{0} {1}", date.Value.Day, monthNames[date.Value.Month - 1]);
        }


        //Day format like 01, 02 etc
        public static string PrettyDay(this int day)
        {
            return day < 10 ? string.Format("0{0}", day) : day.ToString();
        }

        public static string PrettyDay(this int? day)
        {
            return day == null ? string.Empty : ((int) day).PrettyDay();
        }

        public static string CustomMonthTitle(this DateTime date)
        {
            return monthNames[date.Month - 1];
        }

        /// <summary>
        /// This method was written myself (no copy-paste)
        /// </summary>
        /// <returns>Pretty date like 7 days ago or 3 minutes ago etc</returns>
        public static string PrettyDateTime(this DateTime date)
        {
            TimeSpan s = DateTime.Now.Subtract(date);
            
            //get all intervals
            int day = (int)s.TotalDays;
            int second = (int)s.TotalSeconds;
            
            //today
            if (day == 0)
            {
                if (second < 60)
                {
                    return GlobalValuesAndStrings.JustNow;
                }
                if (second < 120)
                {
                    return GlobalValuesAndStrings.MinuteAgo;
                }

                int minutes = (int)Math.Floor((double)second / 60);
                if (second < 3600)
                {
                    if (minutes%10 == 0 || (minutes>10 && minutes<21))
                    {
                        return string.Format("{0} {1}", minutes, GlobalValuesAndStrings.ManyMinutes);                        
                    }
                    if (minutes % 10 == 1)
                    {
                        return string.Format("{0} {1}", minutes, GlobalValuesAndStrings.MinuteAgo);
                    }
                    if (minutes%10 < 5)
                    {
                        return string.Format("{0} {1}", minutes, GlobalValuesAndStrings.SomeMinutes);
                    }
                    return string.Format("{0} {1}",minutes, GlobalValuesAndStrings.ManyMinutes);
                }

                // Less than 2 hours
                if (second < 7200)
                {
                    return GlobalValuesAndStrings.HourAgo;
                }

                // Less than one day ago.
                if (second < 18000)
                {
                    return string.Format("{0} {1}", Math.Floor((double)second / 3600), GlobalValuesAndStrings.SomeHour);
                }

                //5 hours to 20 hours
                if (second < 75600)
                {
                    return string.Format("{0} {1}", Math.Floor((double)second / 3600), GlobalValuesAndStrings.ManyHour);
                }

                //21 hours
                if (second < 79200)
                {
                    return string.Format("{0} {1}", 21, GlobalValuesAndStrings.HourAgo);
                }

                if (second < 86400)
                {
                    return string.Format("{0} {1}", Math.Floor((double)second / 3600), GlobalValuesAndStrings.SomeHour);
                }
            }
            
            //previous days
            if (day == 1)
            {
                return GlobalValuesAndStrings.Yesterday;
            }
            if (day < 5)
            {
                return string.Format("{0} {1}", day, GlobalValuesAndStrings.SomeDays);
            }
            if (day < 8)
            {
                return string.Format("{0} {1}", day, GlobalValuesAndStrings.ManyDays);
            }
            if (day < 28)
            {
                return string.Format("{0} {1}", Math.Ceiling((double)day / 7), GlobalValuesAndStrings.SomeWeeks);
            }
            if (day < 45)
            {
                return GlobalValuesAndStrings.Month;
            }
            if (day < 137)
            {
                return string.Format("{0} {1}", Math.Ceiling((double)day / 30.4), GlobalValuesAndStrings.SomeMonth);                
            }
            if (day < 365)
            {
                return string.Format("{0} {1}", Math.Ceiling((double)day / 30.4), GlobalValuesAndStrings.ManyMonth);                
            }
            if (day < 730)
            {
                return GlobalValuesAndStrings.OneYear;
            }
            if (day >= 730)
            {
                return string.Format("{0} {1} {2}", GlobalValuesAndStrings.MoreThan, Math.Floor((double)day / 365.25),
                    GlobalValuesAndStrings.ManyYears);                                
            }
            return string.Empty;
        }

        public static string PrettyDateTime(this DateTime? date)
        {
            if (date == null) return string.Empty;
            return ((DateTime) date).PrettyDateTime();
        }
    }
}