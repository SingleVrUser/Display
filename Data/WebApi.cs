using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Windows.Storage;

namespace Data
{
    public class WebApi
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private HttpClient QRCodeClient;
        public HttpClient Client;
        public static UserInfo UserInfo;
        public static QRCodeInfo QRCodeInfo;
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
            Client = GetInfoFromNetwork.CreateClient(new Dictionary<string, string> { { "user-agent", GetInfoFromNetwork.BrowserUserAgent } });

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

        public static Windows.Web.Http.HttpClient GetVideoClient()
        {
            var httpClient = new Windows.Web.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("Referer", "https://115.com/?cid=0&offset=0&tab=&mode=wangpan");
            httpClient.DefaultRequestHeaders.Add("User-Agent", GetInfoFromNetwork.BrowserUserAgent);
            return httpClient;
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
                FileMatch.tryToast("网络异常", "检查115登录状态时出现异常：", e.Message);

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
                FileMatch.tryToast("网络异常", "检查115隐藏状态时出现异常：", e.Message);

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

                await Task.Delay(1000);

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
            await Task.Delay(1000);

            //查询下一级文件信息
            var WebFileInfo = await GetFileAsync(cid,LoadAll:true);
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
                FileMatch.tryToast("网络异常", "改变115排序顺序时发生异常：", e.Message);
            }
        }

        /// <summary>
        /// 删除115文件
        /// </summary>
        /// <param Name="pid"></param>
        /// <param Name="fids"></param>
        /// <param Name="ignore_warn"></param>
        /// <returns></returns>
        public async Task DeleteFiles(string pid,List<string> fids,int ignore_warn = 1)
        {
            var values = new Dictionary<string, string>
            {
                { "pid", pid},
            };

            for(int i = 0;i < fids.Count;i++)
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
                FileMatch.tryToast("网络异常", "删除115文件时发生异常：", e.Message);
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
                FileMatch.tryToast("网络异常", "从115回收站恢复文件时发生异常：", e.Message);
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param Name="pid"></param>
        /// <param Name="fids"></param>
        /// <returns></returns>
        public async Task MoveFiles(string pid, List<string> fids)
        {
            var values = new Dictionary<string, string>
            {
                { "pid", pid},
                { "move_proid", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_-{new Random().Next(1,99)}_0"},

            };

            for (int i = 0; i < fids.Count; i++)
            {
                values.Add($"fid[{i}]", fids[i]);
            }

            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response;
            try
            {
                response = await Client.PostAsync("https://webapi.115.com/files/move", content);
            }
            catch (AggregateException e)
            {
                FileMatch.tryToast("网络异常", "移动115文件时发生异常：", e.Message);
            }
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
        public async Task<WebFileInfo> GetFileAsync(string cid, int limit = 40, int offset = 0, bool useApi2 = false, bool LoadAll = false, OrderBy orderBy = OrderBy.user_ptime,int asc=0)
        {
            WebFileInfo WebFileInfoResult = new();

            string url;
            if (!useApi2)
            {
                url = $"https://webapi.115.com/files?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json";
            }
            else
            {
                //旧接口只有t，没有修改时间（te），创建时间（tp）
                url = $"https://aps.115.com/natsort/files.php?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json&fc_mix=0&type=&star=&is_share=&suffix=&custom_order=";
            }

            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync(url);
            }
            catch (AggregateException e)
            {
                FileMatch.tryToast("网络异常", "获取文件信息时出现异常：", e.Message);
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
                WebFileInfoResult = await GetFileAsync(cid, limit, offset, true, LoadAll, orderBy, asc);
            }
            //需要加载全部，但未加载全部
            else if (LoadAll && WebFileInfoResult.count > limit)
            {
                WebFileInfoResult = await GetFileAsync(cid, WebFileInfoResult.count, offset,useApi2,LoadAll, orderBy, asc);
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
                FileMatch.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return result;
            }
            catch (HttpRequestException e)
            {
                FileMatch.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
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
            FolderCategory WebFileInfoResult = null;

            string url = $"https://webapi.115.com/category/get?cid={cid}";
            HttpResponseMessage response;
            try
            {
                response = await Client.GetAsync(url);
            }
            catch (AggregateException e)
            {
                FileMatch.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return WebFileInfoResult;
            }
            catch (HttpRequestException e)
            {
                FileMatch.tryToast("网络异常", "获取文件夹属性时出现异常：", e.Message);
                return WebFileInfoResult;
            }

            if (response.IsSuccessStatusCode)
            {
                var strResult = await response.Content.ReadAsStringAsync();

                WebFileInfoResult = JsonConvert.DeserializeObject<FolderCategory>(strResult);
            }

            return WebFileInfoResult;
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
                success = await RequestDownByBitComet(videoInfoList, GetInfoFromNetwork.DesktopUserAgent, save_path: savePath, topFolderName: topFolderName);
            }
            //Arai2也支持文件和文件夹
            else if (downType == downType.aria2)
            {
                success = await RequestDownByAria2(videoInfoList, GetInfoFromNetwork.DesktopUserAgent, save_path: savePath, topFolderName: topFolderName);
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

            var downRequest = new Browser_115_Request();
            //UID
            downRequest.uid = videoInfoList[0].uid;

            //KEY
            if (QRCodeInfo == null)
            {
                await GetQRCodeInfo();
            }
            downRequest.key = QRCodeInfo.data.uid;

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
            var options = new Windows.System.LauncherOptions();
            options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseLess;

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
                    WebFileInfo webFileInfo = await GetFileAsync(datum.cid,LoadAll:true);
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
                    var downUrlList = GetDownUrl(datum.pc, ua);
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

            var urlList = GetDownUrl(datum.pc, ua);

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
            WebFileInfo webFileInfo = await GetFileAsync(datum.cid,LoadAll:true);

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
            var Content = new StringContent(myContent, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage rep = await client.PostAsync(apiUrl, Content);

                if (rep.IsSuccessStatusCode)
                {
                    string strResult = await rep.Content.ReadAsStringAsync();

                    Aria2GlobalOptionRequest aria2GlobalOptionRequest = JsonConvert.DeserializeObject<Aria2GlobalOptionRequest>(strResult);

                    save_path = aria2GlobalOptionRequest?.result?.dir;
                }
            }
            catch
            {
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

        /// <summary>
        /// 获取下载链接
        /// </summary>
        /// <param Name="pickcode"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetDownUrl(string pickcode, string ua)
        {
            Dictionary<string, string> downUrlList = new();

            long tm = DateTimeOffset.Now.ToUnixTimeSeconds();

            if (AppSettings.IsRecordDownRequest)
            {
                var downUrlInfo = DataAccess.GetDownHistoryBypcAndua(pickcode, ua);

                //检查链接是否失效
                if (downUrlInfo!=null && (tm - downUrlInfo.addTime) > AppSettings.DownUrlOverdueTime)
                {
                    downUrlList.Add(downUrlInfo.fileName, downUrlInfo.trueUrl);
                    return downUrlList;
                }
            }

            string src = $"{{\"pickcode\":\"{pickcode}\"}}";
            var item = m115.encode(src, tm);
            byte[] data = item.Item1;
            byte[] keyBytes = item.Item2;

            string dataString = Encoding.ASCII.GetString(data);
            var dataUrlEncode = System.Web.HttpUtility.UrlEncode(dataString);

            var client = new RestClient($"http://proapi.115.com/app/chrome/downurl?t={tm}");
            var request = new RestRequest();
            request.AddHeader("User-Agent", ua);
            request.AddHeader("Cookie", AppSettings._115_Cookie);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            var body = $"data={dataUrlEncode}";
            request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);

            RestResponse response;
            try
            {
                response = client.Post(request);
            }
            catch
            {
                return downUrlList;
            }
                
            DownUrlBase64EncryptInfo downurl_base64EncryptInfo;

            if (response.IsSuccessful && response.Content != null)
            {
                try
                {
                    downurl_base64EncryptInfo = JsonConvert.DeserializeObject<DownUrlBase64EncryptInfo>(response.Content);
                }
                catch
                {
                    downurl_base64EncryptInfo = null;
                }

                if (downurl_base64EncryptInfo != null && downurl_base64EncryptInfo.state)
                {
                    string base64Text = downurl_base64EncryptInfo.data;

                    byte[] srcBase64 = Convert.FromBase64String(base64Text);

                    var rep = m115.decode(srcBase64, keyBytes);

                    //JObject json = JsonConvert.DeserializeObject<JObject>(rep);
                    var json = JObject.Parse(rep);

                    //如使用的pc是属于文件夹，url为false
                    foreach (var children in json)
                    {
                        var videoInfo = children.Value;

                        if (videoInfo["url"].HasValues)
                        {
                            var downUrl = videoInfo["url"]?["url"].ToString();
                            downUrlList.Add(videoInfo["file_name"].ToString(), downUrl);
                        }

                    }
                }

                //添加下载记录
                if (AppSettings.IsRecordDownRequest && downUrlList.Count != 0)
                {
                    DataAccess.AddDownHistory(new()
                    {
                        fileName = downUrlList.First().Key.ToString(),
                        trueUrl = downUrlList.First().Value.ToString(),
                        pickCode = pickcode,
                        ua = ua
                    });
                }
            }

            return downUrlList;
        }

        /// <summary>
        /// PotPlayer播放（原画）
        /// </summary>
        /// <param Name="playUrl"></param>
        /// <param Name="FileName"></param>
        /// <param Name="showWindow"></param>
        /// <param Name="referrerUrl"></param>
        /// <param Name="user_agnet"></param>
        public static void Play115SourceVideoWithPotPlayer(string playUrl, string user_agnet, string FileName, bool showWindow = true, string referrerUrl = "https://115.com", string subFile = null)
        {
            var process = new Process();

            string addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" /sub=""{subFile}""";

            process.StartInfo.FileName = FileName;
            process.StartInfo.Arguments = @$" ""{playUrl}"" /user_agent=""{user_agnet}"" /referer=""{referrerUrl}""{addSubFile}";
            process.StartInfo.UseShellExecute = false;
            if (!showWindow)
            {
                process.StartInfo.CreateNoWindow = true;
            }

            process.Start();
        }

        /// <summary>
        /// PotPlayer播放(m3u8)
        /// </summary>
        /// <param Name="pickCode"></param>
        public async void PlayeByPotPlayer(string pickCode)
        {
            var m3U8Infos = await Getm3u8InfoByPickCode(pickCode);
            if (m3U8Infos.Count > 0)
            {
                //选择最高分辨率的播放
                FileMatch.PlayByPotPlayer(m3U8Infos[0].Url);
            }
        }

        /// <summary>
        /// 解析m3u8内容
        /// </summary>
        /// <param Name="m3u8_info"></param>
        /// <returns></returns>
        public async Task<m3u8Info> Getm3u8Content(m3u8Info m3u8_info)
        {
            HttpResponseMessage response;
            string strReuslt = string.Empty;

            try
            {
                response = await Client.GetAsync(m3u8_info.Url);
                strReuslt = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                Debug.WriteLine("获取m3u8链接失败");
            }

            var lineList = strReuslt.Split(new char[] { '\n' });

            for (int i = 0; i < lineList.Count(); i++)
            {
                var lineText = lineList[i].Trim('\r');

                var re = Regex.Match(lineText, @"^#EXTINF:(\d*\.\d*),$");
                if (re.Success)
                {
                    var strUrl = lineList[i + 1];
                    var doubleSecond = Convert.ToDouble(re.Groups[1].Value);
                    m3u8_info.ts_info_list.Add(new tsInfo() { Second = doubleSecond, Url = strUrl });
                }
            }

            return m3u8_info;
        }

        public async Task<List<m3u8Info>> Getm3u8InfoByPickCode(string pickCode)
        {
            List<m3u8Info> m3U8Infos = new();

            HttpResponseMessage response;
            string strResult;
            try
            {
                response = await Client.GetAsync($"https://v.anxia.com/site/api/video/m3u8/{pickCode}.m3u8");
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
                    //Debug.WriteLine(re.Groups[0].Value);
                }
            }

            //排序
            m3U8Infos = m3U8Infos.OrderByDescending(x => x.Bandwidth).ToList();

            return m3U8Infos;
        }

        /// <summary>
        /// mpv播放
        /// </summary>
        /// <param Name="playUrl"></param>
        /// <param Name="FileName"></param>
        /// <param Name="showWindow"></param>
        /// <param Name="referrerUrl"></param>
        /// <param Name="user_agnet"></param>
        public void Play115SourceVideoWithMpv(string playUrl, string user_agnet, string FileName, bool showWindow = true, string referrerUrl = "https://115.com", string title = null, string subFile = null)
        {
            var process = new Process();

            string addTitle = string.Empty;

            if (title == null)
            {
                var matchTitle = Regex.Match(HttpUtility.UrlDecode(playUrl), @"/([-@.\w]+?\.\w+)\?t=");
                if (matchTitle.Success)
                    addTitle = @$"  --title=""{matchTitle.Groups[1].Value}""";
            }
            else
            {
                addTitle = @$"  --title=""{title}""";
            }

            string addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" --sub-file=""{subFile}""";

            process.StartInfo.FileName = FileName;

            process.StartInfo.FileName = FileName;
            process.StartInfo.Arguments = @$" ""{playUrl}"" --referrer=""{referrerUrl}"" --user-agent=""{user_agnet}""{addTitle}{addSubFile}";
            process.StartInfo.UseShellExecute = false;
            if (!showWindow)
            {
                process.StartInfo.CreateNoWindow = true;
            }

            process.Start();
        }

        /// <summary>
        /// vlc播放（原画）
        /// </summary>
        /// <param Name="playUrl"></param>
        /// <param Name="FileName"></param>
        /// <param Name="showWindow"></param>
        /// <param Name="referrerUrl"></param>
        /// <param Name="user_agnet"></param>
        public static void Play115SourceVideoWithVlc(string playUrl, string user_agnet, string FileName, bool showWindow = true, string referrerUrl = "https://115.com", string title = null, string subFile = null)
        {
            var process = new Process();

            string addTitle = string.Empty;
            if (title == null)
            {
                var matchTitle = Regex.Match(HttpUtility.UrlDecode(playUrl), @"/([-@.\w]+?\.\w+)\?t=");
                if (matchTitle.Success)
                    addTitle = @$" :meta-title=""{matchTitle.Groups[1].Value}""";
            }
            else
            {
                addTitle = @$" :meta-title=""{title}""";
            }

            string addSubFile = string.IsNullOrEmpty(subFile) ? string.Empty : @$" :sub-file=""{subFile}""";

            process.StartInfo.FileName = FileName;
            process.StartInfo.Arguments = @$" ""{playUrl}"" :http-referrer=""{referrerUrl}"" :http-user-agent=""{user_agnet}""{addTitle}{addSubFile}";
            //process.StartInfo.Arguments = @$" ""{playUrl}""";
            process.StartInfo.UseShellExecute = false;
            if (!showWindow)
            {
                process.StartInfo.CreateNoWindow = true;
            }

            process.Start();
        }

        /// <summary>
        /// vlc播放(m3u8)
        /// </summary>
        /// <param Name="playUrl"></param>
        /// <param Name="FileName"></param>
        /// <param Name="showWindow"></param>
        /// <param Name="referrerUrl"></param>
        /// <param Name="user_agnet"></param>
        public async void PlayByVlc(string pickCode)
        {
            var m3U8Infos = await Getm3u8InfoByPickCode(pickCode);
            if (m3U8Infos.Count > 0)
            {
                //选择最高分辨率的播放
                FileMatch.PlayByVlc(m3U8Infos[0].Url, AppSettings.VlcExePath);
            }
        }

        public enum playMethod { pot, mpv, vlc }
        /// <summary>
        /// 原画播放
        /// </summary>
        /// <param Name="pickcode"></param>
        public async Task PlayVideoWithOriginUrl(string pickcode, playMethod playMethod, XamlRoot xamlRoot, SubInfo subInfo = null)
        {
            //播放路径检查选择
            string savePath = string.Empty;
            string ua = string.Empty;
            string downUrl;
            string subFile = null;

            //检查播放器设置
            switch (playMethod)
            {
                case playMethod.pot:
                    savePath = AppSettings.PotPlayerExePath;
                    ua = GetInfoFromNetwork.DesktopUserAgent;
                    break;
                case playMethod.mpv:
                    savePath = AppSettings.MpvExePath;
                    ua = GetInfoFromNetwork.DesktopUserAgent;
                    break;
                case playMethod.vlc:
                    savePath = AppSettings.VlcExePath;
                    ua = GetInfoFromNetwork.BrowserUserAgent;
                    //ua = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.59 Safari/537.36 115Browser/8.3.0";
                    //ua = "Mozilla/5.0; Windows NT/10.0.19044; 115Desktop/2.0.1.7";
                    //ua = "VLC/3.0.12.1 LibVLC/3.0.12.1";
                    //ua = "nPlayer/3.0";
                    //ua = "Lavf/58.76.100";
                    break;
            }
            if (string.IsNullOrEmpty(savePath))
            {
                ContentDialog dialog = new ContentDialog()
                {
                    XamlRoot = xamlRoot,
                    Title = "播放失败",
                    CloseButtonText = "返回",
                    DefaultButton = ContentDialogButton.Close,
                    Content = "未设置播放器程序路径，请到设置中设置"
                };

                await dialog.ShowAsync();

                return;
            }

            //检查字幕
            if (AppSettings.IsFindSub && subInfo != null)
                subFile = await TryDownSubFile(subInfo.name,subInfo.pickcode);

            //获取下载地址
            var downUrlList = GetDownUrl(pickcode, ua);
            if (downUrlList.Count == 0) return;
            downUrl = downUrlList.First().Value;

            //检查播放方式
            switch (playMethod)
            {
                case playMethod.pot:
                    Play115SourceVideoWithPotPlayer(downUrl, user_agnet: ua, savePath, false, subFile: subFile);
                    break;
                case playMethod.mpv:
                    Play115SourceVideoWithMpv(downUrl, user_agnet: ua, savePath, false, title: downUrlList.First().Key, subFile: subFile);
                    break;
                case playMethod.vlc:
                    //vlc不支持带“; ”的user-agent
                    Play115SourceVideoWithVlc(downUrl, user_agnet: ua, savePath, false, title: downUrlList.First().Key, subFile: subFile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(playMethod), playMethod, null);
            }
        }

        public async Task<string> TryDownSubFile(string fileName, string pickCode)
        {
            //为预防字幕文件没有具体名称，只有数字，更改字幕文件名为 pickCode+字幕文件名
            fileName = $"{pickCode}_{fileName}";

            if (!string.IsNullOrEmpty(pickCode))
            {
                var Sub_SavePath = AppSettings.Sub_SavePath;
                string subFile = Path.Combine(Sub_SavePath, fileName);

                //已存在
                if (File.Exists(subFile))
                    return subFile;

                string ua = GetInfoFromNetwork.DesktopUserAgent;

                //不存在则获取下载链接并下载
                var subUrlList = GetDownUrl(pickCode, ua);
                if (subUrlList.Count == 0)
                {
                    return null;
                }

                var sub_Url = subUrlList.First().Value;

                subFile = await GetInfoFromNetwork.downloadFile(sub_Url, Sub_SavePath, fileName, false, new()
                {
                    {"User-Agent", ua }
                });

                return subFile;
            }

            return null;
        }
    }
}
