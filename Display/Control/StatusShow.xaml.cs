using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class StatusShow : UserControl
    {
        public Status status
        {
            get { return (Status)GetValue(statusProperty); }
            set
            {
                SetValue(statusProperty, value);
                switch (value)
                {
                    case Status.beforeStart:
                        doing_status.Visibility = Visibility.Visible;
                        doing_status.IsIndeterminate = false;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.doing:
                        doing_status.Visibility = Visibility.Visible;
                        doing_status.IsIndeterminate = true;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.success:
                        doing_status.Visibility = Visibility.Collapsed;
                        success_status.Visibility = Visibility.Visible;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.pause:
                        doing_status.Visibility = Visibility.Visible;
                        doing_status.IsIndeterminate = false;
                        doing_status.Value = 50;
                        success_status.Visibility = Visibility.Collapsed;
                        error_status.Visibility = Visibility.Collapsed;
                        break;
                    case Status.error:
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

    public class ConditionCheck
    {
        public string Condition { get; set; }
        public string CheckUrl { get; set; }
        public Status Status { get; set; }
    }
}
