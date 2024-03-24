using System.Linq;
using System.Reflection;

namespace Display.Helper.Data;

internal static class ClassHelper
{
    public static bool UpdateOldInfoByNew<T>(ref T oldInfo, T newInfo)
    {
        var propertyInfos = typeof(T).GetProperties();

        var isCompleted = true;
        foreach (var propertyInfo in propertyInfos)
        {
            var oldValue = propertyInfo.GetValue(oldInfo);
            if (oldValue != null) continue;

            var newValue = propertyInfo.GetValue(newInfo);
            if (newValue == null)
            {
                isCompleted = false;
                continue;
            }

            propertyInfo.SetValue(oldInfo, newValue);
        }

        return isCompleted;
    }

    public static bool InfoIsCompleted<T>(T info,string[] alternativeList)
    {
        return info != null && info.GetType().GetProperties()
            .Where(propertyInfo => !alternativeList.Contains(propertyInfo.Name))
            .Select(propertyInfo => propertyInfo.GetValue(info)).All(value => value != null);
    }
}