using Microsoft.Toolkit.Uwp.Notifications;

namespace Display.Helper.UI
{
    public class Toast
    {
        public static void TryToast(string title, string content1, string content2 = "")
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 384928)

                .AddText(title)

                .AddText(content1)

                .AddText(content2)

                .Show();
        }
    }
}
