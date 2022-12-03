using ByteSizeLib;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Converter;

public class BytesToSizeStrConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || !targetType.IsAssignableFrom(typeof(string)))
        {
            return string.Empty;
        }
        else if (value.ToString() == "0")
        {
            return string.Empty;
        }

        var size = ByteSize.FromBytes((long)value);

        return size.ToString("#.#");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
