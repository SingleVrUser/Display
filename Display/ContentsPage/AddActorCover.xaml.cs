
using Display.Models;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Display.Data;
using static System.String;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddActorCover : Page
    {
        IncrementallLoadActorInfoCollection actorinfo;

        ObservableCollection<string> ShowImageList = new();

        ObservableCollection<string> failList = new();

        bool isCheckGit = true;

        CancellationTokenSource s_cts = new();

        public AddActorCover()
        {
            this.InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Grid_Loaded;

            actorinfo = new(new() { { "prifile_path", false } });
            actorinfo.SetFilter(new() { "Name != ''" });
            await actorinfo.LoadData();
            BasicGridView.ItemsSource = actorinfo;

        }

        ActorInfo _storedItem;
        private async void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ConnectedAnimation animation = null;

            if (BasicGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                _storedItem = container.Content as ActorInfo;
                animation = BasicGridView.PrepareConnectedAnimation("forwardAnimation", _storedItem, "connectedElement");

                animation.Completed += Animation_Completed1;
            }

            if (e.ClickedItem is not ActorInfo actorInfo) return;

            var filePath = AppSettings.ActorFileTreeSavePath;

            if (isCheckGit)
            {
                progress_TextBlock.Text = "正在获取GFriend仓库信息……";
                if (!await TryUpdateFileTree(filePath))
                {
                    progress_TextBlock.Text = "获取GFriend仓库信息失败!";
                    return;
                }
                progress_TextBlock.Text = "获取仓库信息成功";

                isCheckGit = false;
            }

            var imageUrlList = GetImageUrlListFromFileTreeAsync(actorInfo.name, filePath: filePath);

            ShowImageList.Clear();

            foreach (var imagePath in imageUrlList)
            {
                ShowImageList.Add(imagePath);
            }

            ShoeActorName.Text = actorInfo?.name;
            SmokeGrid.Visibility = Visibility.Visible;

            animation?.TryStart(destinationElement);
            progress_TextBlock.Text = Empty;
        }

        //防止动画开始时，双击触发退出事件
        private void Animation_Completed1(ConnectedAnimation sender, object args)
        {
            SmokeCancelGrid.Tapped += SmokeCancelGrid_Tapped;
        }


        //JObject json;
        private List<string> GetImageUrlListFromFileTreeAsync(string actorName, int maxCount = -1, string filePath = null)
        {
            if (IsNullOrEmpty(filePath))
            {
                filePath = AppSettings.ActorFileTreeSavePath;
            }

            List<string> result = GetImageUrlFormFileTree(filePath, actorName, maxCount);

            return result;
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

            actorinfo.HasMoreItems = false;

            UpdateGridViewShow();

            var startTime = DateTimeOffset.Now;

            //进度
            var progress = new Progress<ProgressClass>(info =>
            {
                if (!progress_TextBlock.IsLoaded)
                {
                    s_cts.Cancel();
                    return;
                }

                progress_TextBlock.Text = info.Text;

                if (info.Index == -1)
                {
                    return;
                }

                var item = actorinfo[info.Index];

                item.Status = info.Status;

                if (item.Status == Status.Error)
                {
                    failList.Add(item.name);
                }

                if (!IsNullOrEmpty(info.ImagePath))
                {
                    item.prifile_path = info.ImagePath;
                }

                //完成
                if (item.Status != Status.Doing && actorinfo.Count == info.Index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{FileMatch.ConvertDoubleToDateStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
                }
            });

            await Task.Run(() => GetActorCoverByGit(actorinfo.ToList(), progress, s_cts));

            BasicGridView.ItemClick += BasicGridView_ItemClick;

        }

        private async Task GetActorCoverByGit(List<ActorInfo> actorInfos, IProgress<ProgressClass> progress, CancellationTokenSource cts)
        {
            string filePath = AppSettings.ActorFileTreeSavePath;

            if (isCheckGit)
            {
                progress.Report(new ProgressClass() { Text = "正在获取GFriend仓库信息……" });

                if (!await TryUpdateFileTree(filePath))
                {
                    progress.Report(new ProgressClass() { Text = "获取GFriend仓库信息失败!" });
                    return;
                }

                isCheckGit = false;
            }

            for (int i = 0; i < actorInfos.Count; i++)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                ProgressClass progressInfo = new()
                {
                    Index = i,
                    Status = Status.Doing
                };

                var item = actorInfos[i];

                //演员名
                var actorName = item.name;

                if (actorName.Contains("?")) continue;

                string indexText = $"【{i + 1}/{actorinfo.Count}】 ";

                progressInfo.Text = $"{indexText}{actorName}";

                progress.Report(progressInfo);

                var imageSavePath = System.IO.Path.Combine(AppSettings.ActorInfoSavePath, actorName, "face.jpg");

                if (!File.Exists(imageSavePath))
                {
                    string imageUrl = Empty;
                    var imageUrlList = GetImageUrlListFromFileTreeAsync(actorName, 1);
                    if (imageUrlList.Count > 0) imageUrl = imageUrlList[0];

                    if (IsNullOrEmpty(imageUrl))
                    {
                        progressInfo.Status = Status.Error;
                        progress.Report(progressInfo);
                        continue;
                    }

                    var downResult = await DownFile(imageUrl, imageSavePath);

                    if (!downResult)
                    {
                        progressInfo.Status = Status.Error;
                        progress.Report(progressInfo);
                        continue;
                    }
                }

                progressInfo.ImagePath = imageSavePath;
                progressInfo.Status = Status.BeforeStart;
                progress.Report(progressInfo);

                var actorId = DataAccess.GetActorIdByName(actorName);
                if (actorId != -1)
                {
                    //更新数据库
                    DataAccess.UpdateActorInfoPrifilePath(actorId, imageSavePath);
                }
            }

        }

        private static List<string> GetImageUrlFormFileTree(string filePath, string actorName, int maxCount)
        {
            List<string> urlList = new();

            string key = Empty;
            string path = Empty;

            foreach (var line in File.ReadLines(filePath))
            {
                var content = line.Trim();

                if (IsNullOrEmpty(key))
                {
                    var matchKey = Regex.Match(content, "\"(.*)\":");

                    if (matchKey.Success)
                    {
                        key = matchKey.Groups[1].Value;
                    }
                }
                else
                {
                    if (IsNullOrEmpty(path))
                    {
                        var matchPath = Regex.Match(content, "\"(.*)\":");

                        if (matchPath.Success)
                        {
                            path = matchPath.Groups[1].Value;
                        }
                        else
                        {
                            if (content.Contains('}'))
                            {
                                key = Empty;
                            }
                        }
                    }
                    else
                    {
                        var matchName = Regex.Match(content, "\"(.*)\":\"(.*)\"");

                        if (matchName.Success)
                        {
                            string name = matchName.Groups[1].Value;
                            var url = matchName.Groups[2].Value;

                            if (name.Contains($"{actorName}."))
                            {
                                string imageUrl = $"https://raw.githubusercontent.com/gfriends/gfriends/master/{key}/{path}/{url}";
                                if (!urlList.Contains(imageUrl))
                                {
                                    urlList.Add(imageUrl);

                                    if (urlList.Count == maxCount)
                                    {
                                        break;
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (content.Contains('}'))
                            {
                                path = Empty;
                            }
                        }
                    }
                }
            }
            return urlList;
        }

        /// <summary>
        /// 从文件中获取演员地址，占用内存过大，弃用
        /// </summary>
        /// <param Name="json"></param>
        /// <param Name="name"></param>
        /// <param Name="count"></param>
        /// <returns></returns>
        private List<string> GetImageUrlFormFileTreeContent(JObject json, string name, int count)
        {
            List<string> imageUrlList = new();

            int getNum = 0;
            foreach (var item in json)
            {
                if (getNum == count) break;

                var path1 = item.Key;

                if (!item.Value.HasValues) continue;

                var value = (JObject)item.Value;

                foreach (var item2 in value)
                {
                    if (getNum == count) break;

                    var path2 = item2.Key;

                    if (!item2.Value.HasValues) continue;

                    var value2 = (JObject)item2.Value;

                    foreach (var item3 in value2)
                    {
                        var Path3 = item3.Key;

                        if (item3.Value.HasValues) continue;

                        var value3 = item3.Value.ToString();

                        if (Path3.Contains(name))
                        {
                            string imagePath = Path.Combine(path1, path2, value3);
                            string imageUrl = $"https://raw.githubusercontent.com/gfriends/gfriends/master/{imagePath}";
                            imageUrlList.Add(imageUrl);
                            getNum++;
                        }
                    }
                }
            }

            return imageUrlList;
        }

        private async Task<bool> TryUpdateFileTree(string filePath)
        {
            var result = true;
            var isNeedDownFile = false;

            if (!File.Exists(filePath))
            {
                isNeedDownFile = true;
            }
            else
            {
                //本地文件信息
                var fileWriteTime = File.GetLastWriteTime(filePath);

                //仓库信息
                var dateStr = await GetGitUpdateDateStr();

                //获取到最新时间
                if (!IsNullOrEmpty(dateStr))
                {
                    var gitUpdateDate = Convert.ToDateTime(dateStr);

                    if (gitUpdateDate > fileWriteTime)
                    {
                        isNeedDownFile = true;
                    }
                }
            }

            if (isNeedDownFile)
            {
                const string filetreeDownUrl = @"https://raw.githubusercontent.com/gfriends/gfriends/master/Filetree.json";
                result = await DownFile(filetreeDownUrl, filePath, true);
            }

            return result;
        }

        /// <summary>
        /// 获取仓库最新的时间
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetGitUpdateDateStr()
        {
            var client = GetInfoFromNetwork.CommonClient;

            const string gitInfoUrl = @"https://api.github.com/repos/gfriends/gfriends";

            string updateDateStr = Empty;

            HttpResponseMessage resp;
            string strResult;
            try
            {
                resp = await client.GetAsync(gitInfoUrl);
                strResult = await resp.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                return updateDateStr;
            }

            if (resp.IsSuccessStatusCode)
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(strResult);
                updateDateStr = json["updated_at"]?.ToString();
            }

            return updateDateStr;
        }

        HttpClient _client;
        private async Task<bool> DownFile(string url, string filePath, bool isNeedReplace = false)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            _client ??= GetInfoFromNetwork.CommonClient;

            var directoryName = Path.GetDirectoryName(filePath);
            if (!IsNullOrEmpty(directoryName) && !File.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (File.Exists(filePath) && !isNeedReplace) return true;

            try
            {
                var rep = await _client.GetAsync(HttpUtility.UrlPathEncode(url));
                if (rep.IsSuccessStatusCode)
                {
                    //var imageBytes = await rep.Content.ReadAsByteArrayAsync();
                    //await File.WriteAllBytesAsync(filePath, imageBytes);

                    await using var stream = await rep.Content.ReadAsStreamAsync();
                    await using var newStream = File.Create(filePath);
                    await stream.CopyToAsync(newStream);

                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        /// <summary>
        /// 更新GridView显示:
        /// 只保留选中项
        /// 退出选择模式
        /// </summary>
        private void UpdateGridViewShow()
        {
            List<ActorInfo> actorList = new();

            foreach (var item in BasicGridView.SelectedItems)
            {
                actorList.Add(item as ActorInfo);
            }

            actorinfo.Clear();
            foreach (var actor in actorList)
            {
                actorinfo.Add(actor);
            }

            BasicGridView.SelectionMode = ListViewSelectionMode.None;

            selectedCheckBox.Visibility = Visibility.Collapsed;

            StartButton.Visibility = Visibility.Collapsed;

            modifyToggleSwitch.IsEnabled = false;
        }

        /// <summary>
        /// 获取演员头像（通过视频）
        /// </summary>
        private async void GetActorCoverByWebVideo()
        {
            var webApi = WebApi.GlobalWebApi;
            var startTime = DateTimeOffset.Now;

            //获取链接
            foreach (var item in actorinfo)
            {
                item.Status = Status.Doing;

                //演员名
                var actorName = item.name;

                progress_TextBlock.Text = $"{actorName} - 获取演员参演的视频";
                //获取演员参演的视频
                var videoList = DataAccess.loadVideoInfoByActor(actorName);

                //progress_TextBlock.Text = $"{actorName} - 获取演员参演的视频 - 成功";


                progress_TextBlock.Text = $"{actorName} - 挑选单体作品（非VR）";
                var videoInfoList = videoList.Where(x => x.actor == actorName && !x.category.Contains("VR") && !x.series.Contains("VR")).ToList();

                //无单体作品，跳过
                if (videoInfoList.Count == 0)
                {
                    progress_TextBlock.Text = $"{actorName} - 无单体作品 - 跳过";
                    item.Status = Status.Success;
                    continue;
                }

                var videoPlayUrl = Empty;
                Datum videoFileListCanPlay = null;
                for (var i = 0; i < videoInfoList.Count && videoPlayUrl == Empty; i++)
                {
                    var firstVideoInfo = videoInfoList[i];

                    //视频名称
                    var videoName = firstVideoInfo.truename;

                    progress_TextBlock.Text = $"{actorName} - {videoName} - 挑选单体作品 - {videoName}";

                    //获取视频列表
                    var videoFileList = DataAccess.loadFileInfoByTruename(videoName);

                    progress_TextBlock.Text = $"{actorName} - {videoName} - 挑选单体作品中能在线看的";

                    //挑选能在线观看的视频
                    videoFileListCanPlay = videoFileList.FirstOrDefault(x => x.vdi != 0);

                    //全部未转码失败视频，跳过
                    if (videoFileListCanPlay == null)
                    {
                        progress_TextBlock.Text = $"{actorName} - {videoName} - 在线视频转码未完成 - 跳过";
                        continue;
                    }

                    progress_TextBlock.Text = $"{actorName} - {videoName} - 挑选单体作品中能在线看的 - {videoFileListCanPlay.n}";

                    //获取视频的PickCode
                    var pickCode = videoFileListCanPlay.pc;

                    progress_TextBlock.Text = $"{actorName} - {videoName} - {videoFileListCanPlay.n} - 获取视频播放地址";

                    //获取播放地址
                    var m3U8InfoList = await webApi.GetM3U8InfoByPickCode(pickCode);

                    //文件不存在或已删除
                    if (m3U8InfoList.Count == 0)
                    {
                        progress_TextBlock.Text = $"{actorName} - {videoName} - {videoFileListCanPlay.n} - 文件不存在或已删除";
                        continue;
                    }
                    //成功
                    else
                    {
                        videoPlayUrl = m3U8InfoList[0].Url;
                    }
                }

                //无符合条件的视频，跳过
                if (videoPlayUrl == Empty)
                {
                    progress_TextBlock.Text = $"{actorName} - 未找到符合条件的视频";
                    item.Status = Status.Error;
                    continue;
                }

                var savePath = Path.Combine(AppSettings.ActorInfoSavePath, actorName);
                var imageName = "face";

                var imagePath = Path.Combine(savePath, $"{imageName}.jpg");
                if (File.Exists(imagePath))
                {
                    item.prifile_path = imagePath;
                    progress_TextBlock.Text = $"{actorName} - 头像已存在，跳过";

                }
                else
                {
                    progress_TextBlock.Text = $"{actorName} - {videoFileListCanPlay?.n} - 从视频中获取演员头像 - 女（可信度>0.99）";

                    var progressInfo = await GetActorFaceByVideoPlayUrl(actorName, videoPlayUrl, 1, savePath, imageName);

                    if (!IsNullOrEmpty(progressInfo.imagePath))
                    {
                        item.prifile_path = progressInfo.imagePath;

                        //最后一次report的就是目标信息
                        var face = progressInfo.faceList[progressInfo.faceList.Count - 1];
                        item.genderInfo = $"{face.gender.result}（{face.gender.confidence:0.00}）";
                        item.ageInfo = $"{face.age.result}（{face.age.confidence:0.00}）";
                    }
                }

                item.Status = Status.BeforeStart;
            }

            //结束
            progress_TextBlock.Text = $"总耗时：{FileMatch.ConvertDoubleToDateStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
        }

        private static async Task<progressInfo> GetActorFaceByVideoPlayUrl(string actorName, string url, int Task_Count, string SavePath, string imageName, bool isShowWindow = true)
        {
            //var faceImagePath = string.Empty;

            progressInfo progressInfo = new();

            GetImageByOpenCV openCV = new GetImageByOpenCV();

            ////进度条最大值
            //AllProgressBar.Maximum = Task_Count;

            //AllProgressBar.Value = 0;

            //int Task_Count = 20;

            var framesNum = openCV.getTotalFrameCount(url);
            if (framesNum == 0) return progressInfo;

            //跳过开头
            const int startJumpFrameNum = 1000;

            //跳过结尾
            int endJumpFrame_num = 0;

            var actualEndFrame = framesNum - endJumpFrame_num;

            //平均长度
            var averageLength = (int)(actualEndFrame - startJumpFrameNum) / Task_Count;

            //Progress_TextBlock.Text = $"{AllProgressBar.Value}/{Task_Count}";
            //AllProgressBar.Visibility = Visibility.Visible;

            //进度
            var progress = new Progress<progressInfo>(info =>
            {
                //tryStartShowFaceCanv();

                //RichTextBlock richTextBlock = new RichTextBlock();
                //Paragraph paragraph = new Paragraph();

                //var faces = info.faceList;

                //paragraph.Inlines.Add(new Run() { Text = $"捕获人脸({faces.Count})：" });
                //paragraph.Inlines.Add(new LineBreak());
                //for (int i = 0; i < faces.Count; i++)
                //{
                //    string index = string.Empty;
                //    if (faces.Count > 1)
                //    {
                //        index = $"{i + 1}. ";
                //    }

                //    paragraph.Inlines.Add(new Run() { Text = $"{index}{faces[i].gender.result}（{faces[i].gender.confidence:0.00}）：{faces[i].age.result}岁（{faces[i].age.confidence:0.00}）" });
                //    if (i < faces.Count - 1)
                //    {
                //        paragraph.Inlines.Add(new LineBreak());
                //    }
                //}
                progressInfo = info;
                //faceImagePath = info.imagePath;
                //showImage.Source = new BitmapImage(new Uri(info.imagePath));

                //updateFaceCanv(info);

                //if (imageIntroduce_RichTextBlock.Blocks.Count == 0)
                //{
                //    imageIntroduce_RichTextBlock.Blocks.Add(new Paragraph());
                //}

                //imageIntroduce_RichTextBlock.Blocks[0] = paragraph;
            });

            //string SavePath = Path.Combine(AppSettings.ActorInfo_SavePath,actorName);
            //string iamgeName = "face";

            //double taskStartFrame = startJumpFrame_num;
            //double length = actualEndFrame - startJumpFrame_num < 2 * averageLength ? actualEndFrame - startJumpFrame_num : averageLength;

            double length = 100 * 100;

            await Task.Run(() => openCV.Task_GenderByVideo(url, startJumpFrameNum, length, isShowWindow, progress, SavePath, imageName));

            //var startTime = DateTimeOffset.Now;
            //for (double start_frame = startJumpFrame_num; start_frame + averageLength <= actualEndFrame; start_frame += averageLength)
            //{


            //    //Debug.WriteLine($"进程开始：{start_frame}，长度{length}");



            //    //AllProgressBar.Value++;

            //    //Progress_TextBlock.Text = $"{AllProgressBar.Value}/{Task_Count}";

            //    ////计算剩余时间
            //    //leftTime_TextBlock.Text = $"预计剩余：{ConvertDoubleToDateStr((Task_Count - AllProgressBar.Value) * ((DateTimeOffset.Now - startTime) / AllProgressBar.Value).TotalSeconds)}";

            //}


            return progressInfo;
        }

        private async void CancelSmokeGrid()
        {
            if (!destinationElement.IsLoaded) return;

            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardsAnimation", destinationElement);
            SmokeGrid.Children.Remove(destinationElement);

            // Collapse the smoke when the animation completes.
            animation.Completed += Animation_Completed;

            // If the connected item appears outside the viewport, scroll it into view.
            BasicGridView.ScrollIntoView(_storedItem, ScrollIntoViewAlignment.Default);
            BasicGridView.UpdateLayout();

            // Use the Direct configuration to go back (if the API is available). 
            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                animation.Configuration = new DirectConnectedAnimationConfiguration();
            }

            // Play the second connected animation. 
            await BasicGridView.TryStartConnectedAnimationAsync(animation, _storedItem, "connectedElement");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            CancelSmokeGrid();
        }

        private void Animation_Completed(ConnectedAnimation sender, object args)
        {
            SmokeGrid.Visibility = Visibility.Collapsed;
            SmokeGrid.Children.Add(destinationElement);
            SmokeCancelGrid.Tapped -= SmokeCancelGrid_Tapped;
        }

        private static Visibility IsShowFailList(ObservableCollection<string> List)
        {
            return List.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private class ProgressClass
        {
            public int Index { get; set; } = -1;
            public string ImagePath { get; set; }
            public string Text { get; set; }
            public Status Status { get; set; }
        }

        private void modifyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleSwitch toggleSwitch) return;

            //添加模式
            if (toggleSwitch.IsOn)
            {
                StartButton.IsEnabled = true;
                selectedCheckBox.Visibility = Visibility.Visible;
                BasicGridView.SelectionMode = ListViewSelectionMode.Multiple;
                BasicGridView.ItemClick -= BasicGridView_ItemClick;
            }
            //修改模式
            else
            {
                StartButton.IsEnabled = false;
                selectedCheckBox.Visibility = Visibility.Collapsed;
                BasicGridView.SelectionMode = ListViewSelectionMode.None;
                BasicGridView.ItemClick += BasicGridView_ItemClick;
            }
        }

        private void SmokeCancelGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CancelSmokeGrid();
        }

        private async void ModifyActorImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not HyperlinkButton { DataContext: string imageUrl }) return;

            string actorName = ShoeActorName.Text;

            string savePath = Path.Combine(AppSettings.ActorInfoSavePath, actorName);
            await GetInfoFromNetwork.DownloadFile(imageUrl, savePath, "face", true);

            foreach (var item in actorinfo)
            {
                if (item.name == actorName)
                {
                    item.prifile_path = imageUrl;
                    break;
                }
            }
        }

        private void connectedElement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void connectedElement_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
    }



    ///// <summary>
    ///// GridView样式选择
    ///// </summary>
    //public class CoverItemTemplateSelector : DataTemplateSelector
    //{
    //    public DataTemplate HaveFaceImageTemplate { get; set; }
    //    public DataTemplate WithoutFaceImageTemplate { get; set; }

    //    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    //    {
    //        var Actor = (item as ActorsInfo);
    //        if (string.IsNullOrEmpty(Actor.prifilePhotoPath)) return WithoutFaceImageTemplate;

    //        return HaveFaceImageTemplate;
    //    }
    //}
}
