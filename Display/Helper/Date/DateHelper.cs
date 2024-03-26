using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Display.Helper.Date
{
    internal class DateHelper
    {

        public static TimeSpan CalculateTimeStrDiff(string dt1Str, string dt2Str)
        {
            if (string.IsNullOrEmpty(dt1Str) || string.IsNullOrEmpty(dt2Str)) return TimeSpan.Zero;

            var dt1 = Convert.ToDateTime(dt1Str);

            var dt2 = Convert.ToDateTime(dt2Str);

            if (dt2 > dt1)
            {
                return dt2 - dt1;
            }

            return dt1 - dt2;
        }

        public static int ConvertDateTimeToInt32(string dateStr)
        {
            var dt1 = new DateTime(1970, 1, 1, 8, 0, 0);
            var dt2 = Convert.ToDateTime(dateStr);
            return Convert.ToInt32((dt2 - dt1).TotalSeconds);
        }

        public static string ConvertInt32ToDateTime(int dateInt)
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0);
            startTime = startTime.AddSeconds(dateInt).ToLocalTime();

            var result = $"{startTime:yyyy/MM/dd HH:mm}";

            return result;
        }

        public static string ConvertDoubleToLengthStr(double second)
        {
            if (second is double.NaN)
                return second.ToString(CultureInfo.InvariantCulture);

            var formatStr = second switch
            {
                < 60 => "ss'秒'",
                < 3600 => "mm'分'ss'秒'",
                < 86400 => "hh'小时'mm'分'ss'秒'",
                _ => "dd'天'hh'小时'mm'分'ss'秒'"
            };

            var ts = TimeSpan.FromSeconds(second);

            return ts.ToString(formatStr);
        }


        public static string ConvertPtTimeToTotalMinute(string ptTimeStr)
        {
            var totalMinute = 0;

            var match = Regex.Match(ptTimeStr, @"PT((\d+)H)?(\d+)M(\d+)S", RegexOptions.IgnoreCase);
            if (!match.Success) return ptTimeStr;

            if (int.TryParse(match.Groups[2].Value, out var result))
            {
                totalMinute += result * 60;
            }

            if (int.TryParse(match.Groups[3].Value, out result))
            {
                totalMinute += result;
            }

            return $"{totalMinute}分钟";

        }

        public static void GetTimeFromTimeStamp(long currentTime, out int hh, out int mm, out int ss)
        {
            var ns = (int)(currentTime / 1000000L);
            hh = ns / 3600;
            mm = ns % 3600 / 60;
            ss = ns % 60;
        }
    }
}
