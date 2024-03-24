using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.System;
using Display.Managers;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Display.Models.Settings.Options;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.SpiderVideoInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConditionalCheck : Page
    {
        ObservableCollection<ConditionCheck> ConditionCheckItems;
        ConditionCheck ImageItem;

        MainPage lastPage;

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
            ConditionCheckItems = [];

            //显示UI

            ImageItem = new ConditionCheck()
            {
                Condition = "图片存放地址",
                CheckUrl = AppSettings.ImageSavePath,
                CheckUrlRoutedEventHandler = ImageCheckButton_Click,

            };

            ConditionCheckItems.Add(ImageItem);

            var spiders = SpiderManager.Spiders;

            var onSpiders = spiders.Where(spider=> spider.IsOn);

            if (!onSpiders.Any())
            {
                spiderOrigin_TextBlock.Visibility = Visibility.Visible;
                return;
            }

            foreach (var spider in SpiderManager.Spiders)
            {
                ConditionCheckItems.Add(new ConditionCheck
                {
                    Condition = $"访问{spider.Abbreviation}",
                    CheckUrl = spider.BaseUrl,
                });
            }

            Check_Condition();
        }
        
        private async void ImageCheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not HyperlinkButton { DataContext: ConditionCheck item }) return;

            var folder = await StorageFolder.GetFolderFromPathAsync(item.CheckUrl);

            await Launcher.LaunchFolderAsync(folder);
        }

        private async void Check_Condition()
        {
            var isAllSuccess = true;
            foreach (var item in ConditionCheckItems)
            {
                item.Status = Status.Doing;

                if (item.CheckUrl.Contains("http"))
                {
                    var isUseful = await GetInfoFromNetwork.CheckUrlUseful(item.CheckUrl);

                    //网络有用
                    if (isUseful)
                    {
                        item.Status = Status.Success;
                    }
                    else
                    {
                        item.Status = Status.Error;
                        isAllSuccess = false;
                    }
                }
                //图片路径检查
                else
                {
                    var isExistsImageSavePath = Directory.Exists(item.CheckUrl);

                    item.Status = isExistsImageSavePath ? Status.Success : Status.Error;
                }

            }

            if (!isAllSuccess) Error_TextBlock.Visibility = Visibility.Visible;
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

        private Status _status = Status.BeforeStart;
        public Status Status
        {
            get => _status;
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
