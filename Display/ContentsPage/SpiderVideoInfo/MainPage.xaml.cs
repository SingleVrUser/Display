using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.SpiderVideoInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private Model.IncrementalLoadingdFileCollection _failList;
        Model.IncrementalLoadingdFileCollection FailList
        {
            get => _failList;
            set
            {
                if (_failList == value)
                    return;

                _failList = value;

                OnPropertyChanged();
            }
        }

        private Datum _selectedDatum;
        public Datum SelectedDatum
        {
            get => _selectedDatum;
            set
            {
                if (_selectedDatum == value)
                    return;
                _selectedDatum = value;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 展开Expander初始化检查图片路径和网络
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (!(sender is Expander expander)) return;

            if ((expander.Content as ConditionalCheck) == null)
            {
                expander.Content = new ContentsPage.SpiderVideoInfo.ConditionalCheck(this);
            }

            expander.SetValue(Grid.ColumnProperty, 0);
            expander.SetValue(Grid.ColumnSpanProperty, 2);
        }


        /// <summary>
        /// 关闭Expander
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Expander_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
        {
            if (!(sender is Expander expander)) return;

            expander.SetValue(Grid.ColumnProperty, 1);
            expander.SetValue(Grid.ColumnSpanProperty, 1);
        }

        /// <summary>
        /// 点击匹配按钮开始匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartMatchName_ButtonClick(object sender, RoutedEventArgs e)
        {
            //检查是否有选中文件
            if (Explorer.FolderTreeView.SelectedNodes.Count == 0)
            {
                SelectNull_TeachintTip.IsOpen = true;
                return;
            }

            //获取需要搜刮的文件
            List<Datum> datumList = new();
            List<string> folderNameList = new();
            foreach (var node in Explorer.FolderTreeView.SelectedNodes)
            {
                var explorer = node.Content as ExplorerItem;

                if (explorer == null) continue;

                //文件夹
                folderNameList.Add(explorer.Name);

                //文件夹下的文件
                var items = Explorer.GetFilesFromItems(explorer.Cid, FilesInfo.FileType.File);

                datumList.AddRange(items);
            }

            //创建进度窗口
            var page = new ContentsPage.SpiderVideoInfo.Progress(folderNameList, datumList);
            page.CreateWindow();

        }

        /// <summary>
        /// 切换“本地数据库”或“匹配失败”显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is RadioButton radioButton)
            {
                if (radioButton.Name == nameof(matchFail_RadioButton))
                {
                    tryShowFailList();
                }
            }
        }

        /// <summary>
        /// 显示失败列表
        /// </summary>
        private async void tryShowFailList()
        {
            if (FailListView.ItemsSource == null)
            {
                FailList = new();
                FailListView.ItemsSource = FailList;
            }

            if (FailList.Count == 0)
            {
                //失败总数
                var failCount = await DataAccess.CheckFailFilesCount();
                FailListTotalCount_Run.Text = $"/{failCount}";

                //当前显示的
                var list = await DataAccess.LoadFailFileInfo(0,30);
                list.ForEach(item => FailList.Add(item));
            }
        }

        /// <summary>
        /// 点击显示文件信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerItemClick(object sender, ItemClickEventArgs e)
        {
            var itemInfo = e.ClickedItem as FilesInfo;

            if (FileInfoShow_Grid.Visibility == Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;
            SelectedDatum = itemInfo.datum;
        }

        /// <summary>
        /// 点击TreeView的Item显示文件信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var content = ((args.InvokedItem as TreeViewNode).Content as ExplorerItem);

            if(FileInfoShow_Grid.Visibility== Visibility.Collapsed) FileInfoShow_Grid.Visibility = Visibility.Visible;
            SelectedDatum = content.datum;
        }

        /// <summary>
        /// 点击失败列表显示文件信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FailListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
                return;

            SelectedDatum = e.AddedItems[0] as Datum;
        }

        /// <summary>
        /// 点击视频按钮播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(SelectedDatum== null) return;

            Views.DetailInfoPage.PlayeVideo(SelectedDatum.pc, this.XamlRoot);
        }

    }
    public class SpliderInfoProgress
    {
        public VideoInfo videoInfo { get; set; }
        public MatchVideoResult matchResult { get; set; }

        public int index { get; set; } = 0;
    }

    public enum FileFormat { Video, Subtitles, Torrent, Image, Audio,Archive }

    public class FileStatistics
    {
        public FileStatistics(FileFormat name)
        {
            type = name;
            size = 0;
            count = 0;
            data = new();
        }

        public FileFormat type { get; set; }
        public long size { get; set; }
        public int count { get; set; }
        public List<Data> data { get; set; }

        public class Data
        {
            public string name { get; set; }
            public int count { get; set; } = 0;
            public long size { get; set; } = 0;
        }
    }
}
