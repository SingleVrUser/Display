using Display.Helper;
using Display.Models;
using Display.Views;
using Display.WindowView;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Windows.Storage;
using HttpMethod = System.Net.Http.HttpMethod;
using HttpRequestMessage = System.Net.Http.HttpRequestMessage;

namespace Display.Data
{
    public class WebApi
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private HttpClient QRCodeClient;
        public HttpClient Client;
        public static UserInfo UserInfo;
        public static QRCodeInfo QRCodeInfo;
        private static UploadInfo _uploadInfo;
        public static bool isEnterHiddenMode;
        public TokenInfo TokenInfo;

        //string api_version = "2.0.1.7";

        private static WebApi _webApi;

        public static WebApi GlobalWebApi => _webApi ??= new WebApi();

        public WebApi(bool useCookie = true)
        {
            if (useCookie)
            {
                InitializeInternet();
            }
        }

        /// <summary>
        /// 添加user-agent和cookie
        /// </summary>
        public void InitializeInternet()
        {
            Client = GetInfoFromNetwork.CreateClient(new Dictionary<string, string> { { "user-agent", GetInfoFromNetwork.UserAgent } });

            var cookie = AppSettings._115_Cookie;

            //cookie不为空且可用
            if (!string.IsNullOrEmpty(cookie))
            {
                Client.DefaultRequestHeaders.Add("Cookie", cookie);
            }
        }

        /// <summary>
        /// 更新cookie
        /// </summary>
        /// <param Name="cookie"></param>
        public void RefreshCookie(string cookie)
        {
            Client.DefaultRequestHeaders.Remove("Cookie");
            Client.DefaultRequestHeaders.Add("Cookie", cookie);
        }

        private static Windows.Web.Http.HttpClient _windowWebHttpClient;
        public static Windows.Web.Http.HttpClient GetVideoWindowWebHttpClient()
        {
            if (_windowWebHttpClient != null) return _windowWebHttpClient;

            _windowWebHttpClient = CreateWindowWebHttpClient();

            return _windowWebHttpClient;
        }

        public static Windows.Web.Http.HttpClient CreateWindowWebHttpClient()
        {
            var windowWebHttpClient = new Windows.Web.Http.HttpClient();
            windowWebHttpClient.DefaultRequestHeaders.Add("Referer", "https://115.com/?cid=0&offset=0&tab=&mode=wangpan");
            windowWebHttpClient.DefaultRequestHeaders.Add("User-Agent", GetInfoFromNetwork.UserAgent);

            return windowWebHttpClient;
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        /// <returns>true - 登录，false - 未登录</returns>
        public async Task<bool> UpdateLoginInfo()
        {
            bool result = false;
            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync($"https://my.115.com/?ct=ajax&ac=nav&callback=jQuery172046995607070659906_1647774910536&_={DateTimeOffset.Now.ToUnixTimeSeconds()}");
            }
            catch (HttpRequestException e)
            {
                Toast.tryToast("网络异常", "检查115登录状态时出现异常：", e.Message);

                return result;
            }

            if (!response.IsSuccessStatusCode)
            {
                return result;
            }

            string strReuslt = await response.Content.ReadAsStringAsync();
            strReuslt = strReuslt.Replace("jQuery172046995607070659906_1647774910536(", "");
            strReuslt = strReuslt.Replace(");", "");

            try
            {
                UserInfo = JsonConvert.DeserializeObject<UserInfo>(strReuslt);
                result = UserInfo.state;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 检查是否为隐藏模式
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsHiddenModel()
        {
            var result = false;

            var values = new Dictionary<string, string>
                {
                    { "last_file_type", " folder"},
                    {"last_file_id"," 1865386445801900763" }
                };
            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response;
            try
            {
                response = Client.PostAsync("https://115.com/?ac=setting&even=saveedit&is_wl_tpl=1", content).Result;
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "检查115隐藏状态时出现异常：", e.Message);

                return result;
            }

            if (!response.IsSuccessStatusCode)
            {
                return result;
            }

            var strResult = await response.Content.ReadAsStringAsync();
            _115Setting DriveSetting;
            try
            {
                DriveSetting = JsonConvert.DeserializeObject<_115Setting>(strResult);
            }
            catch
            {
                return result;
            }

            if (DriveSetting.data.show == "1")
            {
                result = true;
                isEnterHiddenMode = true;
            }
            else
            {
                isEnterHiddenMode = false;
            }

            return result;
        }



        /// <summary>
        /// 导入CidList获取到的所有信息到数据库
        /// </summary>
        /// <param Name="cidList"></param>
        /// <param Name="getFilesProgressInfo"></param>
        /// <param Name="progress"></param>
        /// <returns></returns>
        public async Task GetAllFileInfoToDataAccess(List<string> cidList, GetFilesProgressInfo getFilesProgressInfo, CancellationToken token, IProgress<GetFileProgessIProgress> progress = null)
        {
            foreach (var cid in cidList)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                //统计发送请求的频率
                int sendCount = 0;
                long nowDate = DateTimeOffset.Now.ToUnixTimeSeconds();

                await Task.Delay(1000, token);

                // 一开始只有cid，先获取cid的属性
                var cidCategory = await GetFolderCategory(cid);
                sendCount++;

                //正常不为空，为空说明有异常
                if (cidCategory == null)
                {
                    progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, status = ProgressStatus.error, sendCountPerMinutes = 1 });

                    // 退出
                    return;
                }

                // 获取上一次已添加文件夹的pid（如果存在，且修改时间不变；不存在的默认值为string.empty）
                var pid = DataAccess.GetLatestFolderPid(cidCategory.pick_code, cidCategory.utime);

