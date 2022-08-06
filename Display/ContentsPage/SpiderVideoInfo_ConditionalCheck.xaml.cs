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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using static Data.FilesInfo;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpiderVideoInfo_ConditionalCheck : Page
    {
        private WebApi webapi;
        ObservableCollection<ConditionCheck> ConditionCheckItems;
        ConditionCheck JavbusItem;
        ConditionCheck JavdbItem;
        ConditionCheck ImageItem;

        SpiderVideoInfo lastPage;
        GetInfoFromNetwork network;


        public SpiderVideoInfo_ConditionalCheck(ContentsPage.SpiderVideoInfo page)
        {
            this.InitializeComponent();
            lastPage = page;

            IntitializeView();
        }

        public SpiderVideoInfo_ConditionalCheck()
        {
            this.InitializeComponent();

            IntitializeView();

        }

        private void IntitializeView()
        {
            webapi = new();
            ConditionCheckItems = new();
            network = new();

            //显示UI
            JavbusItem = new ConditionCheck()
            {
                Condition = "访问 JavBus",
                CheckUrl = AppSettings.JavBus_BaseUrl,
            };

            JavdbItem = new ConditionCheck()
            {
                Condition = "访问 JavDB",
                CheckUrl = AppSettings.JavDB_BaseUrl,
            };
            ImageItem = new ConditionCheck()
            {
                Condition = "图片存放地址",
                CheckUrl = AppSettings.Image_SavePath,
                CheckUrlRoutedEventHandler = ImageCheckButton_Click,
            };

            ConditionCheckItems.Add(ImageItem);
            ConditionCheckItems.Add(JavbusItem);
            ConditionCheckItems.Add(JavdbItem);

            Check_Condition();
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
            foreach(var item in ConditionCheckItems)
            {
                item.Status = Status.doing;

                if (item.CheckUrl.Contains("http"))
                {
                    bool isUseful = await network.CheckUrlUseful(item.CheckUrl);

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


    public class ConditionCheck:INotifyPropertyChanged
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
