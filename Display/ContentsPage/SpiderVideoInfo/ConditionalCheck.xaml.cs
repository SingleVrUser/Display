
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.System;
using Display.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SpiderVideoInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConditionalCheck : Page
    {
        ObservableCollection<ConditionCheck> ConditionCheckItems;
        ConditionCheck ImageItem;

        SpiderVideoInfo.MainPage lastPage;

        public ConditionalCheck(MainPage page)
        {
            this.InitializeComponent();
            lastPage = page;

            InitializeView();
        }

        public ConditionalCheck()
        {
            this.InitializeComponent();

            InitializeView();

        }

        private void InitializeView()
        {
            ConditionCheckItems = new();

            //显示UI

            ImageItem = new ConditionCheck()
            {
                Condition = "图片存放地址",
                CheckUrl = AppSettings.ImageSavePath,
                CheckUrlRoutedEventHandler = ImageCheckButton_Click,

            };

            ConditionCheckItems.Add(ImageItem);

            AddSpiderMethod(AppSettings.IsUseJavBus, "访问 JavBus", AppSettings.JavBus_BaseUrl);

            AddSpiderMethod(AppSettings.IsUseJav321, "访问 Jav321", AppSettings.Jav321_BaseUrl);

            AddSpiderMethod(AppSettings.IsUseAvMoo, "访问 AvMoo", AppSettings.AvMoo_BaseUrl);

            AddSpiderMethod(AppSettings.IsUseAvSox, "访问 AvSox", AppSettings.AvSox_BaseUrl);

            AddSpiderMethod(AppSettings.IsUseLibreDmm, "访问 LibreDmm", AppSettings.LibreDmm_BaseUrl);

            AddSpiderMethod(AppSettings.IsUseFc2Hub, "访问 Fc2hub", AppSettings.Fc2hub_BaseUrl);

            AddSpiderMethod(AppSettings.IsUseJavDb, "访问 JavDB", AppSettings.JavDB_BaseUrl);

            //至少选择一个搜刮源
            if (!(AppSettings.IsUseJavBus || AppSettings.IsUseLibreDmm || AppSettings.IsUseFc2Hub || AppSettings.IsUseJavDb))
            {
                spiderOrigin_TextBlock.Visibility = Visibility.Visible;
            }

            Check_Condition();
        }

        private void AddSpiderMethod(bool isAdd, string tip, string url)
        {
            if (isAdd)
            {
                ConditionCheckItems.Add(new ConditionCheck()
                {
                    Condition = tip,
                    CheckUrl = url,
                });
            }
        }

        private async void ImageCheckButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as ConditionCheck;
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item.CheckUrl as String);

            await Launcher.LaunchFolderAsync(folder);
        }

        private async void Check_Condition()
        {
            bool isAllSuccess = true;
            foreach (var item in ConditionCheckItems)
            {
                item.Status = Status.doing;

                if (item.CheckUrl.Contains("http"))
                {
                    bool isUseful = await GetInfoFromNetwork.CheckUrlUseful(item.CheckUrl);

                    //网络有用
                    if (isUseful)
                    {
                        item.Status = Status.success;
                    }
                    else
                    {
                        item.Status = Status.error;
                        isAllSuccess = false;
                    }
                }
                //图片路径检查
                else
                {
                    bool isExistsImageSavePath = Directory.Exists(item.CheckUrl);
                    if (isExistsImageSavePath)
                    {
                        item.Status = Status.success;
                    }
                    else
                    {
                        item.Status = Status.error;
                    }
                }

            }

            if (isAllSuccess)
            {
                tryUpdateLastPageStatus(Status.success);
            }
            else
            {
                tryUpdateLastPageStatus(Status.error);
                Error_TextBlock.Visibility = Visibility.Visible;
            }


        }


        /// <summary>
        /// 更新上一页面的状态信息
        /// </summary>
        private void tryUpdateLastPageStatus(Status status)
        {
            if (lastPage != null)
            {
                lastPage.LoginCheck.status = status;
            }
        }
        private void ClickOne_Click(object sender, RoutedEventArgs e)
        {
            Check_Condition();
        }

    }


    public class ConditionCheck : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Condition { get; set; }
        public string CheckUrl { get; set; }

        private Status _status = Status.beforeStart;
        public Status Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        private static void NullRoutedEventHandler(object sender, RoutedEventArgs e) { }

        public RoutedEventHandler CheckUrlRoutedEventHandler { get; set; } = NullRoutedEventHandler;
    }
}
