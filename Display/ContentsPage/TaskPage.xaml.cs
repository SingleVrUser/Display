// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Display.ViewModels;
using Microsoft.UI.Xaml.Controls;

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
        public TaskPage()
        {
            this.InitializeComponent();
        }
    }
}
