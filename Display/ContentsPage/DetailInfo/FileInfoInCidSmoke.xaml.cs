
using Display.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DetailInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileInfoInCidSmoke : Page
    {
        private string truename;

        public FileInfoInCidSmoke(string truename)
        {
            this.InitializeComponent();

            this.truename = truename;

            this.Loaded += PageLoad;
        }

        private async void PageLoad(object sender, RoutedEventArgs e)
        {
            var VideoInfos = await DataAccess.FindFileInfoByTrueName(truename);

            InfosListView.ItemsSource = VideoInfos;
        }
    }
}
