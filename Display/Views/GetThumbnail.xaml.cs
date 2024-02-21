
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Display.Helper.Date;
using Display.Helper.FileProperties.Name;
using Display.Models.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetThumbnail : Page
    {
        List<ThumbnailInfo> _storeThumbnailInfo;
        ObservableCollection<ThumbnailInfo> thumbnailInfo = new();

        CancellationTokenSource s_cts = new();

        ObservableCollection<string> failList = new();

        public GetThumbnail()
        {
            this.InitializeComponent();
            LoadedData();
        }

        private async void LoadedData()
        {
            var videoInfoList = await Task.Run(() => DataAccess.Get.GetVideoInfo(-1));

            foreach (var item in videoInfoList)
            {
                thumbnailInfo.Add(new ThumbnailInfo(item));
            }
        }

        //private void BasicGridView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    LoadedData();
        //}

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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (BasicGridView.SelectedItems.Count == 0) return;

            updateGridViewShow();

            switch (OriginMethodSelected_ComboBox.SelectedIndex)
            {
                //搜刮源网站
                case 0:
                    StartGetThumbnailFromUrl();
                    break;
                //在线视频
                case 1:
                    //检查在线模型文件是否存在
                    if (GetImageByOpenCV.IsModelFilesExists)
                    {
                        StartGetThumbnailFromWebVideo();
                    }
                    else
                    {
                        ContentDialog dialog = new()
                        {
                            XamlRoot = XamlRoot,
                            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                            Title = "未找到训练模型",
                            CloseButtonText = "返回",
                            DefaultButton = ContentDialogButton.Close,
                            Content = "未找到模型文件，需要下载指定训练模型并存放到相应路径，才能继续此操作"
                        };

                        await dialog.ShowAsync();

                    }
                    break;
            }

        }

        private void StartGetThumbnailFromUrl()
        {

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

                if (item.Status == Status.Error)
                {
                    failList.Add(item.name);
                }

                if (!string.IsNullOrEmpty(info.imagePath))
                {
                    item.PhotoPath = info.imagePath;
                }

                //完成
                if (item.Status != Status.Doing && thumbnailInfo.Count == info.index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{DateHelper.ConvertDoubleToLengthStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
                }
            });

            Task.Run(() => GetThumbnailFromUrl(thumbnailInfo.ToList(), progress, s_cts));
        }

        private void StartGetThumbnailFromWebVideo()
        {
            var startTime = DateTimeOffset.Now;

            // 整体进度
            var overallProgress = new Progress<progressClass>(info =>
            {
                if (!IsLoaded)
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

                if (item.Status == Status.Error)
                {
                    failList.Add(item.name);
                }

                if (!string.IsNullOrEmpty(info.imagePath))
                {
                    item.PhotoPath = info.imagePath;
                }

                //完成
                if (item.Status != Status.Doing && thumbnailInfo.Count == info.index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{DateHelper.ConvertDoubleToLengthStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
                }

            });

            // 截图进度
            var getImageProgress = new Progress<progressInfo>();

            GetThumbnailFromWebVideoSetting getThumbnailFromWebVideoSetting = new GetThumbnailFromWebVideoSetting()
            {
                IsJumpVrVideo = IsJumpVrVideo_ToggleSwitch.IsOn,
                isDetectFaces = IsSelectedFaceImage_ToggleSwitch.IsOn,
                imageCount = (int)ScreenshotsNumber_NumberBox.Value,
                isShowWindows = IsShowWindows_ToggleSwitch.IsOn
            };


            Task.Run(() => GetThumbnailFromWebVideo(thumbnailInfo.ToList(), getThumbnailFromWebVideoSetting, overallProgress, getImageProgress, s_cts));
        }

        class GetThumbnailFromWebVideoSetting
        {
            public bool IsJumpVrVideo = true;
            public bool isDetectFaces = false;
            public bool isShowWindows = false;
            public int imageCount = 10;
        }

        private async void GetThumbnailFromWebVideo(List<ThumbnailInfo> thumbnailinfos, GetThumbnailFromWebVideoSetting getThumbnailFromWebVideoSetting, IProgress<progressClass> overallProgress, IProgress<progressInfo> getImageProgress, CancellationTokenSource s_cts)
        {
            var webApi = WebApi.GlobalWebApi;
            GetImageByOpenCV openCV = new(true);

            for (int i = 0; i < thumbnailinfos.Count; i++)
            {
                progressClass progressinfo = new();
                progressinfo.index = i;
                progressinfo.status = Status.Doing;

                overallProgress.Report(progressinfo);

                var name = thumbnailinfos[i].name;

                //最大获取数量
                int imageCount = getThumbnailFromWebVideoSetting.imageCount;

                string startText = $"【{i + 1}/{thumbnailinfos.Count}】 {name}";

                //保存路径
                string SavePath = Path.Combine(AppSettings.ImageSavePath, name);

                //缩略图已存在，跳过
                string existsPath = Path.Combine(SavePath, $"Thumbnail_{imageCount - 1}.jpg");
                if (File.Exists(existsPath))
                {
                    progressinfo.text = $"{startText} - 缩略图已存在，跳过该项目";
                    progressinfo.imagePath = existsPath;
                    progressinfo.status = Status.BeforeStart;
                    overallProgress.Report(progressinfo);
                    continue;
                };

                //跳过VR
                if (getThumbnailFromWebVideoSetting.IsJumpVrVideo && thumbnailinfos[i].isVr)
                {
                    progressinfo.text = $"{startText} - 跳过VR";
                    progressinfo.status = Status.BeforeStart;
                    overallProgress.Report(progressinfo);
                    continue;
                }

                //考虑多集视频
                var videoInfoList = DataAccess.Get.GetSingleFileInfoByTrueName(name);

                //检查视频是否已转码
                var videofileListAfterDecode = videoInfoList.Where(x => x.Vdi != 0).ToList();

                progressinfo.text = $"{startText} - 获取到视频（总数：{videoInfoList.Count}，转码完成数：{videofileListAfterDecode.Count}）";
                overallProgress.Report(progressinfo);

                //检查能否播放，获取总时长、总帧数
                Dictionary<int, double> VideoIndexAndFrameCountDict = new();

                VideoToThumbnail videoToThumbnail = new VideoToThumbnail();

                videoToThumbnail.name = name;

                for (int j = 0; j < videofileListAfterDecode.Count; j++)
                {
                    var videoInfo = videofileListAfterDecode[j];
                    var m3u8InfoList = await webApi.GetM3U8InfoByPickCode(videoInfo.PickCode);
                    if (m3u8InfoList.Count > 0)
                    {
                        ////总帧数计算
                        //var frameCount = openCV.getTotalFrameCount(m3u8InfoList[0].Url);
                        //videoToThumbnail.frame_count += frameCount;

                        ////总时长计算，现在已m3u8文件的时长为准，故弃用
                        //videoToThumbnail.play_long += videoInfo.play_long;

                        //添加到pickCodeList中
                        videoToThumbnail.pickCodeList.Add(videoInfo.PickCode);
                    }
                }

                progressinfo.text = $"{startText} - 获取到视频（总数：{videoInfoList.Count}，转码完成数：{videofileListAfterDecode.Count}，可播放数：{videoToThumbnail.pickCodeList.Count}）";
                overallProgress.Report(progressinfo);

                foreach (var pickCode in videoToThumbnail.pickCodeList)
                {
                    //重新获取（时间过长m3u8地址可能失效）
                    var m3u8InfoList = await webApi.GetM3U8InfoByPickCode(pickCode);

                    if (m3u8InfoList.Count == 0) continue;

                    //缩略图画质不需要太好，选择最后一个
                    var meu8Info = await webApi.GetM3U8Content(m3u8InfoList[^1]);

                    //总时长
                    var play_long = meu8Info.TotalSecond;

                    ////总帧数
                    //var length = videoToThumbnail.frame_count;


                    int startJumpSecond = 180;
                    int endJumpSecond = 10;

                    var averageLength = (play_long - (startJumpSecond + endJumpSecond)) / imageCount;

                    double currentTime = startJumpSecond;

                    bool isShowWindow = getThumbnailFromWebVideoSetting.isShowWindows;

                    int count = 0;

                    for (int j = 0; j < meu8Info.ts_info_list.Count && count < imageCount; j++)
                    {
                        var item = meu8Info.ts_info_list[j];

                        currentTime += item.Second;

                        if (currentTime > averageLength * count)
                        {
                            string imagePath = Path.Combine(SavePath, $"Thumbnail_{count}.jpg");


                            if (!File.Exists(imagePath))
                            {

                                bool isGetImage = false;
                                for (int tryCount = 0; tryCount < 20 && !isGetImage; tryCount++)
                                {
                                    if (j + tryCount >= meu8Info.ts_info_list.Count)
                                    {
                                        break;
                                    }

                                    var tsUrl = $"{meu8Info.BaseUrl}{meu8Info.ts_info_list[j + tryCount].Url}";

                                    isGetImage = openCV.Task_GetThumbnailByVideoPath(tsUrl, 0, isShowWindow, getImageProgress, SavePath, $"Thumbnail_{count}", getThumbnailFromWebVideoSetting.isDetectFaces);

                                    //Debug.WriteLine("获取链接中的图片");
                                    Debug.WriteLine($"【{tryCount + 1}/20】url:{tsUrl}\ncurrentTime:{currentTime}");
                                }

                                //多次尝试后仍未获取人脸信息，就不检测人脸了，随便截一张
                                if (!isGetImage)
                                {
                                    openCV.Task_GetThumbnailByVideoPath($"{meu8Info.BaseUrl}{item.Url}", 0, isShowWindow, getImageProgress, SavePath, $"Thumbnail_{count}", false);
                                }

                            }


                            progressinfo.imagePath = imagePath;
                            overallProgress.Report(progressinfo);

                            count++;
                        }

                    }

                    ////平均长度
                    //double averageLength = (videoToThumbnail.frame_count - (startJumpFrame_num + endJumpFrame_num)) / imageCount;

                    //double next_frame = startJumpFrame_num;
                    ////-startJumpFrame_num * 1 为消除 double -> int 误差
                    //for (double current_frame = next_frame; current_frame < length - 1; current_frame += averageLength)
                    //{
                    //    string tsUrl = string.Empty;
                    //    double startFrame = 0;
                    //    double tsStartFrame = 0;



                    //    if (string.IsNullOrEmpty(tsUrl))
                    //    {
                    //        continue;
                    //    }

                    //    string imagePath = Path.Combine(SavePath, $"Thumbnail_{count}.jpg");

                    //    if (!File.Exists(imagePath))
                    //    {

                    //        openCV.Task_GetThumbnailByVideoPath(tsUrl, tsStartFrame, isShowWindow, getImageProgress, SavePath, $"Thumbnail_{count}", getThumbnailFromWebVideoSetting.isDetectFaces, length - current_frame);
                    //        //Debug.WriteLine("获取链接中的图片");
                    //        Debug.WriteLine($"url:{tsUrl}\nstartFrame:{tsStartFrame}");
                    //    }

                    //    progressinfo.imagePath = imagePath;

                    //    overallProgress.Report(progressinfo);


                    //    count++;
                    //    next_frame = averageLength - (length - current_frame);

                    //}

                }

                progressinfo.status = Status.BeforeStart;
                overallProgress.Report(progressinfo);

            }
        }

        //private async Task<progressInfo> getThumbnialByVideoUrl(string url, int startJumpFrame_num = 100,int endJumpFrame_num = 0, bool isShowWindow = true)
        //{
        //    progressInfo progressInfo = new();

        //    GetImageByOpenCV openCV = new GetImageByOpenCV();

        //    var frames_num = openCV.getTotalFrameCount(url);
        //    if (frames_num == 0) return progressInfo;

        //    ////跳过开头
        //    //int startJumpFrame_num = 1000;

        //    ////跳过结尾
        //    //int endJumpFrame_num = 0;

        //    var actualEndFrame = frames_num - endJumpFrame_num;

        //    //平均长度
        //    var averageLength = (int)(actualEndFrame - startJumpFrame_num) / imageCount;

        //    //进度
        //    var progress = new Progress<progressInfo>(info =>
        //    {

        //        progressInfo = info;

        //    });

        //    double length = 100 * 100;

        //    await Task.Run(() => openCV.Task_GenderByVideo(url, startJumpFrame_num, length, isShowWindow, progress, SavePath));

        //    return progressInfo;
        //}


        private async void GetThumbnailFromUrl(List<ThumbnailInfo> thumbnailinfos, IProgress<progressClass> progress, CancellationTokenSource s_cts)
        {
            for (int i = 0; i < thumbnailinfos.Count; i++)
            //foreach(var thumbnail in thumbnailinfos)
            {
                progressClass progressinfo = new();
                progressinfo.index = i;
                progressinfo.status = Status.Doing;

                var thumbnail = thumbnailinfos[i];
                var DownUrlList = thumbnail.thumbnailDownUrlList;

                progressinfo.text = $"【{i + 1}/{thumbnailinfos.Count}】{thumbnail.name}  （{DownUrlList.Count}张）";
                progress.Report(progressinfo);

                string imagePath = string.Empty;
                for (int j = 0; j < DownUrlList.Count; j++)
                {
                    string SavePath = Path.Combine(AppSettings.ImageSavePath, thumbnail.name);
                    string SaveName = $"Thumbnail_{j}";
                    string saveImagePath = await GetInfoFromNetwork.DownloadFile(DownUrlList[j], SavePath, SaveName);

                    if (j == 1)
                    {
                        imagePath = saveImagePath;
                    }

                }

                if (string.IsNullOrEmpty(imagePath))
                {
                    progressinfo.status = Status.Error;
                }
                else
                {
                    progressinfo.imagePath = imagePath;
                    progressinfo.status = Status.BeforeStart;
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

        private void ShowVideoPart_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_storeThumbnailInfo == null)
            {
                _storeThumbnailInfo = thumbnailInfo.ToList();
            }

            foreach (var item in _storeThumbnailInfo)
            {
                if (item.thumbnailDownUrlList.Count > 0)
                {
                    thumbnailInfo.Remove(item);
                }
            }

        }

        private void ShowVideoPart_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_storeThumbnailInfo != null && _storeThumbnailInfo.Count > 0)
            {
                thumbnailInfo.Clear();
                foreach (var item in _storeThumbnailInfo)
                {
                    thumbnailInfo.Add(item);
                }
            }
        }

        private void OriginMethodSelected_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ComboBox = sender as ComboBox;
            switch (ComboBox.SelectedIndex)
            {
                case 0:
                    VisualStateManager.GoToState(this, "SelectedUrl", true);
                    break;
                case 1:
                    VisualStateManager.GoToState(this, "SelectedWebVideo", true);
                    break;
            }
        }

        private void OpenModelSavePath_HyperLinkClick(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            string path = Path.Combine(Package.Current.InstalledLocation.Path, @"Assets\Models\caffe");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FileMatch.LaunchFolder(path);
        }
    }

}
