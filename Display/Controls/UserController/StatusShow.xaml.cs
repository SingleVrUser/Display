using Display.Models.Dto.OneOneFive;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls.UserController
{
    public sealed partial class StatusShow : UserControl
    {
        public Status status
        {
            get => (Status)GetValue(statusProperty);
            set
            {
                SetValue(statusProperty, value);
                switch (value)
                {
                    case Status.BeforeStart:
                        doing_status.Visibility = Visibility.Visible;
                        doing_status.IsIndeterminate = false;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.Doing:
                        doing_status.Visibility = Visibility.Visible;
                        doing_status.IsIndeterminate = true;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.Success:
                        doing_status.Visibility = Visibility.Collapsed;
                        success_status.Visibility = Visibility.Visible;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.Pause:
                        doing_status.Visibility = Visibility.Visible;
                        doing_status.IsIndeterminate = false;
                        doing_status.Value = 50;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.Error:
                        doing_status.Visibility = Visibility.Collapsed;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        public static readonly DependencyProperty statusProperty =
            DependencyProperty.Register("status", typeof(Status), typeof(StatusShow), null);

        public StatusShow()
        {
            this.InitializeComponent();
        }

    }
}
