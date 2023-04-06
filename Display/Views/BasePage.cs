using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views
{
    public class BasePage
    {
        public static async void ShowTeachingTip(TeachingTip lightDismissTeachingTip, string subtitle, string content = null)
        {
            if (lightDismissTeachingTip.IsOpen) lightDismissTeachingTip.IsOpen = false;

            lightDismissTeachingTip.Subtitle = subtitle;

            if (content != null)
                lightDismissTeachingTip.Content = content;

            lightDismissTeachingTip.IsOpen = true;

            await Task.Delay(1000);

            if (lightDismissTeachingTip.IsOpen) lightDismissTeachingTip.IsOpen = false;
        }
    }
}
