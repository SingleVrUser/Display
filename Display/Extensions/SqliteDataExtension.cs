using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Display.Data;
using System.Xml.Linq;
using ABI.Windows.Devices.Printers;

namespace Display.Extensions
{
    internal static class SQLiteDataExtension
    {
        /// <summary>
        /// 获取key对应类型的值，key必须与Filed名称一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static T GetNullableFieldValue<T>(this SqliteDataReader reader, string key)
        {
            var obj = reader[key];
            var stringFormat = obj.ToString();

            var type = typeof(T);
            var isNullOrEmpty = string.IsNullOrEmpty(stringFormat);
            var underlyingType = Nullable.GetUnderlyingType(type);
            // 可为空
            var nullable = underlyingType != null;

            // 如果nullable且为空，返回默认（null）
            if (isNullOrEmpty && nullable)
            {
                return default;
            }
            
            var trueType = nullable ? underlyingType : type;

            object finalValue = null;
            if (trueType == typeof(string))
            {
                finalValue = stringFormat;
            }
            else if (trueType == typeof(int))
            {
                finalValue = int.TryParse(stringFormat, out var result) ? result : -1;
            }
            else if (trueType == typeof(long))
            {
                finalValue = long.TryParse(stringFormat, out var result) ? result : -1;
            }
            else if (trueType == typeof(double))
            {
                finalValue = double.TryParse(stringFormat, out var result) ? result : -1;
            }

            return (T)finalValue;

            //// 不可靠, 结果为空时异常
            //var stringFormat = reader.GetString(key);
            //// 同上
            //return reader.GetFieldValue<T>(key);
        }

        /// <summary>
        /// Reader导出成另外的格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Export<T>(this SqliteDataReader reader) where T : new()
        {
            var data = new T();
            foreach (var propertyInfo in data.GetType().GetProperties())
            {
                // 获取属性对应表字段的名称
                var name = propertyInfo.Name;
                var attrs = propertyInfo.GetCustomAttributes().OfType<JsonPropertyAttribute>().ToArray();

                if (attrs.Length == 0 || attrs.First().PropertyName is null) continue;

                var fieldName = attrs.First().PropertyName;

                // 获取属性对应的Type
                var type = propertyInfo.PropertyType;
                if (type == typeof(string))
                {
                    propertyInfo.SetValue(data, reader.GetNullableFieldValue<string>(fieldName));
                }
                else if (type == typeof(int))
                {
                    propertyInfo.SetValue(data, reader.GetNullableFieldValue<int>(fieldName));
                }
                else if (type == typeof(long))
                {
                    propertyInfo.SetValue(data, reader.GetNullableFieldValue<long>(fieldName));
                }
                else if (type == typeof(long?))
                {
                    propertyInfo.SetValue(data, reader.GetNullableFieldValue<long?>(fieldName));
                }
                else if (type == typeof(double))
                {
                    propertyInfo.SetValue(data, reader.GetNullableFieldValue<long>(fieldName));
                }else if (type == typeof(Datum))
                {
                    propertyInfo.SetValue(data, reader.Export<Datum>());
                }
            }

            return data;
        }

    }
}
