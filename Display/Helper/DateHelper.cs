using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Display.Helper
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

            string formatStr;

            if (second < 60)
            {
                formatStr = "ss'秒'";
            }
            else if (second < 3600)
            {
                formatStr = "mm'分'ss'秒'";
            }
            else if (second < 86400)
            {
                formatStr = "hh'小时'mm'分'ss'秒'";
            }
            else
            {
                formatStr = "dd'天'hh'小时'mm'分'ss'秒'";
            }

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
    }
}
