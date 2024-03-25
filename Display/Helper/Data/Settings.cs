using System;
using System.Diagnostics;
using System.Text.Json;
using Windows.Storage;

namespace Display.Helper.Data;

internal static class Settings
{
    private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

    public static void SetValue<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (value is null)
        {
            LocalSettings.Values.Remove(key);
        }
        else if (value is string or int or double)
        {
            LocalSettings.Values[key] = value;
        }
        else
        {
            LocalSettings.Values[key] = JsonSerializer.Serialize(value);

            // LocalSettings 最大设置 8KB (8 * 1024 bytes)
            //Debug.WriteLine($"占用字节数：{System.Text.Encoding.Default.GetByteCount(JsonSerializer.Serialize(value))}");
        }
    }

    public static T GetValue<T>(string key, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(key)) return defaultValue;

        var value = LocalSettings.Values[key];

        switch (value)
        {
            case T t:
                return t;
            case string s:
                T result;
                try
                {
                    result = JsonSerializer.Deserialize<T>(s);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return defaultValue;
                }
                return result;

            default:
                return defaultValue;
        }

    }
}