                // 该文件已存在数据库里，且修改时间不变
                if (!string.IsNullOrEmpty(pid) && Data.StaticData.isJumpExistsFolder)
                {
                    //如果修改时间未变，但移动了位置
                    if (pid == cidCategory.paths.Last().file_id)
                    {
                        await DataAccess.AddFilesInfoAsync(FolderCategory.ConvertFolderToDatum(cidCategory, cid));
                    }

                    //统计上下级文件夹所含文件的数量
                    //文件数量
                    getFilesProgressInfo.FilesCount += cidCategory.count;
                    //文件夹数量
                    getFilesProgressInfo.FolderCount += cidCategory.folder_count;

                    var cpm = sendCount * 60 / (DateTimeOffset.Now.ToUnixTimeSeconds() - nowDate);

                    progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });

                }
                //之前未添加或修改时间已改变
                else
                {
                    //获取当前文件夹下所有文件信息和文件夹信息（从数据库或者网络）
                    getFilesProgressInfo = await TraverseAllFileInfo(cid, getFilesProgressInfo, token, progress);

                    if (getFilesProgressInfo == null) continue;

                    var addToDataAccessList = getFilesProgressInfo.addToDataAccessList;

                    //删除后重新添加
                    DataAccess.DeleteAllDirectroyAndFiles_InfilesInfoTabel(cid);

                    if (addToDataAccessList.Count > 0)
                    {
                        //需要添加进数据库的Datum
                        foreach (var item in addToDataAccessList)
                        {
                            await DataAccess.AddFilesInfoAsync(item);
                        }
                    }

                    //不添加有错误的目录进数据库（添加数据库时会跳过已经添加过的目录，对于出现错误的目录不添加方便后续重新添加）
                    if (getFilesProgressInfo.FailCid.Count == 0)
                    {
                        await DataAccess.AddFilesInfoAsync(FolderCategory.ConvertFolderToDatum(cidCategory, cid));
                    }
                }
            }

            if (token.IsCancellationRequested)
            {
                progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, status = ProgressStatus.cancel, sendCountPerMinutes = 1 });
            }
            else
            {
                // 完成
                progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, status = ProgressStatus.done, sendCountPerMinutes = 1 });
            }

        }

        /// <summary>
        /// 获取所有文件信息
        /// </summary>
        /// <param Name="cid"></param>
        /// <param Name="webFileInfoList"></param>
        /// <returns></returns>
        public async Task<GetFilesProgressInfo> TraverseAllFileInfo(string cid, GetFilesProgressInfo getFilesProgressInfo, CancellationToken token, IProgress<GetFileProgessIProgress> progress = null)
        {
            //var webFileInfoList = fileProgressInfo.datumList;
            if (token.IsCancellationRequested) return null;

            //统计请求速度
            int sendCount = 0;
            long nowDate = DateTimeOffset.Now.ToUnixTimeSeconds();

            //successCount++;
            await Task.Delay(1000, token);

            //查询下一级文件信息
            var WebFileInfo = await GetFileAsync(cid, LoadAll: true);
            sendCount++;

            if (WebFileInfo.state)
            {
                foreach (var item in WebFileInfo.data)
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    //文件夹
                    if (item.fid == null)
                    {
                        getFilesProgressInfo.FolderCount++;

                        //先添加文件夹后添加文件，方便删除已有文件夹中的文件
                        getFilesProgressInfo.addToDataAccessList.Add(item);

                        //查询数据库是否存在
                        if (!string.IsNullOrEmpty(DataAccess.GetLatestFolderPid(item.pc, item.te)) && Data.StaticData.isJumpExistsFolder)
                        {
                            //统计下级文件夹所含文件的数量
                            //通过数据库获取
                            var datumList = DataAccess.GetAllFilesTraverse(item.cid);

                            getFilesProgressInfo.addToDataAccessList.AddRange(datumList);

                            //文件数量
                            getFilesProgressInfo.FilesCount += datumList.Count;

                            var cpm = sendCount * 60 / (DateTimeOffset.Now.ToUnixTimeSeconds() - nowDate);
                            progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });
                        }
                        else
                        {
                            getFilesProgressInfo = await TraverseAllFileInfo(item.cid, getFilesProgressInfo, token, progress);

                            if (getFilesProgressInfo == null) continue;
                        }
                    }
                    //文件
                    else
                    {
                        getFilesProgressInfo.FilesCount++;

                        var cpm = sendCount * 60 / (DateTimeOffset.Now.ToUnixTimeSeconds() - nowDate);
                        progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });

                        getFilesProgressInfo.addToDataAccessList.Add(item);
                    }
                }
            }
            else
            {
                getFilesProgressInfo.FailCid.Add(cid);
            }

            return getFilesProgressInfo;
        }

        /// <summary>
        /// 修改文件列表的排列顺序
        /// </summary>
        /// <param Name="cid"></param>
        /// <param Name="orderBy"></param>
        /// <param Name="asc"></param>
        /// <returns></returns>
        public async Task ChangedShowType(string cid, OrderBy orderBy = OrderBy.user_ptime, int asc = 0)
        {
            var values = new Dictionary<string, string>
                {
                    { "user_order", orderBy.ToString()},
                    {"file_id", cid },
                    {"user_asc",asc.ToString() },
                    {"fc_mix","0" },
                };
            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response;
            try
            {
                response = await Client.PostAsync("https://webapi.115.com/files/order", content);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "改变115排序顺序时发生异常：", e.Message);
            }
        }

        /// <summary>
        /// 删除115文件
        /// </summary>
        /// <param Name="pid"></param>
        /// <param Name="fids"></param>
        /// <param Name="ignore_warn"></param>
        /// <returns></returns>
        public async Task DeleteFiles(string pid, List<string> fids, int ignore_warn = 1)
        {
            var values = new Dictionary<string, string>
            {
                { "pid", pid},
            };

            for (int i = 0; i < fids.Count; i++)
            {
                values.Add($"fid[{i}]", fids[i]);
            }

            values.Add("ignore_warn", ignore_warn.ToString());

            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response;
            try
            {
                response = await Client.PostAsync("https://webapi.115.com/rb/delete", content);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "删除115文件时发生异常：", e.Message);
            }
        }

        /// <summary>
        /// 从115回收站恢复文件
        /// </summary>
        /// <param Name="rids"></param>
        /// <returns></returns>
        public async Task RevertFiles(List<string> rids)
        {
            Dictionary<string, string> values = new();

            for (int i = 0; i < rids.Count; i++)
            {
                values.Add($"rid[{i}]", rids[i]);
            }

            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response;
            try
            {
                response = await Client.PostAsync("https://webapi.115.com/rb/revert", content);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "从115回收站恢复文件时发生异常：", e.Message);
            }
        }

        public async Task<bool> CreateTorrentOfflineDown(string torrentPath)
        {
            var torrentFile = new FileInfo(torrentPath);
            if (torrentFile.Length > 2097152)
            {
                // TODO: 转换为ed2k后添加
                Debug.WriteLine("不支持添加超2M的种子任务");
                return false;
            }

            // 计算torrent文件的sha1
            var sha1 = HashHelper.ComputeSha1ByPath(torrentPath);

            // 检查网盘中是否有该torrent
            var info = await SearchInfoBySha1(sha1);

            // 获取上传信息
            var uploadInfo = await GetUploadInfo();

            // 无该信息
            if (info == null)
            {
                // 获取种子文件应该存放的目录cid
                var torrentCidInfo = await GetTorrentCid();

                if (torrentCidInfo != null && uploadInfo != null)
                {
                    await Upload115.SingleUpload115.UploadTo115(torrentPath, torrentCidInfo.cid, uploadInfo.user_id.ToString(), uploadInfo.userkey);
                }

                // 再次检查网盘中是否有该torrent
                info = await SearchInfoBySha1(sha1);
            }

            // 上传后仍然为空
            if (info == null || uploadInfo == null)  return false;

            var offlineSpaceInfo = await GetOfflineSpaceInfo(uploadInfo.userkey, uploadInfo.user_id);

            // 获取种子信息
            var torrentInfo = await GetTorrentInfo(info.data.pick_code, info.sha1, info.user_id.ToString(), offlineSpaceInfo.sign,
                offlineSpaceInfo.time.ToString());

            if(torrentInfo == null) return false;

            // 最小大小,10 MB
            long minSize = 10485760;
            List<int> selectedIndexList = new();
            for (var i = 0; i < torrentInfo.torrent_filelist_web.Length; i++)
            {
                var fileInfo = torrentInfo.torrent_filelist_web[i];

                if (fileInfo.size > minSize)
                {
                    selectedIndexList.Add(i);
                }
            }

            // 添加任务
            var taskInfo = await AddTaskBt(torrentInfo.info_hash,string.Join(",",selectedIndexList),AppSettings.SavePath115Cid,uploadInfo.user_id.ToString(),offlineSpaceInfo.sign,offlineSpaceInfo.time.ToString());

            if(taskInfo == null) return false;

            Debug.WriteLine("添加任务成功");

            return true;
        }

        private async Task<AddTaskBtResult> AddTaskBt(string infoHash, string wanted, string cid, string uid, string sign,string time, string savePath="")
        {
            var url =
                "https://115.com/web/lixian/?ct=lixian&ac=add_task_bt";

            var values = new Dictionary<string, string>
            {
                { "info_hash", infoHash},
                {"wanted",wanted },
                {"savepath",savePath },
                {"wp_path_id",cid },
                {"uid",uid },
                {"sign",sign },
                {"time",time }
            };

            var content = new FormUrlEncodedContent(values);

            try
            {
                var response = await Client.PostAsync(url, content);

                var result = await response.Content.ReadFromJsonAsync<AddTaskBtResult>();

                if (result.state)
                {
                    return result;
                }

                Debug.WriteLine("添加torrent任务出错");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115中添加torrent任务时出错:{ex.Message}");
            }

            return null;
        }

        private async Task<TorrentInfoResult> GetTorrentInfo(string pickCode, string sha1, string uid, string sign, string time)
        {
            var url =
                "https://115.com/web/lixian/?ct=lixian&ac=torrent";

            var values = new Dictionary<string, string>
            {
                { "pickcode", pickCode},
                {"sha1",sha1 },
                {"uid",uid },
                {"sign",sign },
                {"time",time },
            };

            var content = new FormUrlEncodedContent(values);

            try
            {
                var response = await Client.PostAsync(url,content);

                var result = await response.Content.ReadFromJsonAsync<TorrentInfoResult>();

                if (string.IsNullOrEmpty(result?.error_msg))
                {
                    return result;
                }

                Debug.WriteLine($"获取torrent文件Info出错:{result?.error_msg}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115中torrent文件Info时出错:{ex.Message}");
            }

            return null;
        }

        private async Task<TorrentCidResult> GetTorrentCid()
        {
            var url =
                "http://115.com/?ct=lixian&ac=get_id&torrent=1";

            try
            {
                var response = await Client.GetAsync(url);

                var result = await response.Content.ReadFromJsonAsync<TorrentCidResult>();

                if (!string.IsNullOrEmpty(result?.cid))
                {
                    return result;
                }

                Debug.WriteLine("获取存放torrent文件的目录cid出错");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115中存放torrent文件的目录cid请求时出错:{ex.Message}");
            }

            return null;
        }

        private async Task<ShaSearchResult> SearchInfoBySha1(string sha1)
        {
            var url =
                $"https://webapi.115.com/files/shasearch?sha1={sha1}&_={DateTimeOffset.Now.ToUnixTimeSeconds()}";

            try
            {
                var response = await Client.GetAsync(url);

                var result = await response.Content.ReadFromJsonAsync<ShaSearchResult>();

                if (string.IsNullOrEmpty(result.error))
                    return result;

                Debug.WriteLine($"通过sha1搜索信息出错:{result.error}");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115通过sha1搜索信息请求时出错:{ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 请求创建文件夹
        /// </summary>
        /// <param name="pid">在当前pid下创建文件夹</param>
        /// <param name="name">文件夹名称</param>
        /// <returns>目标cid,创建失败则为null</returns>
        public async Task<MakeDirRequest> RequestMakeDir(string pid, string name)
        {
            var url = "https://webapi.115.com/files/add";

            var values = new Dictionary<string, string>
            {
                { "pid", pid},
                {"cname",name }
            };

            var content = new FormUrlEncodedContent(values);

            try
            {
                var response = await Client.PostAsync(url, content);

                var res = await response.Content.ReadAsStringAsync();

                var result = await response.Content.ReadFromJsonAsync<MakeDirRequest>();

                if (string.IsNullOrEmpty(result.error))
                    return result;

                Debug.WriteLine($"115创建文件夹出错:{result.error}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115创建文件夹请求时出错:{ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param Name="pid"></param>
        /// <param Name="fids"></param>
        /// <returns></returns>
        public async Task MoveFiles(string pid, List<string> fids)
        {
            if (fids is not { Count: > 0 }) return;

            var values = new Dictionary<string, string>
            {
                { "pid", pid},
                { "move_proid", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_-{new Random().Next(1,99)}_0"},

            };

            for (var i = 0; i < fids.Count; i++)
            {
                values.Add($"fid[{i}]", fids[i]);
            }

            var content = new FormUrlEncodedContent(values);

            try
            {
                await Client.PostAsync("https://webapi.115.com/files/move", content);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "移动115文件时发生异常：", e.Message);
            }
        }

        /// <summary>
        /// 重命名文件
        /// </summary>
        /// <param Name="pid"></param>
        /// <param Name="fids"></param>
        /// <returns></returns>
        public async Task<RenameRequest> RenameFile(string pid, string newName)
        {
            var values = new Dictionary<string, string>
            {
                { $"files_new_name[{pid}]", newName},

            };

            var content = new FormUrlEncodedContent(values);

            RenameRequest result = null;

            try
            {
                var response = await Client.PostAsync("https://webapi.115.com/files/batch_rename", content);

                result = await response.Content.ReadFromJsonAsync<RenameRequest>();

                if (string.IsNullOrEmpty(result.error))
                    return result;

                Debug.WriteLine($"115重命名出错:{result.error}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115重命名请求时出错:{ex.Message}");
            }

            return result;
        }

        public enum OrderBy { file_name, file_size, user_ptime }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param Name="cid"></param>
        /// <param Name="limit"></param>
        /// <param Name="offset"></param>
        /// <param Name="useApi2"></param>
        /// <param Name="LoadAll"></param>
        /// <param Name="orderBy"></param>
        /// <param Name="asc"></param>
        /// <returns></returns>
        public async Task<WebFileInfo> GetFileAsync(string cid, int limit = 40, int offset = 0, bool useApi2 = false, bool LoadAll = false, OrderBy orderBy = OrderBy.user_ptime, int asc = 0,bool isOnlyFolder = false)
        {
            WebFileInfo WebFileInfoResult = new();

            string url;
            url = !useApi2 ? $"https://webapi.115.com/files?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json" :
                //旧接口只有t，没有修改时间（te），创建时间（tp）
                $"https://aps.115.com/natsort/files.php?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json&fc_mix=0&type=&star=&is_share=&suffix=&custom_order=";

            if (isOnlyFolder)
                url += "&nf=1";

            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync(url);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "获取文件信息时出现异常：", e.Message);
                return WebFileInfoResult;
            }

            if (!response.IsSuccessStatusCode)
            {
                return WebFileInfoResult;
            }
            var strResult = await response.Content.ReadAsStringAsync();

            WebFileInfoResult = JsonConvert.DeserializeObject<WebFileInfo>(strResult);

            //te，tp简单用t替换，接口2没有te,tp
            if (useApi2)
            {
                foreach (var item in WebFileInfoResult.data)
                {
                    //item.t 可能是 "1658999027" 也可能是 "2022-07-28 17:03"

                    //"1658999027"
                    if (FileMatch.isNumberic1(item.t))
                    {
                        int dateInt = Int32.Parse(item.t);
                        item.te = item.tp = dateInt;
                        item.t = FileMatch.ConvertInt32ToDateTime(dateInt);
                    }
                    //"2022-07-28 17:03"
                    else
                    {

                    }
                }
            }

            if (WebFileInfoResult.data != null)
            {
                foreach (var item in WebFileInfoResult.data)
                {
                    int dateInt;
                    //item.t 可能是 "1658999027" 也可能是 "2022-07-28 17:03"

                    //"1658999027"
                    if (FileMatch.isNumberic1(item.t))
                    {
                        dateInt = Int32.Parse(item.t);
                        item.t = FileMatch.ConvertInt32ToDateTime(dateInt);

                    }
                    //"2022-07-28 17:03"
                    else
                    {
                        dateInt = FileMatch.ConvertDateTimeToInt32(item.t);
                    }

                    if (useApi2)
                    {
                        item.te = item.tp = dateInt;
                    }
                }

            }

            //接口1出错，使用接口2
            if (WebFileInfoResult.errNo == 20130827 && useApi2 == false)
            {
                WebFileInfoResult = await GetFileAsync(cid, limit, offset, true, LoadAll, orderBy, asc,isOnlyFolder: isOnlyFolder);
            }
            //需要加载全部，但未加载全部
            else if (LoadAll && WebFileInfoResult.count > limit)
            {
                WebFileInfoResult = await GetFileAsync(cid, WebFileInfoResult.count, offset, useApi2, LoadAll, orderBy, asc);
            }

            return WebFileInfoResult;
        }

        public async Task<FilesShowInfo> GetFilesShowInfo(string cid)
        {
            FilesShowInfo result = null;

            string url = $"https://webapi.115.com/files?aid=1&cid={cid}&o=user_ptime&asc=0&offset=0&show_dir=1&limit=30&code=&scid=&snap=0&natsort=1&star=1&source=&format=json";
            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync(url);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return result;
            }
            catch (HttpRequestException e)
            {
                Toast.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return result;
            }

            if (response.IsSuccessStatusCode)
            {
                var strResult = await response.Content.ReadAsStringAsync();

                result = JsonConvert.DeserializeObject<FilesShowInfo>(strResult);
            }

            return result;
        }

        /// <summary>
        /// 获取文件夹属性（含大小和数量）
        /// </summary>
        /// <param Name="cid"></param>
        /// <param Name="limit"></param>
        /// <returns></returns>
        public async Task<FolderCategory> GetFolderCategory(string cid)
        {
            FolderCategory webFileInfoResult = null;

            if (cid == "0")
            {
                return new FolderCategory() { file_name = "根目录" };
            }

            var url = $"https://webapi.115.com/category/get?cid={cid}";
            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync(url);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return null;
            }
            catch (HttpRequestException e)
            {
                Toast.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var strResult = await response.Content.ReadAsStringAsync();

                if (strResult == "[]")
                {
                    return null;
                }
                webFileInfoResult = JsonConvert.DeserializeObject<FolderCategory>(strResult);
            }

            return webFileInfoResult;
        }

        /// <summary>
        /// 检查二维码登录验证状态，若登录成功则存储cookie;
        /// </summary>
        /// <returns></returns>
        public async Task<TokenInfo> NetworkVerifyTokenAsync()
        {
            try
            {
                var values = new Dictionary<string, string>
                {
                    { "account", QRCodeInfo.data.uid }
                };
                var content = new FormUrlEncodedContent(values);

                if (QRCodeClient == null)
                {
                    QRCodeClient = new HttpClient();
                }

                var response = QRCodeClient.PostAsync("https://passportapi.115.com/app/1.0/web/1.0/login/qrcode", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string strResult = await response.Content.ReadAsStringAsync();
                TokenInfo = JsonConvert.DeserializeObject<TokenInfo>(strResult);

                if (TokenInfo.state == 1)
                {
                    //存储cookie至本地
                    List<string> CookieList = new List<string>();
                    foreach (var item in TokenInfo.data.cookie.GetType().GetProperties())
                    {
                        CookieList.Add($"{item.Name}={item.GetValue(TokenInfo.data.cookie)}");
                    }

                    var cookie = string.Join(";", CookieList);
                    localSettings.Values["cookie"] = cookie;

                    Client.DefaultRequestHeaders.Add("Cookie", cookie);

                }

            }
            catch (Exception)
            {
                //TokenInfo = null;
            }

            return TokenInfo;
        }

        /// <summary>
        /// 检查二维码扫描状态
        /// </summary>
        /// <returns></returns>
        public async Task<QRCodeStatus> GetQRCodeStatusAsync()
        {
            QRCodeStatus qRCodeStatus = new QRCodeStatus();

            if (QRCodeClient == null)
            {
                QRCodeClient = new HttpClient();
            }

            string url = $"https://qrcodeapi.115.com/get/status/?uid={QRCodeInfo.data.uid}&time={QRCodeInfo.data.time}&sign={QRCodeInfo.data.sign}&_={DateTimeOffset.Now.ToUnixTimeSeconds()}";

            try
            {
                var response = await QRCodeClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return qRCodeStatus;
                }

                var strResult = await response.Content.ReadAsStringAsync();
                qRCodeStatus = JsonConvert.DeserializeObject<QRCodeStatus>(strResult);
            }
            catch (Exception)
            {
                //TokenInfo = null;
            }

            return qRCodeStatus;
        }

        /// <summary>
        /// 获取登录二维码信息
        /// </summary>
        /// <returns></returns>
        public async Task<QRCodeInfo> GetQRCodeInfo()
        {
            QRCodeInfo = new QRCodeInfo();

            if (QRCodeClient == null)
            {
                QRCodeClient = new HttpClient();
            }
            var response = await Client.GetAsync("https://qrcodeapi.115.com/api/1.0/web/1.0/token");
            if (!response.IsSuccessStatusCode)
            {
                return QRCodeInfo;
            }


            var result = await response.Content.ReadAsStringAsync();
            QRCodeInfo = JsonConvert.DeserializeObject<QRCodeInfo>(result);
            return QRCodeInfo;
        }

        public enum downType { _115, bc, aria2 };
        public async Task<bool> RequestDown(List<Datum> videoInfoList, downType downType = downType._115, string savePath = null, string topFolderName = null)
        {
            bool success = false;

            //115只支持文件
            if (downType == downType._115)
                success = await RequestDownBy115Browser(videoInfoList);
            //BitComet支持文件和文件夹
            else if (downType == downType.bc)
            {
                success = await RequestDownByBitComet(videoInfoList, GetInfoFromNetwork.UserAgent, save_path: savePath, topFolderName: topFolderName);
            }
            //Arai2也支持文件和文件夹
            else if (downType == downType.aria2)
            {
                success = await RequestDownByAria2(videoInfoList, GetInfoFromNetwork.UserAgent, save_path: savePath, topFolderName: topFolderName);
            }

            return success;
        }

        /// <summary>
        /// 请求115浏览器下载
        /// </summary>
        /// <param Name="videoInfoList"></param>
        async Task<bool> RequestDownBy115Browser(List<Datum> videoInfoList)
        {

            bool isSuccess = false;

            var downRequest = new Browser_115_Request
            {
                //UID
                uid = videoInfoList[0].uid
            };

            //KEY
            if (QRCodeInfo == null)
            {
                await GetQRCodeInfo();
            }
            downRequest.key = QRCodeInfo?.data.uid;

            //PARAM
            downRequest.param = new Param_Request();
            downRequest.param.list = new();
            foreach (var videoInfo in videoInfoList)
            {
                bool isdir = videoInfo.uid == 0 ? true : false;
                downRequest.param.list.Add(new Down_Request() { n = videoInfo.n, pc = videoInfo.pc, is_dir = isdir });
            }
            downRequest.param.count = downRequest.param.list.Count;
            downRequest.param.ref_url = $"https://115.com/?cid={videoInfoList[0].cid}&offset=0&mode=wangpan";

            string url = "";
            string jsonString = JsonConvert.SerializeObject(downRequest);

            JObject jObject = JObject.Parse(jsonString);
            IEnumerable<string> nameValues = jObject
                .Properties()
                .Select(x => $"{x.Name}={WebUtility.UrlEncode(x.Value.ToString().Replace(System.Environment.NewLine, string.Empty).Replace(" ", ""))}");

            url += "browser115://download?" + string.Join("&", nameValues);

            // The URI to launch
            var uriDown = new Uri(url);

            // Set the option to show a warning
            var options = new Windows.System.LauncherOptions
            {
                DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseLess
            };

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriDown, options);

            if (success)
            {
                isSuccess = true;
                // URI launched
            }
            else
            {
                // URI launch failed
            }

            return isSuccess;
        }

        /// <summary>
        /// 请求比特彗星下载
        /// </summary>
        /// <param Name="videoInfoList"></param>
        /// <returns></returns>
        async Task<bool> RequestDownByBitComet(List<Datum> videoInfoList, string ua, string save_path, string topFolderName = null)
        {
            bool success = true;

            var BitCometSettings = AppSettings.BitCometSettings;

            if (BitCometSettings == null)
                return false;


            string baseUrl = BitCometSettings.ApiUrl;

            var handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(BitCometSettings.UserName, BitCometSettings.Password),
            };

            var client = new HttpClient(handler);

            //存储路径
            if (save_path == null)
                save_path = AppSettings.BitCometSavePath;

            //应用设置中没有，则从比特彗星的设置中读取
            if (string.IsNullOrEmpty(save_path))
                save_path = await getBitCometDefaultSavePath(client, baseUrl);

            if (topFolderName != null)
                save_path = Path.Combine(save_path, topFolderName);

            foreach (Datum datum in videoInfoList)
            {
                string pc = datum.pc;

                //文件夹
                if (string.IsNullOrEmpty(datum.fid) || (!string.IsNullOrEmpty(datum.fid) && datum.fid == "0"))
                {
                    string newSavePath = Path.Combine(save_path, datum.n);
                    //遍历文件夹并下载
                    GetAllFilesTraverseAndDownByBitComet(client, baseUrl, datum, ua, newSavePath);
                }
                //文件
                else
                {
                    bool isOk = await pushOneFileDownRequestToBitComet(client, baseUrl, datum, ua, save_path);

                    if (!isOk)
                        success = false;
                }
            }

            return success;
        }


        /// <summary>
        /// 获取云下载保存路径
        /// </summary>
        /// <param Name="cid"></param>
        /// <param Name="limit"></param>
        /// <returns></returns>
        public async Task<OfflineDownPathRequest> GetOfflineDownPath()
        {
            const string url = "https://webapi.115.com/offine/downpath";
            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync(url);
            }
            catch (AggregateException e)
            {
                Toast.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return null;
            }
            catch (HttpRequestException e)
            {
                Toast.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return null;
            }

            if (!response.IsSuccessStatusCode) return null;

            OfflineDownPathRequest result = null;

            try
            {
                var strResult = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<OfflineDownPathRequest>(strResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115云下载保存路径请求时出错:{ex.Message}");
            }

            return result;
        }

        public async Task<UploadInfo> GetUploadInfo(bool isUpdate=false)
        {
            if (!isUpdate && _uploadInfo != null) return _uploadInfo;

            const string url = "https://proapi.115.com/app/uploadinfo";

            var response = await Client.GetAsync(url);

            try
            {
                var result = await response.Content.ReadFromJsonAsync<UploadInfo>();

                if (string.IsNullOrEmpty(result.error))
                {
                    _uploadInfo = result;
                }
                    
                else
                    Debug.WriteLine($"请求115的UploadInfo时出错：{result.error}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析115返回的UploadInfo请求时出错：{ex.Message}");
            }

            return _uploadInfo;
        }

        public async Task<OfflineSpaceInfo> GetOfflineSpaceInfo(string userKey, int userId)
        {
            var tm = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var url = $"https://115.com/?ct=offline&ac=space&sign={userKey}&time={tm / 1000}&user_id={userId}&_={tm}";

            var response = await Client.GetAsync(url);

            try
            {
                var result = await response.Content.ReadFromJsonAsync<OfflineSpaceInfo>();

                if (result.state)
                    return result;

                Debug.WriteLine("解析115离线任务请求时出错");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"查看115离线任务时出错：{ex.Message}");
            }

            return null;
        }


        public async Task<AddTaskUrlInfo> AddTaskUrl(List<string> linkList, long wpPathId,int uid, string sign, long time)
        {
            if (linkList.Count == 0) return null;

            string url;

            var content = new MultipartFormDataContent
            {
                { new StringContent(""), "savepath" },
                { new StringContent(wpPathId.ToString()), "wp_path_id" },
                { new StringContent(uid.ToString()), "uid" },
                { new StringContent(sign), "sign"},
                { new StringContent(time.ToString()), "time"}
            };

            if (linkList.Count == 1)
            {
                url = "https://115.com/web/lixian/?ct=lixian&ac=add_task_url";
                content.Add(new StringContent(linkList[0]), "url");
            }
            else
            {
                url = "https://115.com/web/lixian/?ct=lixian&ac=add_task_urls";

                for (var i = 0; i < linkList.Count; i++)
                {
                    content.Add(new StringContent(linkList[i]), $"url[{i}]");
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Content = content;

            var response = await Client.SendAsync(request);

            try
            {
                var result = await response.Content.ReadFromJsonAsync<AddTaskUrlInfo>();

                return result;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发送创建115离线任务时出错：{ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 请求Aria2下载
        /// </summary>
        /// <param Name="videoInfoList"></param>
        /// <returns></returns>
        async Task<bool> RequestDownByAria2(List<Datum> videoInfoList, string ua, string save_path, string topFolderName = null)
        {
            var Aria2Settings = AppSettings.Aria2Settings;

            if (Aria2Settings == null)
                return false;

            //存储路径
            if (string.IsNullOrEmpty(save_path))
            {
                save_path = AppSettings.BitCometSavePath;
            }

            //应用设置中没有，则从Aria2的设置中读取
            if (string.IsNullOrEmpty(save_path))
                save_path = await getAria2DefaultSavePath(Aria2Settings.ApiUrl, Aria2Settings.Password, ua);

            if (topFolderName != null)
                save_path = Path.Combine(save_path, topFolderName);

            bool success = await GetAllFilesTraverseAndDownByAria2(videoInfoList, Aria2Settings.ApiUrl, Aria2Settings.Password, save_path, ua);

            return success;
        }

        public async Task<bool> GetAllFilesTraverseAndDownByAria2(List<Datum> videoInfoList, string apiUrl, string password, string save_path, string ua)
        {
            bool success = true;

            Dictionary<string, string> fileList = new();

            foreach (Datum datum in videoInfoList)
            {


                //文件夹
                if (string.IsNullOrEmpty(datum.fid) || (!string.IsNullOrEmpty(datum.fid) && datum.fid == "0"))
                {
                    //获取该文件夹下的文件和文件夹
                    WebFileInfo webFileInfo = await GetFileAsync(datum.cid, LoadAll: true);
                    if (webFileInfo.count == 0)
                    {
                        success = false;
                        continue;
                    }

                    string newSavePath = Path.Combine(save_path, datum.n);
                    bool isOK = await GetAllFilesTraverseAndDownByAria2(webFileInfo.data.ToList(), apiUrl, password, newSavePath, ua);

                    if (!isOK)
                        success = false;
                }

                //文件
                else
                {
                    //一般只有一个
                    var downUrlList = await GetDownUrl(datum.pc, ua);
                    if (downUrlList.Count == 0)
                    {
                        success = false;
                        continue;
                    }

                    ////用来标记aria2的任务id，如果没有就用时间戳代替
                    //string aria2TaskId = datum.pc != null? datum.pc : DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

                    fileList.Add(datum.pc, downUrlList.FirstOrDefault().Value);
                }
            }

            //文件
            if (fileList.Count > 0)
            {
                foreach (var file in fileList)
                {

                    bool isOK = await pushDownRequestToAria2(apiUrl, password, new List<string>() { file.Value }, ua, save_path, file.Key);

                    if (!isOK)
                        success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// 链接只能一个一个添加，添加多个视为为同一文件的不同源
        /// </summary>
        /// <param Name="apiUrl"></param>
        /// <param Name="password"></param>
        /// <param Name="urls"></param>
        /// <param Name="ua"></param>
        /// <param Name="save_path"></param>
        /// <returns></returns>
        async Task<bool> pushDownRequestToAria2(string apiUrl, string password, List<string> urls, string ua, string save_path, string sha1 = null)
        {
            bool success = false;

            save_path = save_path.Replace("\\", "/");

            string gid = sha1 != null ? sha1 : DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            string myContent = "{\"jsonrpc\":\"2.0\"," +
                "\"method\": \"aria2.addUri\"," +
                "\"id\": \"" + gid + "\"," +
                "\"params\": [ \"" + password + "\"," +
                            "[\"" + string.Join("\",\"", urls) + "\"]," +
                            "{\"referer\": \"https://115.com/?cid=0&offset=0&tab=&mode=wangpan\"," +
                            "\"header\": [\"User-Agent: " + ua + "\"]," +
                            "\"dir\": \"" + save_path + "\"}]}";

            HttpClient client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            var Content = new StringContent(myContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage rep = await client.PostAsync(apiUrl, Content);

                if (rep.IsSuccessStatusCode)
                {
                    string strResult = await rep.Content.ReadAsStringAsync();
                    Console.WriteLine(strResult);

                    if (!string.IsNullOrWhiteSpace(strResult))
                        success = true;
                }
            }
            catch
            {
                success = false;
            }

            return success;
        }

        async Task<bool> pushOneFileDownRequestToBitComet(HttpClient client, string baseUrl, Datum datum, string ua, string save_path)
        {
            bool success = false;

            var urlList = await GetDownUrl(datum.pc, ua);

            if (urlList.Count == 0)
                return false;

            bool isOk = await pushDownRequestToBitComet(client,
                baseUrl,
                urlList.First().Value,
                save_path,
                datum.n,
                $"https://115.com/?cid={datum.cid}=0&tab=download&mode=wangpan",
                ua);

            if (isOk)
                success = true;

            return success;
        }

        public async void GetAllFilesTraverseAndDownByBitComet(HttpClient client, string baseUrl, Datum datum, string ua, string save_path)
        {
            //获取该文件夹下的文件和文件夹
            WebFileInfo webFileInfo = await GetFileAsync(datum.cid, LoadAll: true);

            foreach (var data in webFileInfo.data)
            {
                //文件夹
                if (string.IsNullOrEmpty(data.fid) || (!string.IsNullOrEmpty(data.fid) && data.fid == "0"))
                {
                    string newSavePath = Path.Combine(save_path, data.n);
                    GetAllFilesTraverseAndDownByBitComet(client, baseUrl, data, ua, newSavePath);

                    //延迟1s;
                    await Task.Delay(1000);
                }
                //文件
                else
                {
                    await pushOneFileDownRequestToBitComet(client, baseUrl, data, ua, save_path);
                }

            }
        }

        /// <summary>
        /// 向比特彗星发送下载请求
        /// </summary>
        /// <param Name="client">带user和passwd的HttpClient</param>
        /// <param Name="baseUrl">比特彗星接口地址</param>
        /// <param Name="downUrl">文件下载地址</param>
        /// <param Name="save_path">文件保存路径</param>
        /// <param Name="filename">文件名臣</param>
        /// <param Name="referrer">下载需要的referrer</param>
        /// <param Name="user_agent">下载需要的user_agent</param>
        /// <param Name="cookie">个别需要的Cookie</param>
        /// <returns></returns>
        async Task<bool> pushDownRequestToBitComet(HttpClient client, string baseUrl, string downUrl, string save_path, string filename = "", string referrer = "", string user_agent = "", string cookie = "")
        {
            bool isOk = false;


            var values = new Dictionary<string, string>
            {
                { "url", downUrl},
                {"save_path",save_path },
                {"connection","200" },
                {"file_name",filename },
                {"referrer",referrer },
                {"user_agent",user_agent },
                {"cookie",cookie },
                {"mirror_url_list",""}
            };
            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(baseUrl + "/panel/task_add_httpftp_result", content);

            if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                isOk = true;

            return isOk;
        }

        /// <summary>
        /// 获取网页中比特彗星的默认存储地址
        /// </summary>
        /// <param Name="client"></param>
        /// <param Name="baseUrl"></param>
        /// <returns></returns>
        async Task<string> getBitCometDefaultSavePath(HttpClient client, string baseUrl)
        {
            string savePath = null;

            var response = await client.GetAsync(baseUrl + "/panel/task_add_httpftp");

            if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync());

                var savePathNode = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='save_path']");

                if (savePathNode != null)
                    savePath = savePathNode.GetAttributeValue("value", null);
            }

            return savePath;
        }

        async Task<string> getAria2DefaultSavePath(string apiUrl, string password, string ua)
        {
            string save_path = string.Empty;

            string myContent = "{\"jsonrpc\":\"2.0\"," +
                "\"method\": \"aria2.getGlobalOption\"," +
                "\"id\": " + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "," +
                "\"params\": [ \"" + password + "\"] }";

            HttpClient client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage rep = await client.PostAsync(apiUrl, content);

                if (rep.IsSuccessStatusCode)
                {
                    string strResult = await rep.Content.ReadAsStringAsync();

                    Aria2GlobalOptionRequest aria2GlobalOptionRequest = JsonConvert.DeserializeObject<Aria2GlobalOptionRequest>(strResult);

                    save_path = aria2GlobalOptionRequest?.result?.dir;
                }
            }
            catch
            {
                Debug.WriteLine("获取网页中比特彗星的默认存储地址时出错");
            }

            return save_path;
        }

        /// <summary>
        /// 检查Cookie是否可用后更新（Client及localSettings）
        /// </summary>
        /// <returns></returns>
        public async Task<bool> tryRefreshCookie(string cookie)
        {
            bool result = false;
            //先保存之前的Cookie，若Cookie无效则恢复原有Cookie
            string currentCookie;

            IEnumerable<string> value;
            bool haveCookie = Client.DefaultRequestHeaders.TryGetValues("Cookie", out value);
            if (haveCookie)
            {
                currentCookie = value.SingleOrDefault();
                RefreshCookie(cookie);

                //使用新Cookie登录不成功，恢复默认值
                result = await UpdateLoginInfo();
                if (!result)
                {
                    RefreshCookie(currentCookie);
                }
                else
                {
                    localSettings.Values["cookie"] = cookie;
                }
            }
            else
            {
                RefreshCookie(cookie);
                result = await UpdateLoginInfo();
                localSettings.Values["cookie"] = cookie;
            }

            return result;
        }

        /// <summary>
        /// 退出账号
        /// </summary>
        public async void LogoutAccount()
        {
            if (UserInfo == null)
            {
                return;
            }

            //退出账号
            await Client.GetAsync("https://passportapi.115.com/app/1.0/web/1.0/logout/logout/?goto=https%3A%2F%2F115.com%2F");

            //清空账号信息
            isEnterHiddenMode = false;
            QRCodeInfo = null;
            UserInfo = null;
            DeleteCookie();
            //if (!response.IsSuccessStatusCode)
            //{
            //    return QRCodeInfo;
            //}


            //var result = await response.Content.ReadAsStringAsync();
            //QRCodeInfo = JsonConvert.DeserializeObject<QRCodeInfo>(result);
        }

        /// <summary>
        /// 删除Cookie
        /// </summary>
        public static void DeleteCookie()
        {
            ApplicationData.Current.LocalSettings.Values["cookie"] = null;
        }

        //public async Task<string> GetVerifyAccountInfo()
        //{
        //    HttpResponseMessage response;
        //    try
        //    {
        //        response = await Client.GetAsync($"https://captchaapi.115.com/?ac=code&t=sign&callback=jQuery17202178075311826495_1683902571757&_={DateTimeOffset.Now.ToUnixTimeSeconds()}");
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        Toast.tryToast("网络异常", "检查115登录状态时出现异常：", e.Message);

        //        return null;
        //    }

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return null;
        //    }

        //    string strReuslt = await response.Content.ReadAsStringAsync();
        //    strReuslt = strReuslt.Replace("jQuery17202178075311826495_1683902571757(", "");
        //    strReuslt = strReuslt.Replace(");", "");

        //    try
        //    {
        //        UserInfo = JsonConvert.DeserializeObject<UserInfo>(strReuslt);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"发生错误：{ex.Message}");
        //    }
        //    return null;
        //}

        /// <summary>
        /// 获取下载链接
        /// </summary>
        /// <param name="pickCode"></param>
        /// <param name="ua"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetDownUrl(string pickCode, string ua)
        {
            Dictionary<string, string> downUrlList = new();
            
            var tm = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (AppSettings.IsRecordDownRequest)
            {
                var downUrlInfo = DataAccess.GetDownHistoryByPcAndUa(pickCode, ua);

                //检查链接是否失效
                if (downUrlInfo != null && (tm - downUrlInfo.addTime) < AppSettings.DownUrlOverdueTime)
                {
                    downUrlList.Add(downUrlInfo.fileName, downUrlInfo.trueUrl);
                    return downUrlList;
                }
            }

            string src = $"{{\"pickcode\":\"{pickCode}\"}}";
            var item = m115.encode(src, tm);
            byte[] data = item.Item1;
            byte[] keyBytes = item.Item2;

            string dataString = Encoding.ASCII.GetString(data);
            var dataUrlEncode = HttpUtility.UrlEncode(dataString);

            var url = $"http://proapi.115.com/app/chrome/downurl?t={tm}";
            var body = $"data={dataUrlEncode}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = new StringContent(body);
            httpRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            HttpResponseMessage response;
            string content;
            try
            {
                response = await Client.SendAsync(httpRequest);
                content = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return downUrlList;
            }

            if (response is { IsSuccessStatusCode: true, Content: not null })
            {
                DownUrlBase64EncryptInfo downUrlBase64EncryptInfo;
                try
                {
                    downUrlBase64EncryptInfo = JsonConvert.DeserializeObject<DownUrlBase64EncryptInfo>(content);
                }
                catch
                {
                    downUrlBase64EncryptInfo = null;
                }

                if (downUrlBase64EncryptInfo is { state: true })
                {
                    string base64Text = downUrlBase64EncryptInfo.data;

                    byte[] srcBase64 = Convert.FromBase64String(base64Text);

                    var rep = m115.decode(srcBase64, keyBytes);

                    //JObject json = JsonConvert.DeserializeObject<JObject>(rep);
                    var json = JObject.Parse(rep);

                    //如使用的pc是属于文件夹，url为false
                    foreach (var children in json)
                    {
                        var videoInfo = children.Value;

                        if (videoInfo["url"]!.HasValues)
                        {
                            var downUrl = videoInfo["url"]?["url"]?.ToString();
                            downUrlList.Add(videoInfo["file_name"]?.ToString() ?? string.Empty, downUrl);
                        }
                    }
                }

                //添加下载记录
                if (AppSettings.IsRecordDownRequest && downUrlList.Count != 0)
                {
                    DataAccess.AddDownHistory(new()
                    {
                        fileName = downUrlList.First().Key,
                        trueUrl = downUrlList.First().Value,
                        pickCode = pickCode,
                        ua = ua
                    });
                }
            }

            return downUrlList;
        }

        /// <summary>
        /// PotPlayer播放（原画）
        /// </summary>
        /// <param name="playItems"></param>
        /// <param name="userAgent"></param>
        /// <param name="fileName"></param>
        /// <param name="quality"></param>
        /// <param name="showWindow"></param>
        /// <param name="referrerUrl"></param>
        public static async void Play115SourceVideoWithPotPlayer(List<MediaPlayItem> playItems, string userAgent, string fileName, AppSettings.PlayQuality quality, bool showWindow = true, string referrerUrl = "https://115.com")
        {
            var isFirst = true;
            foreach (var mediaPlayItem in playItems)
            {
                var downUrl = await mediaPlayItem.GetUrl(quality);
                var subFile = await mediaPlayItem.GetOneSubFilePath();
                var addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" /sub=""{subFile}""";
                var arguments = @$" ""{downUrl}"" /user_agent=""{userAgent}"" /referer=""{referrerUrl}""{addSubFile}";

                if (isFirst)
                {
                    isFirst = false;
                    arguments += " /current";
                    StartProcess(fileName, arguments, exitedHandler: Process_Exited);
                    await Task.Delay(10000);
                }
                else
                {
                    arguments += " /add";
                    StartProcess(fileName, arguments, showWindow);
                    await Task.Delay(1000);
                }
            }
        }

        private static async void StartProcess(string fileName, string arguments, bool showWindow = false,EventHandler exitedHandler = null)
        {
            using var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = !showWindow;
            if (exitedHandler != null)
            {
                process.Exited += exitedHandler;
            }
            try
            {
                process.Start();
                await process.WaitForExitAsync();
            }
            catch (Win32Exception e)
            {
                Toast.tryToast("播放错误", "调用播放器时出现异常", e.Message);
            }

        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Debug.WriteLine("Process_Exited");
        }

        /// <summary>
        /// 解析m3u8内容
        /// </summary>
        /// <param name="m3U8Info"></param>
        /// <returns></returns>
        public async Task<m3u8Info> GetM3U8Content(m3u8Info m3U8Info)
        {
            var strResult = string.Empty;

            try
            {
                var response = await Client.GetAsync(m3U8Info.Url);
                strResult = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Debug.WriteLine("获取m3u8链接失败");
            }

            var lineList = strResult.Split(new char[] { '\n' });

            for (int i = 0; i < lineList.Count(); i++)
            {
                var lineText = lineList[i].Trim('\r');

                var re = Regex.Match(lineText, @"^#EXTINF:(\d*\.\d*),$");
                if (re.Success)
                {
                    var strUrl = lineList[i + 1];
                    var doubleSecond = Convert.ToDouble(re.Groups[1].Value);
                    m3U8Info.ts_info_list.Add(new tsInfo() { Second = doubleSecond, Url = strUrl });
                }
            }

            return m3U8Info;
        }

        public async Task<List<m3u8Info>> GetM3U8InfoByPickCode(string pickCode)
        {
            List<m3u8Info> m3U8Infos = new();

            string strResult;
            try
            {
                var response = await Client.GetAsync($"https://v.anxia.com/site/api/video/m3u8/{pickCode}.m3u8");
                strResult = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
                //Debug.WriteLine("获取m3u8链接失败");
            }

            var lineList = strResult.Split(new char[] { '\n' });
            for (int i = 0; i < lineList.Count(); i++)
            {
                var lineText = lineList[i].Trim('\r');

                var re = Regex.Match(lineText, @"BANDWIDTH=(\d*),RESOLUTION=(\w*),NAME=""(\w*)""");
                if (re.Success)
                {
                    m3U8Infos.Add(new m3u8Info(re.Groups[3].Value, re.Groups[1].Value, re.Groups[2].Value, lineList[i + 1].Trim('\r')));
                }
            }

            // 检查账号是否异常
            if (m3U8Infos.Count == 0 && strResult.Contains(Const.AccountAnomalyTip))
            {
                var window = CreateWindowToVerifyAccount();
                
                if (window.Content is not VerifyAccountPage page) return m3U8Infos;

                var isClosed = false;
                page.VerifyAccountCompleted += async (_, iSucceeded) =>
                {
                    // 失败
                    if (!iSucceeded)
                    {
                        isClosed = true;
                        return;
                    }

                    m3U8Infos = await GetM3U8InfoByPickCode(pickCode);
                    isClosed = true;
                };

                window.Activate();

                //堵塞，直到关闭输入验证码的window
                while (!isClosed)
                {
                    await Task.Delay(2000);
                }

                return m3U8Infos;
            }

            //排序
            m3U8Infos = m3U8Infos.OrderByDescending(x => x.Bandwidth).ToList();

            return m3U8Infos;
        }


        public static Window CreateWindowToVerifyAccount()
        {
            var window = new CommonWindow("验证账号", 360, 560);
            var page = new VerifyAccountPage(window);

            window.Content = page;

            return window;
                ;
        }

        /// <summary>
        /// mpv播放
        /// </summary>
        /// <param name="playItems"></param>
        /// <param name="userAgent"></param>
        /// <param name="fileName"></param>
        /// <param name="quality"></param>
        /// <param name="showWindow"></param>
        /// <param name="referrerUrl"></param>
        public async void Play115SourceVideoWithMpv(List<MediaPlayItem> playItems, string userAgent, string fileName, AppSettings.PlayQuality quality, bool showWindow = true, string referrerUrl = "https://115.com")
        {
            var arguments = string.Empty;
            foreach (var mediaPlayItem in playItems)
            {
                var playUrl = await mediaPlayItem.GetUrl(quality);
                var title = mediaPlayItem.Title;
                var subFile = await mediaPlayItem.GetOneSubFilePath();

                var addTitle = string.Empty;
                if (string.IsNullOrEmpty(title))
                {
                    var matchTitle = Regex.Match(HttpUtility.UrlDecode(playUrl), @"/([-@.\w]+?\.\w+)\?t=");
                    if (matchTitle.Success)
                        title = matchTitle.Groups[1].Value;
                }

                if(!string.IsNullOrEmpty(title))
                    addTitle = @$"  --title=""播放 - {title}""";

                var addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" --sub-file=""{subFile}""";

                arguments += " --{" + @$" ""{playUrl}"" --referrer=""{referrerUrl}"" --user-agent=""{userAgent}"" --force-media-title=""{mediaPlayItem.FileName}""{addTitle}{addSubFile}" + " --}";
            }

            StartProcess(fileName, arguments, showWindow);

        }

        /// <summary>
        /// vlc播放（原画）
        /// </summary>
        /// <param name="playItems"></param>
        /// <param name="userAgent"></param>
        /// <param name="fileName"></param>
        /// <param name="quality"></param>
        /// <param name="showWindow"></param>
        /// <param name="referrerUrl"></param>
        public static async void Play115SourceVideoWithVlc(List<MediaPlayItem> playItems, string userAgent, string fileName, AppSettings.PlayQuality quality, bool showWindow = true, string referrerUrl = "https://115.com")
        {
            var arguments = string.Empty;
            foreach (var mediaPlayItem in playItems)
            {
                var playUrl = await mediaPlayItem.GetUrl(quality);
                var title = mediaPlayItem.Title;
                var subFile = await mediaPlayItem.GetOneSubFilePath();

                var addTitle = string.Empty;
                if (string.IsNullOrEmpty(title))
                {
                    var matchTitle = Regex.Match(HttpUtility.UrlDecode(playUrl), @"/([-@.\w]+?\.\w+)\?t=");
                    if (matchTitle.Success)
                        addTitle = matchTitle.Groups[1].Value;
                }

                if (!string.IsNullOrEmpty(title))
                {
                    addTitle = @$" :meta-title=""{title}""";
                }
                var addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" :sub-file=""{subFile}""";

                arguments += @$" ""{playUrl}"" :http-referrer=""{referrerUrl}"" :http-user-agent=""{userAgent}""{addTitle}{addSubFile}";
            }

            StartProcess(fileName, arguments,showWindow);
        }

        /// <summary>
        /// vlc播放(m3u8)
        /// </summary>
        /// <param name="pickCode"></param>
        public async void PlayByVlc(string pickCode)
        {
            var m3U8Infos = await GetM3U8InfoByPickCode(pickCode);
            if (m3U8Infos.Count > 0)
            {
                //选择最高分辨率的播放
                FileMatch.PlayByVlc(m3U8Infos[0].Url, AppSettings.VlcExePath);
            }
        }

        public enum PlayMethod { pot, mpv, vlc }

        /// <summary>
        /// 原画播放
        /// </summary>
        /// <param name="playItems"></param>
        /// <param name="playMethod"></param>
        /// <param name="xamlRoot"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task PlayVideoWithPlayer(List<MediaPlayItem> playItems, PlayMethod playMethod, XamlRoot xamlRoot, IProgress<int> progress = null)
        {
            string savePath;
            string ua;

            //检查播放器设置
            switch (playMethod)
            {
                case PlayMethod.pot:
                    savePath = AppSettings.PotPlayerExePath;
                    ua = GetInfoFromNetwork.UserAgent;
                    break;
                case PlayMethod.mpv:
                    savePath = AppSettings.MpvExePath;
                    ua = GetInfoFromNetwork.UserAgent;
                    break;
                case PlayMethod.vlc:
                    savePath = AppSettings.VlcExePath;
                    ua = GetInfoFromNetwork.UserAgent;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playMethod), playMethod, null);
            }
            
            //播放路径检查选择
            if (string.IsNullOrEmpty(savePath))
            {
                var dialog = new ContentDialog()
                {
                    XamlRoot = xamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "播放失败",
                    CloseButtonText = "返回",
                    DefaultButton = ContentDialogButton.Close,
                    Content = "未设置播放器程序路径，请到设置中设置"
                };

                await dialog.ShowAsync();

                return;
            }

            var quality = (AppSettings.PlayQuality)AppSettings.DefaultPlayQuality;

            //打开一层文件夹并添加可播放文件到List中
            playItems = await MediaPlayItem.OpenFolderThenInsertVideoFileToMediaPlayItem(playItems, GlobalWebApi);

            for (int i = 0; i < playItems.Count; i++)
            {
                var playItem = playItems[i];

                // 获取下载链接
                await playItem.GetUrl(quality);
                Debug.WriteLine("成功获取");
                
                // 下载字幕
                var subPath = await playItem.GetOneSubFilePath();

                if (!string.IsNullOrEmpty(subPath))
                {
                    Debug.WriteLine("完成字幕");
                }

                progress?.Report(i);
            }

            //检查播放方式
            switch (playMethod)
            {
                case PlayMethod.pot:
                    Play115SourceVideoWithPotPlayer(playItems, userAgent: ua, savePath, quality, true);
                    break;
                case PlayMethod.mpv:
                    Play115SourceVideoWithMpv(playItems, userAgent: ua, savePath, quality, false);
                    break;
                case PlayMethod.vlc:
                    //vlc不支持带“; ”的user-agent
                    Play115SourceVideoWithVlc(playItems, userAgent: ua, savePath, quality, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playMethod), playMethod, null);
            }
        }
        
        public async Task<string> TryDownSubFile(string fileName, string pickCode)
        {
            //为预防字幕文件没有具体名称，只有数字，更改字幕文件名为 pickCode+字幕文件名
            fileName = $"{pickCode}_{fileName}";
            
            if (string.IsNullOrEmpty(pickCode)) return null;
            var subSavePath = AppSettings.SubSavePath;
            var subFile = Path.Combine(subSavePath, fileName);

            //已存在
            if (File.Exists(subFile))
                return subFile;

            var ua = GetInfoFromNetwork.UserAgent;

            //不存在则获取下载链接并下载
            var subUrlList = await GetDownUrl(pickCode, ua);
            if (subUrlList.Count == 0)
            {
                return null;
            }

            var subUrl = subUrlList.First().Value;

            subFile = await GetInfoFromNetwork.downloadFile(subUrl, subSavePath, fileName, false, new()
            {
                {"User-Agent", ua }
            });

            return subFile;

        }
        
    }
}
