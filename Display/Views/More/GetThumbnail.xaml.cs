using Display.Helper.Date;
using Display.Helper.FileProperties.Name;
using Display.Models.Data;
using Display.Providers;
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
using Display.Models.Dto.OneOneFive;
using Display.Providers.Downloader;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.More
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GetThumbnail : Page
    {
        private List<ThumbnailInfo> _storeThumbnailInfo;
        private readonly ObservableCollection<ThumbnailInfo> thumbnailInfo = [];

        private readonly CancellationTokenSource _cts = new();

        private readonly ObservableCollection<string> _failList = [];

        public GetThumbnail()
        {
            InitializeComponent();
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

            UpdateGridViewShow();

            switch (OriginMethodSelected_ComboBox.SelectedIndex)
            {
                //搜刮源网站
                case 0:
                    StartGetThumbnailFromUrl();
                    break;
                //在线视频
                case 1:
                    //检查在线模型文件是否存在
                    if (GetImageByOpenCv.IsModelFilesExists)
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
            var progress = new Progress<ProgressClass>(info =>
            {
                if (!progress_TextBlock.IsLoaded)
                {
                    _cts.Cancel();
                    return;
                }

                progress_TextBlock.Text = info.Text;

                if (info.Index == -1)
                {
                    return;
                }

                var item = thumbnailInfo[info.Index];

                item.Status = info.Status;

                if (item.Status == Status.Error)
                {
                    _failList.Add(item.name);
                }

                if (!string.IsNullOrEmpty(info.ImagePath))
                {
                    item.PhotoPath = info.ImagePath;
                }

                //完成
                if (item.Status != Status.Doing && thumbnailInfo.Count == info.Index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{DateHelper.ConvertDoubleToLengthStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
                }
            });

            Task.Run(() => GetThumbnailFromUrl(thumbnailInfo.ToList(), progress, _cts));
        }

        private void StartGetThumbnailFromWebVideo()
        {
            var startTime = DateTimeOffset.Now;

            // 整体进度
            var overallProgress = new Progress<ProgressClass>(info =>
            {
                if (!IsLoaded)
                {
                    _cts.Cancel();
                    return;
                }

                progress_TextBlock.Text = info.Text;

                if (info.Index == -1)
                {
                    return;
                }

                var item = thumbnailInfo[info.Index];

                item.Status = info.Status;

                if (item.Status == Status.Error)
                {
                    _failList.Add(item.name);
                }

                if (!string.IsNullOrEmpty(info.ImagePath))
                {
                    item.PhotoPath = info.ImagePath;
                }

                //完成
                if (item.Status != Status.Doing && thumbnailInfo.Count == info.Index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{DateHelper.ConvertDoubleToLengthStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
                }

            });

            // 截图进度
            var getImageProgress = new Progress<GetImageByOpenCv.ProgressInfo>();

            var getThumbnailFromWebVideoSetting = new GetThumbnailFromWebVideoSetting()
            {
                IsJumpVrVideo = IsJumpVrVideo_ToggleSwitch.IsOn,
                IsDetectFaces = IsSelectedFaceImage_ToggleSwitch.IsOn,
                ImageCount = (int)ScreenshotsNumber_NumberBox.Value,
                IsShowWindows = IsShowWindows_ToggleSwitch.IsOn
            };


            Task.Run(() => GetThumbnailFromWebVideo(thumbnailInfo.ToList(), getThumbnailFromWebVideoSetting, overallProgress, getImageProgress, _cts));
        }

        private record GetThumbnailFromWebVideoSetting
        {
            public bool IsJumpVrVideo { get; init; } = true;
            public bool IsDetectFaces;
            public bool IsShowWindows;
            public int ImageCount = 10;
        }

        private async void GetThumbnailFromWebVideo(IReadOnlyList<ThumbnailInfo> thumbnailInfos, GetThumbnailFromWebVideoSetting getThumbnailFromWebVideoSetting, IProgress<ProgressClass> overallProgress, IProgress<GetImageByOpenCv.ProgressInfo> getImageProgress, CancellationTokenSource cts)
        {
            var webApi = WebApi.GlobalWebApi;
            GetImageByOpenCv openCv = new(true);

            for (var i = 0; i < thumbnailInfos.Count; i++)
            {
                ProgressClass progressInfo = new()
                {
                    Index = i,
                    Status = Status.Doing
                };

                overallProgress.Report(progressInfo);

                var name = thumbnailInfos[i].name;

                //最大获取数量
                var imageCount = getThumbnailFromWebVideoSetting.ImageCount;

                var startText = $"【{i + 1}/{thumbnailInfos.Count}】 {name}";

                //保存路径
                var savePath = Path.Combine(AppSettings.ImageSavePath, name);

                //缩略图已存在，跳过
                var existsPath = Path.Combine(savePath, $"Thumbnail_{imageCount - 1}.jpg");
                if (File.Exists(existsPath))
                {
                    progressInfo.Text = $"{startText} - 缩略图已存在，跳过该项目";
                    progressInfo.ImagePath = existsPath;
                    progressInfo.Status = Status.BeforeStart;
                    overallProgress.Report(progressInfo);
                    continue;
                }

                //跳过VR
                if (getThumbnailFromWebVideoSetting.IsJumpVrVideo && thumbnailInfos[i].IsVr)
                {
                    progressInfo.Text = $"{startText} - 跳过VR";
                    progressInfo.Status = Status.BeforeStart;
                    overallProgress.Report(progressInfo);
                    continue;
                }

                //考虑多集视频
                var videoInfoList = DataAccess.Get.GetSingleFileInfoByTrueName(name);

                //检查视频是否已转码
                var videoFileListAfterDecode = videoInfoList.Where(x => x.Vdi != 0).ToList();

                progressInfo.Text = $"{startText} - 获取到视频（总数：{videoInfoList.Count}，转码完成数：{videoFileListAfterDecode.Count}）";
                overallProgress.Report(progressInfo);

                //检查能否播放，获取总时长、总帧数
                var videoToThumbnail = new VideoToThumbnail
                {
                    name = name
                };

                foreach (var videoInfo in videoFileListAfterDecode)
                {
                    var m3U8InfoList = await webApi.GetM3U8InfoByPickCode(videoInfo.PickCode);
                    if (m3U8InfoList.Count > 0)
                    {

                        //添加到pickCodeList中
                        videoToThumbnail.pickCodeList.Add(videoInfo.PickCode);
                    }
                }

                progressInfo.Text = $"{startText} - 获取到视频（总数：{videoInfoList.Count}，转码完成数：{videoFileListAfterDecode.Count}，可播放数：{videoToThumbnail.pickCodeList.Count}）";
                overallProgress.Report(progressInfo);

                foreach (var pickCode in videoToThumbnail.pickCodeList)
                {
                    //重新获取（时间过长m3u8地址可能失效）
                    var m3U8InfoList = await webApi.GetM3U8InfoByPickCode(pickCode);

                    if (m3U8InfoList.Count == 0) continue;

                    //缩略图画质不需要太好，选择最后一个
                    var meu8Info = await webApi.GetM3U8Content(m3U8InfoList[^1]);

                    //总时长
                    var playLong = meu8Info.TotalSecond;

                    var startJumpSecond = 180;
                    var endJumpSecond = 10;

                    var averageLength = (playLong - (startJumpSecond + endJumpSecond)) / imageCount;

                    double currentTime = startJumpSecond;

                    var isShowWindow = getThumbnailFromWebVideoSetting.IsShowWindows;

                    var count = 0;

                    for (var j = 0; j < meu8Info.TsInfoList.Count && count < imageCount; j++)
                    {
                        var item = meu8Info.TsInfoList[j];

                        currentTime += item.Second;

                        if (!(currentTime > averageLength * count)) continue;

                        var imagePath = Path.Combine(savePath, $"Thumbnail_{count}.jpg");


                        if (!File.Exists(imagePath))
                        {

                            var isGetImage = false;
                            for (var tryCount = 0; tryCount < 20 && !isGetImage; tryCount++)
                            {
                                if (j + tryCount >= meu8Info.TsInfoList.Count)
                                {
                                    break;
                                }

                                var tsUrl = $"{meu8Info.BaseUrl}{meu8Info.TsInfoList[j + tryCount].Url}";

                                isGetImage = openCv.Task_GetThumbnailByVideoPath(tsUrl, 0, isShowWindow, getImageProgress, savePath, $"Thumbnail_{count}", getThumbnailFromWebVideoSetting.IsDetectFaces);

                                //Debug.WriteLine("获取链接中的图片");
                                Debug.WriteLine($"【{tryCount + 1}/20】url:{tsUrl}\ncurrentTime:{currentTime}");
                            }

                            //多次尝试后仍未获取人脸信息，就不检测人脸了，随便截一张
                            if (!isGetImage)
                            {
                                openCv.Task_GetThumbnailByVideoPath($"{meu8Info.BaseUrl}{item.Url}", 0, isShowWindow, getImageProgress, savePath, $"Thumbnail_{count}", false);
                            }
                        }

                        progressInfo.ImagePath = imagePath;
                        overallProgress.Report(progressInfo);

                        count++;
                    }
                }

                progressInfo.Status = Status.BeforeStart;
                overallProgress.Report(progressInfo);

            }
        }

        private async void GetThumbnailFromUrl(IReadOnlyList<ThumbnailInfo> thumbnailInfos, IProgress<ProgressClass> progress, CancellationTokenSource cts)
        {
            for (var i = 0; i < thumbnailInfos.Count; i++)
            {
                ProgressClass progressInfo = new()
                {
                    Index = i,
                    Status = Status.Doing
                };

                var thumbnail = thumbnailInfos[i];
                var downUrlList = thumbnail.ThumbnailDownUrlList;

                progressInfo.Text = $"【{i + 1}/{thumbnailInfos.Count}】{thumbnail.name}  （{downUrlList.Count}张）";
                progress.Report(progressInfo);

                var imagePath = string.Empty;
                for (var j = 0; j < downUrlList.Count; j++)
                {
                    var savePath = Path.Combine(AppSettings.ImageSavePath, thumbnail.name);
                    var saveName = $"Thumbnail_{j}";
                    var saveImagePath = await GetInfoFromNetwork.DownloadFile(downUrlList[j], savePath, saveName);

                    if (j == 1) imagePath = saveImagePath;

                }

                if (string.IsNullOrEmpty(imagePath))
                {
                    progressInfo.Status = Status.Error;
                }
                else
                {
                    progressInfo.ImagePath = imagePath;
                    progressInfo.Status = Status.BeforeStart;
                }

                progress.Report(progressInfo);
            }
        }

        /// <summary>
        /// 更新GridView显示:
        /// 只保留选中项
        /// 退出选择模式
        /// </summary>
        private void UpdateGridViewShow()
        {
            List<ThumbnailInfo> thumbnailList = [];
            thumbnailList.AddRange(BasicGridView.SelectedItems.Cast<ThumbnailInfo>());

            thumbnailInfo.Clear();
            thumbnailList.ForEach(thumbnailInfo.Add);

            BasicGridView.SelectionMode = ListViewSelectionMode.None;

            selectedCheckBox.Visibility = Visibility.Collapsed;

            StartButton.Visibility = Visibility.Collapsed;
        }

        private class ProgressClass
        {
            public int Index { get; init; } = -1;
            public string ImagePath { get; set; }
            public string Text { get; set; }
            public Status Status { get; set; }
        }

        private void ShowVideoPart_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _storeThumbnailInfo ??= [.. thumbnailInfo];

            foreach (var item in _storeThumbnailInfo
                         .Where(item => item.ThumbnailDownUrlList.Count > 0))
            {
                thumbnailInfo.Remove(item);
            }
        }

        private void ShowVideoPart_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_storeThumbnailInfo is not { Count: > 0 }) return;

            thumbnailInfo.Clear();
            foreach (var item in _storeThumbnailInfo)
            {
                thumbnailInfo.Add(item);
            }
        }

        private void OriginMethodSelected_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;

            var goToState = VisualStateManager.GoToState(this,
                comboBox.SelectedIndex switch
                {
                    0 => "SelectedUrl",
                    1 => "SelectedWebVideo",
                    _ => "SelectedUrl"
                }
                , true);
        }

        private void OpenModelSavePath_HyperLinkClick(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            var path = Path.Combine(Package.Current.InstalledLocation.Path, @"Assets\Models\caffe");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FileMatch.LaunchFolder(path);
        }
    }

}
