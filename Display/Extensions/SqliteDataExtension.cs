using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Composition;
using Org.BouncyCastle.Crypto.Digests;
using SharpCompress;
using Windows.Foundation;

namespace Display.Extensions
{
    internal static class SQLiteDataExtension
    {
        public static T GetNullableValue<T>(this SqliteDataReader reader, string key,T defaultValue=default)
        {   
            var obj = reader[key];
            var stringFormat = obj.ToString();
            var type = typeof(T);
            var isNullOrEmpty = string.IsNullOrEmpty(stringFormat);

            var underlyingType = Nullable.GetUnderlyingType(type);

            // 可为空
            var nullable = underlyingType != null;

            var trueType = nullable ? underlyingType : type;

            var result = defaultValue;
            if (trueType == typeof(string))
            {   
                result = (T)obj;
            }
            else if (trueType == typeof(long))
            {
                if (isNullOrEmpty && nullable)
                {
                    result = default;
                }
                else if (long.TryParse(stringFormat, out var longResult))
                {
                    result = (T)(object)longResult;
                }
            }
            else if (trueType == typeof(int))
            {
                if (int.TryParse(stringFormat, out var intResult))
                {
                    result = (T)(object)intResult;
                }
            }

            return result;
        }

    }
}
