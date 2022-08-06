using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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

        public WebApi()
        {
            InitializeInternet();
        }

        /// <summary>
        /// 添加user-agent和cookie
        /// </summary>
        public void InitializeInternet()
        {
            var handler = new HttpClientHandler { UseCookies = false };
            Client = new HttpClient(handler);
            Client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36 115Browser/25.0.1.0");

            var cookie = (string)localSettings.Values["cookie"];

            //cookie不为空且可用
            if (!string.IsNullOrEmpty(cookie))
            {
                Client.DefaultRequestHeaders.Add("Cookie", cookie);
            }
        }

        /// <summary>
        /// 更新cookie
        /// </summary>
        /// <param name="cookie"></param>
        public void RefreshCookie(string cookie)
        {
            Client.DefaultRequestHeaders.Remove("Cookie");
            Client.DefaultRequestHeaders.Add("Cookie", cookie);
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        /// <returns>true - 登录，false - 未登录</returns>
        public async Task<bool> UpdateLoginInfo()
        {
            bool result = false;
            //Uri uri = new Uri("https://webapi.115.com/files?aid=1&cid=2223208807868137192&o=user_ptime&asc=0&offset=0&show_dir=1&limit=56&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json");
            var response = await Client.GetAsync($"https://my.115.com/?ct=ajax&ac=nav&callback=jQuery172046995607070659906_1647774910536&_={DateTimeOffset.Now.ToUnixTimeSeconds()}");

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
            var response = Client.PostAsync("https://115.com/?ac=setting&even=saveedit&is_wl_tpl=1", content).Result;

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

        public async Task GetAllFileInfoToDataAccess(List<string> cidList, GetFilesProgressInfo getFilesProgressInfo, IProgress<GetFileProgessIProgress> progress = null)
        {
            foreach (var cid in cidList)
            {
                //fileProgressInfo.datumList = new List<Datum>();

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
                    //fileProgressInfo.status = ProgressStatus.error;

                    progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, status = ProgressStatus.error, sendCountPerMinutes = 1 });

                    // 退出
                    return;
                }


                // 该文件已存在数据库里
                if (DataAccess.IsLastestFileDataExists(cidCategory.pick_code, cidCategory.utime))
                {
                    //统计上下级文件夹所含文件的数量
                    //文件数量
                    getFilesProgressInfo.FilesCount += cidCategory.count;
                    //文件夹数量
                    getFilesProgressInfo.FolderCount += cidCategory.folder_count;

                    var cpm = sendCount * 60 / (DateTimeOffset.Now.ToUnixTimeSeconds() - nowDate);
                    progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });

                }
                //之前未添加
                else
                {

                    //获取当前文件夹下所有文件
                    getFilesProgressInfo = await TraverseAllFileInfo(cid, getFilesProgressInfo, progress);

                    DataAccess.AddFilesInfo(FolderCategory.ConvertFolderToDatum(cidCategory, cid));
                }
            }

            // 完成
            progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, status = ProgressStatus.done, sendCountPerMinutes = 1 });

        }

        /// <summary>
        /// 获取所有文件信息
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="webFileInfoList"></param>
        /// <returns></returns>
        public async Task<GetFilesProgressInfo> TraverseAllFileInfo(string cid, GetFilesProgressInfo getFilesProgressInfo, IProgress<GetFileProgessIProgress> progress = null)
        {
            //var webFileInfoList = fileProgressInfo.datumList;

            //统计请求速度
            int sendCount = 0;
            long nowDate = DateTimeOffset.Now.ToUnixTimeSeconds();

            //successCount++;
            await Task.Delay(1000);

            //查询下一级文件信息
            var WebFileInfo = GetFile(cid);
            sendCount++;

            if (WebFileInfo.state)
            {
                foreach (var item in WebFileInfo.data)
                {
                    //文件夹
                    if(item.fid == null)
                    {
                        getFilesProgressInfo.FolderCount++;

                        //查询数据库是否存在
                       if (DataAccess.IsLastestFileDataExists(item.pc, item.te))
                        {
                            //统计下级文件夹所含文件的数量
                            //通过数据库获取
                            var datumList = DataAccess.GetAllFilesTraverse(item.cid);

                            //文件数量
                            getFilesProgressInfo.FilesCount += datumList.Count;

                            var cpm = sendCount * 60 / (DateTimeOffset.Now.ToUnixTimeSeconds() - nowDate);
                            progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });
                        }
                        else
                        {


                            DataAccess.AddFilesInfo(item);

                            //webFileInfoList = await TraverseAllFileInfo(item.cid, fileProgressInfo, progress);
                            getFilesProgressInfo = await TraverseAllFileInfo(item.cid, getFilesProgressInfo, progress);
                        }
                    }
                    //文件
                    else
                    {
                        getFilesProgressInfo.FilesCount++;

                        var cpm = sendCount * 60 / (DateTimeOffset.Now.ToUnixTimeSeconds() - nowDate);
                        progress.Report(new GetFileProgessIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });

                        DataAccess.AddFilesInfo(item);

                        //webFileInfoList.Add(item);

                    }
                }
            }

            return getFilesProgressInfo;
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public WebFileInfo GetFile(string cid, int limit = 40,bool userApi2=false)
        {
            WebFileInfo WebFileInfoResult = new();

            string url;
            if (!userApi2)
            {
                url = $"https://webapi.115.com/files?aid=1&cid={cid}&o=user_ptime&asc=0&offset=0&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json";
            }
            else
            {
                //旧接口只有t，没有修改时间（te），创建时间（tp）
                url = $"https://aps.115.com/natsort/files.php?aid=1&cid={cid}&o=file_name&asc=1&offset=0&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json&fc_mix=0&type=&star=&is_share=&suffix=&custom_order=";
            }

            var response = Client.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
            {
                return WebFileInfoResult;
            }
            var strResult = response.Content.ReadAsStringAsync().Result;

            WebFileInfoResult = JsonConvert.DeserializeObject<WebFileInfo>(strResult);

            //te，tp简单用t替换，接口2没有te,tp
            if (userApi2)
            {
                foreach(var item in WebFileInfoResult.data)
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
                        int dateInt = FileMatch.ConvertDateTimeToInt32(item.t);
                        item.te = item.tp = dateInt;
                    }


                }
            }

            //接口1出错，使用接口2
            if (WebFileInfoResult.errNo == 20130827 && userApi2 == false)
            {
                WebFileInfoResult = GetFile(cid, limit, true);
            }
            //未加载全部
            else if(WebFileInfoResult.count > limit)
            {
                WebFileInfoResult = GetFile(cid, WebFileInfoResult.count, userApi2);
            }

            return WebFileInfoResult;
        }

        /// <summary>
        /// 获取文件夹属性（含大小和数量）
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<FolderCategory> GetFolderCategory(string cid)
        {
            FolderCategory WebFileInfoResult = new();

            string url = $"https://webapi.115.com/category/get?cid={cid}";

            HttpResponseMessage response = await Client.GetAsync(url);

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

        /// <summary>
        /// 请求115浏览器下载
        /// </summary>
        /// <param name="videoInfoList"></param>
        public async void RequestDown(List<Datum> videoInfoList)
        {
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
                // URI launched
            }
            else
            {
                // URI launch failed
            }
        }

        /// <summary>
        /// 检查Cookie是否可用后更新（Client及localSettings）
        /// </summary>
        /// <returns></returns>
        public async Task<bool> tryRefreshCookie(string cookie)
        {
            bool result = false;
            //先保存之前的Cookie，若Cookie无效则回复原有Cookie
            string currentCookie="";

            IEnumerable<string> value;
            bool haveCookie = Client.DefaultRequestHeaders.TryGetValues("Cookie",out value);
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
    }
}
