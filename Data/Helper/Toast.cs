using Microsoft.Toolkit.Uwp.Notifications;

namespace Display.Helper
{
    public class Toast
    {
        public static void tryToast(string Title, string content1, string content2 = "")
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 384928)

                .AddText(Title)

                .AddText(content1)

                .AddText(content2)

                .Show();
        }
    }
}
