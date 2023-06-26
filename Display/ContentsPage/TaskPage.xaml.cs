// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Display.Helper;
using Display.ViewModels;
using Display.WindowView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OpenCvSharp;
using WinUIEx;
using Window = Microsoft.UI.Xaml.Window;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TaskPage : Page
    {
        private readonly UploadViewModel _uploadVm = UploadViewModel.Instance;

#nullable enable
        private static Window? _window;
#nullable disable

        public TaskPage()
        {
            InitializeComponent();

        }

        public static void ShowSingleWindow()
        {
            if (_window == null)
            {
                _window = new CommonWindow("传输任务",842,537)
                {
                    Content = new TaskPage()
                };
                _window.Closed += (_,_) =>
                {
                    _window = null;
                };
                _window?.Show();
            }
            else
            {
                WindowHelper.SetForegroundWindow(_window);
            }
        }

    }
}
