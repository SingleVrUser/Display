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
            var result = defaultValue;
            if (!nullable)
            {
                if (type == typeof(string))
                {
                    result = (T)obj;
                }
                else if (type == typeof(long))
                {
                    if (isNullOrEmpty)
                    {
                        result = defaultValue;
                    }
                    else
                    {
                        result = (T)obj;
                    }
                }
            }
            else
            {
                if (underlyingType == typeof(string))
                {
                    result = (T)obj;
                }else if (underlyingType == typeof(long))
                {
                    if (isNullOrEmpty)
                    {
                        result = default;
                    }
                    else
                    {
                        result = (T)obj;
                    }
                }
            }

            return result;
        }

    }
}
