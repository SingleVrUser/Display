using Display.WindowView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MorePage : Page
    {
        ObservableCollection<FunctionModule> DataSource = new();

        public MorePage()
        {
            this.InitializeComponent();

            //FunctionTreeView.ItemsSource = FunctionModuleList;
            OnLoaded();
        }

        private void OnLoaded()
        {
            DataSource.Add(new FunctionModule()
            {
                Name = "文件列表",
                IconPath = "/Assets/Svg/file_alt_icon.svg",
                Description = "115中的文件列表"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "搜刮信息",
                IconPath = "/Assets/Svg/find_internet_magnifier_search_security_icon.svg",
                Description = "搜刮本地数据库中视频对应的信息"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "演员头像",
                IconPath = "/Assets/Svg/face_male_man_portrait_icon.svg",
                Description = "从gfriends仓库中获取演员头像"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "缩略图",
                IconPath = "/Assets/Svg/image_icon.svg",
                Description = "获取视频缩略图"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "浏览器",
                IconPath = "/Assets/Svg/explorer_internet_logo_logos_icon.svg",
                Description = "115网页版，并附加下载选项"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "导入115数据",
                IconPath = "/Assets/Svg/import_icon.svg",
                Description = "导入115数据到本地数据库",
                Label = "即将弃用"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "计算Sha1",
                IconPath = "/Assets/Svg/accounting_banking_business_calculate_calculator_icon.svg",
                Description = "计算本地文件的Sha1",
                Label = "测试中"
            });
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickItem = e.ClickedItem as FunctionModule;
            switch (clickItem?.Name)
            {
                case "导入115数据":
                    SingleFrameWindow.CreateWindow(typeof(ContentsPage.Import115DataToLocalDataAccess.MainPage));
                    break;
                case "搜刮信息":
                    CommonWindow.CreateAndShowWindow(new ContentsPage.SpiderVideoInfo.MainPage());
                    break;
                case "计算Sha1":
                    CommonWindow.CreateAndShowWindow(new ContentsPage.CalculateLocalFileSha1());
                    break;
                case "浏览器":
                    var window4 = new CommonWindow();
                    window4.Content = new BrowserPage(window4);
                    window4.Activate();
                    break;
                case "文件列表":
                    CommonWindow.CreateAndShowWindow(new ContentsPage.DatumList.FileListPage());
                    break;
                case "演员头像":
                    CommonWindow.CreateAndShowWindow(new ContentsPage.AddActorCover());
                    break;
                case "缩略图":
                    CommonWindow.CreateAndShowWindow(new ContentsPage.GetThumbnail());
                    break;
            }
        }
    }

    public class FunctionModule
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public string Label { get; set; }
        public Visibility IsRightTopLabelShow
        {
            get { return string.IsNullOrEmpty(Label) ? Visibility.Collapsed : Visibility.Visible; }
        }
    }

}
