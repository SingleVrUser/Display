using System;
using System.ComponentModel;

namespace Display.Extensions
{
    internal static class EnumExtension
    {
        public static string GetCustomDescription(object objEnum)
        {
            var content = objEnum.ToString();
            if (string.IsNullOrEmpty(content)) return content;

            var fi = objEnum.GetType().GetField(content);
            if (fi == null) return content;

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : content;
        }

        public static string GetDescription(this Enum value)
        {
            return GetCustomDescription(value);
        }

        public static TEnum ParseEnumByDescription<TEnum>(this string str, TEnum defaultValue = default) where TEnum : Enum
        {
            var type = typeof(TEnum);
            var infos = type.GetFields();

            foreach (var info in infos)
            {
                var attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes.Length <= 0) continue;


                var description = attributes[0].Description;
                if (description == str) return (TEnum)Enum.Parse(type, info.Name);
            }

            return defaultValue;
        }
    }
}
