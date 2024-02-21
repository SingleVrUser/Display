
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views
{
    public class BasePage
    {
        public static async void ShowTeachingTip(TeachingTip lightDismissTeachingTip, string subtitle, string content = null)
        {
            object tipContent = !string.IsNullOrEmpty(content) ? new TextBlock {Text = content, TextTrimming = TextTrimming.CharacterEllipsis }
                                                                : null;

            await ShowTeachingTip(lightDismissTeachingTip, subtitle, tipContent);
        }

        public static async void ShowTeachingTip(TeachingTip lightDismissTeachingTip, string subtitle,
            string actionContent, TypedEventHandler<TeachingTip, object> actionButtonClick)
        {       
            await ShowTeachingTip(lightDismissTeachingTip,subtitle, actionContent, actionButtonClick, 2);
        }

        private static async System.Threading.Tasks.Task ShowTeachingTip(TeachingTip lightDismissTeachingTip, string subtitle,
            object content = null, TypedEventHandler<TeachingTip, object> actionButtonClick = null, int delaySecond = 1)
        {
            if (lightDismissTeachingTip.IsOpen) lightDismissTeachingTip.IsOpen = false;

            lightDismissTeachingTip.Subtitle = subtitle;

            if (actionButtonClick == null)
            {
                lightDismissTeachingTip.Content = content;
            }
            else
            {
                lightDismissTeachingTip.ActionButtonContent = content;
                lightDismissTeachingTip.ActionButtonClick += actionButtonClick;
                lightDismissTeachingTip.CloseButtonContent = "关闭";
            }

            lightDismissTeachingTip.IsOpen = true;

            await Task.Delay(delaySecond*1000);

            if (lightDismissTeachingTip.IsOpen) lightDismissTeachingTip.IsOpen = false;

            if (actionButtonClick != null)
            {
                lightDismissTeachingTip.ActionButtonClick -= actionButtonClick;
            }

        }
    }
}