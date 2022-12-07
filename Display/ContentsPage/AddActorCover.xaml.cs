using Data;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddActorCover : Page
    {
        ObservableCollection<ActorsInfo> actorinfo = new();
        Dictionary<string, List<string>> ActorsInfoDict = new();

        ObservableCollection<string> ShowImageList = new();

        ObservableCollection<string> failList = new();

        CancellationTokenSource s_cts = new();

        ////绘制头像框
        //CanvasControl canv = new CanvasControl();

        public AddActorCover()
        {
            this.InitializeComponent();


        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            List<VideoInfo> VideoInfoList = await Task.Run(() => DataAccess.LoadAllVideoInfo(-1));

            foreach (var VideoInfo in VideoInfoList)
            {
                string actor_str = VideoInfo.actor;

                var actor_list = actor_str.Split(",");
                foreach (var actor in actor_list)
                {
                    //名字为空
                    if (actor == String.Empty) continue;

                    //当前名称不存在
                    if (!ActorsInfoDict.ContainsKey(actor))
                    {
                        ActorsInfoDict.Add(actor, new List<string>());
                    }
                    ActorsInfoDict[actor].Add(VideoInfo.truename);
                }

            }

            //先排序
            List< ActorsInfo> tmpInfos = new();
            foreach (var item in ActorsInfoDict)
            {
                tmpInfos.Add(new ActorsInfo
                {
                    name = item.Key,
                    count = item.Value.Count,
                });
            }
            tmpInfos = tmpInfos.OrderByDescending(x => x.prifilePhotoPath).ToList();

            tmpInfos.ForEach(x => actorinfo.Add(x));

        }

        ActorsInfo _storedItem;
        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ConnectedAnimation animation = null;

            if (BasicGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                _storedItem = container.Content as ActorsInfo;
                animation = BasicGridView.PrepareConnectedAnimation("forwardAnimation", _storedItem, "connectedElement");

            }
            var actorInfo = e.ClickedItem as ActorsInfo;

            var ImageUrlList = GetImageUrlListFromFileTree( actorInfo.name);

            ShowImageList.Clear();

            foreach (var imagePath in ImageUrlList)
            {
                ShowImageList.Add(imagePath);
            }

            ShoeActorName.Text = (e.ClickedItem as ActorsInfo).name;
            SmokeGrid.Visibility = Visibility.Visible;

            animation.Completed += Animation_Completed1;

            animation.TryStart(destinationElement);


        }

        //防止动画开始时，双击触发退出事件
        private void Animation_Completed1(ConnectedAnimation sender, object args)
        {
            SmokeCancelGrid.Tapped += SmokeCancelGrid_Tapped;
        }


        //JObject json;
        private List<string> GetImageUrlListFromFileTree(string actorName,int count = -1)
        {
            //if (json == null)
            //{
            //    string filePath = AppSettings.ActorFileTree_SavePath;

            //    var lines = await File.ReadAllLinesAsync(filePath);
            //    string textContent = string.Join(Environment.NewLine, lines);
            //    json = JsonConvert.DeserializeObject<JObject>(textContent);
            //}
            //List<string> result = getImageUrlFormFileTreeContent(json, actorName, count);

            List<string> result = getImageUrlFormFileTree(AppSettings.ActorFileTree_SavePath, actorName, count);


            return result;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BasicGridView.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BasicGridView.SelectedItems.Clear();
            //BasicGridView.DeselectRange(new ItemIndexRange(0, (uint)actorinfo.Count));
        }

        private void BasicGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //int totalItemCount = actorinfo.Count;
            var selectedItemCount = BasicGridView.SelectedItems.Count;

            selectedCheckBox.Content = $"共选 {selectedItemCount} 项";
            //if(selectedItemCount< totalItemCount)
            //{
            //    selectedCheckBox.Content = $"共选 {selectedItemCount} 项";
            //}
            //else if (selectedItemCount == totalItemCount)
            //{
            //    selectedCheckBox.Content = "取消全选";
            //}
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

                if(info.index == -1)
                {
                    return;
                }

                var item = actorinfo[info.index];

                item.Status = info.status;

                if(item.Status == Status.error)
                {
                    failList.Add(item.name);
                }

                if (!string.IsNullOrEmpty(info.imagePath))
                {
                    item.prifilePhotoPath = info.imagePath;
                }

                //完成
                if(item.Status!= Status.doing && actorinfo.Count == info.index + 1)
                {
                    progress_TextBlock.Text = $"任务已完成，耗时{FileMatch.ConvertInt32ToDateStr((DateTimeOffset.Now-startTime).TotalSeconds)}";
                }
            });

            Task.Run(() => getActorCoverByGit(actorinfo.ToList(), progress, s_cts));

            BasicGridView.ItemClick += BasicGridView_ItemClick;

            //getActorCoverByWebVideo();
        }

        private async void getActorCoverByGit(List<ActorsInfo> actorinfos,IProgress<progressClass> progress, CancellationTokenSource s_cts)
        {
            string filePath = AppSettings.ActorFileTree_SavePath;

            progress.Report(new progressClass() { text = "正在获取GFriend仓库信息……"});

            if (!await tryUpdateFileTree(filePath))
            {
                progress.Report(new progressClass() { text = "获取GFriend仓库信息失败!" });
                return;
            }
            //else
            //    progress.Report(new progressClass() { text = "开始下载头像" });

            //IEnumerable<string> line = File.ReadLines(filePath);

            //var textContent = string.Join(Environment.NewLine, line);

            for (int i = 0;i< actorinfos.Count;i++)
            //foreach (var item in actorinfo)
            {
                if (s_cts.IsCancellationRequested)
                {
                    break;
                }

                progressClass progressinfo = new();
                progressinfo.index = i;
                progressinfo.status = Status.doing;

                var item = actorinfos[i];
                //item.Status = Status.doing;

                //演员名
                var actorName = item.name;

                string indexText = $"【{i + 1}/{actorinfo.Count}】 ";

                progressinfo.text = $"{indexText}{actorName}";

                progress.Report(progressinfo);

                var iamgeSavePath = System.IO.Path.Combine(AppSettings.ActorInfo_SavePath, actorName, "face.jpg");

                if (!File.Exists(iamgeSavePath))
                {
                    string ImageUrl = string.Empty;
                    var ImageUrlList = GetImageUrlListFromFileTree(actorName, 1);
                    if (ImageUrlList.Count>0) ImageUrl = ImageUrlList[0];

                    if (string.IsNullOrEmpty(ImageUrl))
                    {
                        progressinfo.status = Status.error;
                        progress.Report(progressinfo);
                        continue;
                    }

                    var downResult = await DownFile(ImageUrl, iamgeSavePath);

                    if (!downResult)
                    {
                        progressinfo.status = Status.error;
                        progress.Report(progressinfo);
                        continue;
                    }
                }

                progressinfo.imagePath = iamgeSavePath;
                progressinfo.status = Status.beforeStart;

                progress.Report(progressinfo);
            }


        }

        private List<string> getImageUrlFormFileTree(string filePath,string actorName, int count)
        {
            List<string> urlList = new();

            string key = String.Empty;
            string path = String.Empty;
            //string name = String.Empty;

            foreach (var line in File.ReadLines(filePath))
            {
                var content = line.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    var matchKey = Regex.Match(content, "\"(.*)\":");

                    if (matchKey.Success)
                    {
                        key = matchKey.Groups[1].Value;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(path))
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
                                key = string.Empty;
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

                            if (url.Contains(actorName))
                            {
                                urlList.Add($"https://raw.githubusercontent.com/gfriends/gfriends/master/{key}/{path}/{url}");

                                if(urlList.Count == count)
                                {
                                    break;
                                }
                            }

                        }
                        else
                        {
                            if (content.Contains('}'))
                            {
                                path = string.Empty;
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
        /// <param name="json"></param>
        /// <param name="name"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<string> getImageUrlFormFileTreeContent(JObject json, string name, int count)
        {
            List<string> imageUrlList = new();

            int getNum = 0;
            foreach (var item in json)
            {
                if (getNum == count) break;

                var Path1 = item.Key;

                if (!item.Value.HasValues) continue;

                var value = (JObject)item.Value;

                foreach (var item2 in value)
                {
                    if (getNum == count) break;

                    var Path2 = item2.Key;

                    if (!item2.Value.HasValues) continue;

                    var value2 = (JObject)item2.Value;

                    foreach (var item3 in value2)
                    {
                        var Path3 = item3.Key;

                        if (item3.Value.HasValues) continue;

                        var value3 = item3.Value.ToString();

                        if (Path3.Contains(name))
                        {
                            string imagePath = Path.Combine(Path1, Path2, value3);
                            string imageUrl = $"https://raw.githubusercontent.com/gfriends/gfriends/master/{imagePath}";
                            imageUrlList.Add(imageUrl);
                            getNum++;
                        }
                    }
                }
            }

            return imageUrlList;
        }

        private async Task<bool> tryUpdateFileTree(string filePath)
        {
            bool result = true;
            bool isNeedDownFile = false;

            if (!File.Exists(filePath))
            {
                isNeedDownFile = true;
            }
            else
            {
                var fileWriteTime = File.GetLastWriteTime(filePath);
                //本地文件信息

                //仓库信息
                var dateStr = await GetGitUpdateDateStr();
                if (string.IsNullOrEmpty(dateStr))
                    return false;

                var gitUpdateDate = Convert.ToDateTime(dateStr);

                if (gitUpdateDate > fileWriteTime)
                {
                    isNeedDownFile = true;
                }
            }

            if (isNeedDownFile)
            {
                string FiletreeDownUrl = @"https://raw.githubusercontent.com/gfriends/gfriends/master/Filetree.json";
                result = await DownFile(FiletreeDownUrl, filePath,true);
            }

            return result;
        }

        private async Task<string> GetGitUpdateDateStr()
        {
            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");

            string UpdateDateStr = string.Empty;

            string gitInfoUrl = @"https://api.github.com/repos/gfriends/gfriends";

            HttpResponseMessage resp;
            string strResult;
            try
            {
                resp = await Client.GetAsync(gitInfoUrl);
                strResult = await resp.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                return UpdateDateStr;
            }

            if (resp.IsSuccessStatusCode)
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(strResult);
                UpdateDateStr = json["updated_at"].ToString();
            }

            return UpdateDateStr;
        }

        HttpClient Client;
        private async Task<bool> DownFile(string downurl,string filePath,bool isNeedReplace = false)
        {
            if(Client == null)
                Client = new HttpClient();
            //var handler = new HttpClientHandler { UseCookies = false };
            //Client = new HttpClient(handler);
            //Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");

            var directoryNaem = Path.GetDirectoryName(filePath);
            if (!File.Exists(directoryNaem))
            {
                Directory.CreateDirectory(directoryNaem);
            }

            if (!File.Exists(filePath) || isNeedReplace)
            {
                try
                {
                    var rep = await Client.GetAsync(HttpUtility.UrlPathEncode(downurl));
                    if (rep.IsSuccessStatusCode)
                    {
                        byte[] imageBytes = await rep.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(filePath, imageBytes);
                    }
                    else
                    {
                        return false;
                    }

                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 更新GridView显示:
        /// 只保留选中项
        /// 退出选择模式
        /// </summary>
        private void updateGridViewShow()
        {
            List<ActorsInfo> actorList = new();

            foreach (var item in BasicGridView.SelectedItems)
            {
                actorList.Add(item as ActorsInfo);
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
        private async void getActorCoverByWebVideo()
        {
            WebApi webApi = new();
            var startTime = DateTimeOffset.Now;

            //获取链接
            foreach (var item in actorinfo)
            {
                item.Status = Status.doing;

                //演员名
                var actorName = item.name;

                progress_TextBlock.Text = $"{actorName} - 获取演员参演的视频";
                //获取演员参演的视频
                var VideoList = DataAccess.loadVideoInfoByActor(actorName);

                //progress_TextBlock.Text = $"{actorName} - 获取演员参演的视频 - 成功";

                
                progress_TextBlock.Text = $"{actorName} - 挑选单体作品（非VR）";
                var VideoInfoList = VideoList.Where(x=>x.actor == actorName && !x.category.Contains("VR") && !x.series.Contains("VR")).ToList();

                //无单体作品，跳过
                if (VideoInfoList.Count == 0)
                {
                    progress_TextBlock.Text = $"{actorName} - 无单体作品 - 跳过";
                    item.Status = Status.success;
                    continue;
                }

                var videoPlayUrl = string.Empty;
                Datum videofileListCanPlay = null;
                for (int i = 0; i < VideoInfoList.Count && videoPlayUrl== string.Empty; i++)
                {
                    var FirstVideoInfo = VideoInfoList[i];

                    //视频名称
                    var videoName = FirstVideoInfo.truename;

                    progress_TextBlock.Text = $"{actorName} - {videoName} - 挑选单体作品 - {videoName}";

                    //获取视频列表
                    var videoFileList = DataAccess.loadVideoInfoByTruename(videoName);

                    progress_TextBlock.Text = $"{actorName} - {videoName} - 挑选单体作品中能在线看的";

                    //挑选能在线观看的视频
                    videofileListCanPlay = videoFileList.Where(x => x.vdi != 0) .FirstOrDefault();

                    //全部未转码失败视频，跳过
                    if (videofileListCanPlay == null)
                    {
                        progress_TextBlock.Text = $"{actorName} - {videoName} - 在线视频转码未完成 - 跳过";
                        continue;
                    }

                    progress_TextBlock.Text = $"{actorName} - {videoName} - 挑选单体作品中能在线看的 - {videofileListCanPlay.n}";

                    //获取视频的PickCode
                    var pickCode = videofileListCanPlay.pc;

                    progress_TextBlock.Text = $"{actorName} - {videoName} - {videofileListCanPlay.n} - 获取视频播放地址";

                    //获取播放地址
                    var m3u8InfoList = await webApi.Getm3u8InfoByPickCode(pickCode);

                    //文件不存在或已删除
                    if (m3u8InfoList.Count == 0)
                    {
                        progress_TextBlock.Text = $"{actorName} - {videoName} - {videofileListCanPlay.n} - 文件不存在或已删除";
                        continue;
                    }
                    //成功
                    else
                    {
                        videoPlayUrl = m3u8InfoList[0].Url;
                    }
                }

                //无符合条件的视频，跳过
                if(videoPlayUrl == string.Empty)
                {
                    progress_TextBlock.Text = $"{actorName} - 未找到符合条件的视频";
                    item.Status = Status.error;
                    continue;
                }

                string SavePath = Path.Combine(AppSettings.ActorInfo_SavePath, actorName);
                string imageName = "face";

                string imagePath = Path.Combine(SavePath, $"{imageName}.jpg");
                if (File.Exists(imagePath))
                {
                    item.prifilePhotoPath = imagePath;
                    progress_TextBlock.Text = $"{actorName} - 头像已存在，跳过";

                }
                else
                {
                    progress_TextBlock.Text = $"{actorName} - {videofileListCanPlay.n} - 从视频中获取演员头像 - 女（可信度>0.99）";

                    var progressInfo = await getActorFaceByVideoPlayUrl(actorName, videoPlayUrl, 1, SavePath, imageName);

                    if(!string.IsNullOrEmpty(progressInfo.imagePath))
                    {
                        item.prifilePhotoPath = progressInfo.imagePath;

                        //最后一次report的就是目标信息
                        var face = progressInfo.faceList[progressInfo.faceList.Count - 1];
                        item.genderInfo = $"{face.gender.result}（{face.gender.confidence:0.00}）";
                        item.ageInfo = $"{face.age.result}（{face.age.confidence:0.00}）";
                    }
                }

                item.Status = Status.beforeStart;
            }

            //结束
            progress_TextBlock.Text = $"总耗时：{FileMatch.ConvertInt32ToDateStr((DateTimeOffset.Now - startTime).TotalSeconds)}";
        }

        private async Task<progressInfo> getActorFaceByVideoPlayUrl(string actorName, string url, int Task_Count,string SavePath,string imageName, bool isShowWindow = true)
        {
            //var faceImagePath = string.Empty;

            progressInfo progressInfo = new();

            GetImageByOpenCV openCV = new GetImageByOpenCV();

            ////进度条最大值
            //AllProgressBar.Maximum = Task_Count;

            //AllProgressBar.Value = 0;

            //int Task_Count = 20;

            var frames_num = openCV.getTotalFrameCount(url);
            if (frames_num == 0) return progressInfo;

            //跳过开头
            int startJumpFrame_num = 1000;

            //跳过结尾
            int endJumpFrame_num = 0;

            var actualEndFrame = frames_num - endJumpFrame_num;

            //平均长度
            var averageLength = (int)(actualEndFrame - startJumpFrame_num) / Task_Count;

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

            await Task.Run(() => openCV.Task_GenderByVideo(url, startJumpFrame_num, length, isShowWindow, progress, SavePath, imageName));

            //var startTime = DateTimeOffset.Now;
            //for (double start_frame = startJumpFrame_num; start_frame + averageLength <= actualEndFrame; start_frame += averageLength)
            //{


            //    //Debug.WriteLine($"进程开始：{start_frame}，长度{length}");



            //    //AllProgressBar.Value++;

            //    //Progress_TextBlock.Text = $"{AllProgressBar.Value}/{Task_Count}";

            //    ////计算剩余时间
            //    //leftTime_TextBlock.Text = $"预计剩余：{ConvertInt32ToDateStr((Task_Count - AllProgressBar.Value) * ((DateTimeOffset.Now - startTime) / AllProgressBar.Value).TotalSeconds)}";

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

        private Visibility isShowFailList(ObservableCollection<string> List)
        {
            if (List.Count > 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private class progressClass
        {
            public int index { get; set; } = -1;
            public string imagePath { get; set; }
            public string text { get; set; }
            public Status status { get; set; }
        }

        private void modifyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleswitch = sender as ToggleSwitch;
            //添加模式
            if (toggleswitch.IsOn)
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

        private void ModifyActorImage_Click(object sender, RoutedEventArgs e)
        {
            var imageUrl = (sender as HyperlinkButton).DataContext as string;
            string actorName = ShoeActorName.Text;
            string savePath = Path.Combine(AppSettings.ActorInfo_SavePath, actorName);

            //string imageFullPath =  await GetInfoFromNetwork.downloadFile(imageUrl, savePath, "face",true);

            foreach(var item in actorinfo)
            {
                if(item.name == actorName)
                {
                    item.prifilePhotoPath = imageUrl;
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
