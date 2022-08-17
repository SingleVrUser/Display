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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetThumbnail : Page
    {
        ObservableCollection<ThumbnailInfo> thumbnailInfo = new();

        CancellationTokenSource s_cts = new();

        ObservableCollection<string> failList = new();

        public GetThumbnail()
        {
            this.InitializeComponent();
        }

        private async void LoadedData()
        {
            List<VideoInfo> VideoInfoList = await Task.Run(() => DataAccess.LoadAllVideoInfo(-1));

            //TotalProgressBar.Maximum = VideoInfoList.Count;
            //TotalProgressBar.Value = 0;
            //TotalProgressBar.Visibility = Visibility.Visible;

            //加载时间过长，弃用
            //Dictionary<string, List<Datum>> ThumnailInfoDict = await Task.Run(() =>
            //{
            //    Dictionary<string, List<Datum>> ThumnailInfoDict = new();
            //    foreach (var VideoInfo in VideoInfoList)
            //    {
            //        string videoName = VideoInfo.truename;

            //        //List<Datum> videoInfoList = DataAccess.loadVideoInfoByTruename(videoName);

            //        //var actor_list = actor_str.Split(",");
            //        //foreach (var actor in actor_list)
            //        //{
            //        //    //名字为空
            //        //    if (actor == String.Empty) continue;

            //        //    //当前名称不存在
            //        //    if (!ActorsInfoDict.ContainsKey(actor))
            //        //    {
            //        //        ActorsInfoDict.Add(actor, new List<string>());
            //        //    }
            //        //    ActorsInfoDict[actor].Add(VideoInfo.truename);
            //        //}
            //        //thumbnailInfo.Add(new ThumbnailInfo() { name= videoName });

            //        ////当前名称不存在
            //        //if (!ThumnailInfoDict.ContainsKey(videoName))
            //        //{
            //        //    ThumnailInfoDict.Add(videoName, videoInfoList);
            //        //}
            //    }

            //    return ThumnailInfoDict;
            //});

            foreach (var item in VideoInfoList)
            {
                thumbnailInfo.Add(new() { name = item.truename , thumbnailDownUrlList = item.sampleImageList.Split(',').ToList()});
                //TotalProgressBar.Value++;
            }

            //TotalProgressBar.Visibility = Visibility.Collapsed;
        }

        private void BasicGridView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadedData();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BasicGridView.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BasicGridView.SelectedItems.Clear();
        }

        private void BasicGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItemCount = BasicGridView.SelectedItems.Count;

            selectedCheckBox.Content = $"共选 {selectedItemCount} 项";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (BasicGridView.SelectedItems.Count == 0) return;

            updateGridViewShow();

            var startTime = DateTimeOffset.Now;

            //进度
            var progress = new Progress<progressClass>(info =>
            {
                if (!progress_TextBlock.IsLoaded)
                {
                    s_cts.Cancel();
                    return;
                }

                progress_TextBlock.Text = info.text;

                if (info.index == -1)
                {
                    return;
                }

                var item = thumbnailInfo[info.index];

                item.Status = info.status;

                if (item.Status == Status.error)
                {
                    failList.Add(item.name);
                }

                if (!string.IsNullOrEmpty(info.imagePath))
                {
                    item.PhotoPath = info.imagePath;
                }

                //完成
                if (item.Status != Status.doing && thumbnailInfo.Count == info.index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{FileMatch.ConvertInt32ToDateStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
                }
            });

            Task.Run(() => GetThumbnailFromUrl(thumbnailInfo.ToList(), progress, s_cts));

            //BasicGridView.ItemClick += BasicGridView_ItemClick;

        }

        private async void GetThumbnailFromUrl(List<ThumbnailInfo> thumbnailinfos, IProgress<progressClass> progress, CancellationTokenSource s_cts)
        {
            for(int i = 0;i < thumbnailinfos.Count;i++)
            //foreach(var thumbnail in thumbnailinfos)
            {
                progressClass progressinfo = new();
                progressinfo.index = i;
                progressinfo.status = Status.doing;

                var thumbnail = thumbnailinfos[i];
                var DownUrlList = thumbnail.thumbnailDownUrlList;
                
                progressinfo.text = $"【{i+1}/{thumbnailinfos.Count}】{thumbnail.name}  （{DownUrlList.Count}张）";
                progress.Report(progressinfo);


                string imagePath = string.Empty;
                for (int j = 0;j< DownUrlList.Count; j++)
                {
                    string SavePath = Path.Combine(AppSettings.Image_SavePath,thumbnail.name);
                    string SaveName = $"Thumbnail_{j}";
                    string saveImagePath = await GetInfoFromNetwork.downloadImage(DownUrlList[j],SavePath,SaveName);
                    //Debug.WriteLine($@"下载：{} => {SavePath} => {SaveName}");

                    if(j == 1)
                    {
                        imagePath = saveImagePath;
                        //imagePath = Path.Combine(SavePath, SaveName, Path.GetExtension(DownUrlList[j]));
                    }

                    //Thread.Sleep(1000);
                }

                if (string.IsNullOrEmpty(imagePath))
                {
                    progressinfo.status = Status.error;
                }
                else
                {
                    progressinfo.imagePath = imagePath;
                    progressinfo.status = Status.beforeStart;
                }

                progress.Report(progressinfo);
            }
        }

        /// <summary>
        /// 更新GridView显示:
        /// 只保留选中项
        /// 退出选择模式
        /// </summary>
        private void updateGridViewShow()
        {
            List<ThumbnailInfo> thumbnailList = new();

            foreach (var item in BasicGridView.SelectedItems)
            {
                thumbnailList.Add(item as ThumbnailInfo);
            }

            thumbnailInfo.Clear();

            foreach (var actor in thumbnailList)
            {
                thumbnailInfo.Add(actor);
            }

            BasicGridView.SelectionMode = ListViewSelectionMode.None;

            selectedCheckBox.Visibility = Visibility.Collapsed;

            StartButton.Visibility = Visibility.Collapsed;
        }

        private class progressClass
        {
            public int index { get; set; } = -1;
            public string imagePath { get; set; }
            public string text { get; set; }
            public Status status { get; set; }
        }
    }

}
