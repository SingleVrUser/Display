using Display.WindowView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
                Name = "导入115数据",
                IconPath = "/Assets/Svg/import_icon.svg",
                Description = "导入115数据到本地数据库"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "搜刮信息",
                IconPath = "/Assets/Svg/find_internet_magnifier_search_security_icon.svg",
                Description = "搜刮本地数据库中视频对应的信息"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "文件列表",
                IconPath = "/Assets/Svg/file_alt_icon.svg",
                Description = "115中的文件列表",
                Label = "测试中"
            });
            DataSource.Add(new FunctionModule()
            {
                Name = "浏览器",
                IconPath = "/Assets/Svg/explorer_internet_logo_logos_icon.svg",
                Description = "使用 WebView2 浏览115内容",
                Label = "测试中"
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
            var ClickItem = e.ClickedItem as FunctionModule;
            switch (ClickItem.Name)
            {
                case "导入115数据":
                    SingleFrameWindow window1 = new SingleFrameWindow();
                    window1.NavigationToPageWithWebView(typeof(ContentsPage.Import115DataToLocalDataAccess), window1);
                    window1.Activate();
                    break;
                case "搜刮信息":
                    WindowView.CommonWindow window2 = new WindowView.CommonWindow();
                    window2.Content = new ContentsPage.SpiderVideoInfo();
                    window2.Activate();
                    break;
                case "计算Sha1":
                    WindowView.CommonWindow window3 = new WindowView.CommonWindow();
                    window3.Content = new ContentsPage.CalculateLocalFileSha1();
                    window3.Activate();
                    break;
                case "浏览器":
                    WindowView.CommonWindow window4 = new WindowView.CommonWindow();
                    window4.Content = new BrowserPage(window4);
                    window4.Activate();
                    break;
                case "文件列表":
                    WindowView.CommonWindow window5 = new WindowView.CommonWindow();
                    window5.Content = new FileListPage();
                    window5.Activate();
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
