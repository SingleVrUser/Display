using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ByteSizeLib;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileListPage : Page
    {
        ObservableCollection<AccountContentInPage> AccountInPage = new ObservableCollection<AccountContentInPage>();
        ObservableCollection<DisplayInfo> groups;
        List<Datum> datumList;
        (int, int) DisplayRange = (0, 50);

        public FileListPage()
        {
            this.InitializeComponent();
        }

        private void DisplayInfo()
        {
            if (groups != null)
            {
                groups.Clear();
            }

            nowPageButton.Content = (DisplayRange.Item1 / (DisplayRange.Item2 - DisplayRange.Item1)) + 1;

            //////只挑选前五十个
            //var count = 50;

            //// 显示全部
            //var count = datumList.Count;
            for (var i = DisplayRange.Item1; i < DisplayRange.Item2; i++)
            {
                if (i >= datumList.Count) return;
                //文件夹跳过
                if (datumList[i].s == 0)
                {
                    //只显示前有限个时使用，确保显示的指定数量
                    //count++;
                    continue;
                }

                var FileSize = ByteSize.FromBytes(datumList[i].s);
                var info = new DisplayInfo
                {
                    name = datumList[i].n,
                    size = FileSize.ToString("#.#"),
                    modifyTime = datumList[i].t,
                    coverImage = "/Assets/SmallTile.scale-200.png"
                };

                string[] videotype = { "mp4", "wmv", "iso", "avi", "mkv", "rmvb", "ts" };
                string[] imagetype = { "jpg", "png", "jpeg", "gif" };
                string[] audiotyep = { "mp3" };
                string[] texttype = { "ini", "nfo", "chm", "html", "htm", "mht", "url", "txt", "pdf" };
                string[] rartype = { "rar", "zip" };
                string[] subtitlestype = { "srt", "ass", "sbu", "idx" };
                string[] torrenttype = { "torrent" };

                //视频文件
                if (videotype.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/icons8-circled-play-96.png";
                }
                else if (imagetype.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/icons8-image-96.png";
                }
                else if (audiotyep.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/icons8-music-96.png";
                }
                else if (texttype.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/icons8-page-96.png";
                }
                else if (rartype.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/icons8-archive-folder-96.png";
                }
                else if (subtitlestype.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/subtitles.png";
                }
                else if (torrenttype.Contains(datumList[i].ico))
                {
                    info.coverImage = "/Assets/torrent-symbol-file-format.png";
                }

                //图片地址有有效期，此方法已失效
                //if (datumList[i].u != "")
                //{
                //    var url = datumList[i].u;
                //    info.coverImage = url;
                //}
                groups.Add(info);
            }
        }

        /// <summary>
        /// 加载数据库的数据
        /// </summary>
        private void loadData()
        {
            ProgressRing.IsActive = true;
            //groups = new ObservableCollection<Datum>();
            //BaseExample.ItemsSource = groups;

            datumList = DataAccess.LoadDataAccess();

            if (datumList.Count == 0) return;

            DisplayInfo();

            FileOverview.Message = $"当前显示{DisplayRange.Item1}到{DisplayRange.Item2}的文件";

            ProgressRing.IsActive = false;

        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Grid加载时调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_loaded(object sender, RoutedEventArgs e)
        {
            groups = new ObservableCollection<DisplayInfo>();
            BaseExample.ItemsSource = groups;

            for (var i = 50; i < 500; i += 50)
            {
                AccountInPage.Add(new AccountContentInPage()
                {
                    ContentAcount = i,
                });
            }
            //默认选中第二项
            ContentAcountListView.SelectedIndex = 1;

            ContentAcountListView.ItemsSource = AccountInPage;


            loadData();
        }

        /// <summary>
        /// 添加一页显示数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AddAccountPage_NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newValue = (int)args.NewValue;
            if (newValue != double.NaN && newValue > 0)
            {
                AccountContentInPage newAccount = new AccountContentInPage()
                {
                    ContentAcount = newValue
                };

                //排除重复项
                var item = AccountInPage.Where(i => i.ContentAcount == newValue);
                if (item.Count() == 0)
                {
                    AccountInPage.Add(newAccount);
                }

                //排序
                var sortedItemsList = AccountInPage.OrderBy(i => i.ContentAcount).ToList();
                foreach (var sortedItem in sortedItemsList)
                {
                    AccountInPage.Move(AccountInPage.IndexOf(sortedItem), sortedItemsList.IndexOf(sortedItem));
                }
            }

        }

        /// <summary>
        /// 删除一页显示数量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

            var item = (sender as Button).DataContext as AccountContentInPage;

            //删除项为选中项
            if (item == ContentAcountListView.SelectedItem)
            {
                //数量两个以上
                if (AccountInPage.Count() > 1)
                {
                    //存在下一项
                    if (ContentAcountListView.SelectedIndex + 2 <= AccountInPage.Count)
                    {
                        ContentAcountListView.SelectedIndex++;
                    }
                    else
                    {
                        ContentAcountListView.SelectedIndex--;
                    }
                }

            }
            AccountInPage.Remove(item);

            //数量为零，添加一项
            if (AccountInPage.Count() == 0)
            {
                AccountInPage.Add(new AccountContentInPage()
                {
                    ContentAcount = 50,
                });
                ContentAcountListView.SelectedIndex = 0;

            }
        }

        private void NextPageButton(object sender, RoutedEventArgs e)
        {
            int differenceValue = DisplayRange.Item2 - DisplayRange.Item1;
            int startIndex = DisplayRange.Item1 + differenceValue;
            int endIndex = DisplayRange.Item2 + differenceValue;
            if (endIndex <= datumList.Count)
            {
                DisplayRange = (startIndex, endIndex);
                FileOverview.Message = $"当前显示{DisplayRange.Item1}到{DisplayRange.Item2}的文件";
                DisplayInfo();
            }
        }

        private void previousPageButton(object sender, RoutedEventArgs e)
        {
            int differenceValue = DisplayRange.Item2 - DisplayRange.Item1;
            int startIndex = DisplayRange.Item1 - differenceValue;
            int endIndex = DisplayRange.Item2 - differenceValue;
            if (startIndex >= 0)
            {
                DisplayRange = (startIndex, endIndex);
                FileOverview.Message = $"当前显示{DisplayRange.Item1}到{DisplayRange.Item2}的文件";
                DisplayInfo();
            }
        }
    }
}
