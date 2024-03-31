using Display.Extensions;
using Display.Helper.Crypto;
using Display.Helper.Date;
using Display.Helper.Network;
using Display.Models.Data;
using Display.Services.Upload;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Display.Helper.Notifications;
using Display.Models.Api.Aria2;
using Display.Models.Api.OneOneFive;
using Display.Models.Api.OneOneFive.Browser;
using Display.Models.Api.OneOneFive.Down;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Api.OneOneFive.OfflineDown;
using Display.Models.Api.OneOneFive.Search;
using Display.Models.Api.OneOneFive.Setting;
using Display.Models.Api.OneOneFive.Upload;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;
using Display.Models.Enums;
using Display.Models.Enums.OneOneFive;
using Display.Models.Vo;
using Display.Models.Vo.OneOneFive;
using Display.Models.Vo.Progress;
using Display.Views.Pages.Settings.Account;
using FileInfo = System.IO.FileInfo;

namespace Display.Providers;

internal class WebApi
{
    public static UserInfoResult UserInfoResult;
    public static QRCodeInfoResult QrCodeInfoResult;
    private static UploadInfoResult _uploadInfoResult;

    public HttpClient Client;
    private Result _result;

    private static long NowDate => DateTimeOffset.Now.ToUnixTimeSeconds();

    private static WebApi _webApi;

    public static WebApi GlobalWebApi => _webApi ??= new WebApi();

    private WebApi(bool useCookie = true)
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
        var headers = new Dictionary<string, string> { { "user-agent", DbNetworkHelper.DownUserAgent } };

        var cookie = AppSettings._115_Cookie;
        //cookie不为空且可用
        if (!string.IsNullOrEmpty(cookie))
        {
            headers["Cookie"] = cookie;
        }

        Client = NetworkHelper.CreateClient(headers);


    }

    /// <summary>
    /// 更新cookie
    /// </summary>
    /// <param name="cookie"></param>
    public void ResetCookie(string cookie)
    {
        Client.DefaultRequestHeaders.Remove("Cookie");
        Client.DefaultRequestHeaders.Add("Cookie", cookie);
    }

    private static Windows.Web.Http.HttpClient _windowWebHttpClient;
    public static Windows.Web.Http.HttpClient SingleVideoWindowWebHttpClient
    {
        get
        {
            _windowWebHttpClient ??= CreateWindowWebHttpClient();

            return _windowWebHttpClient;
        }
    }

    public static Windows.Web.Http.HttpClient CreateWindowWebHttpClient()
    {
        const string referer = "https://115.com/?cid=0&offset=0&tab=&mode=wangpan";
        var windowWebHttpClient = new Windows.Web.Http.HttpClient();
        windowWebHttpClient.DefaultRequestHeaders.Add("Referer", referer);
        windowWebHttpClient.DefaultRequestHeaders.Add("User-Agent", DbNetworkHelper.DownUserAgent);

        return windowWebHttpClient;
    }

    /// <summary>
    /// 检查登录状态
    /// </summary>
    /// <returns>true - 登录，false - 未登录</returns>
    public async Task<bool> UpdateLoginInfoAsync()
    {
        UserInfoResult = await Client.SendAsync<UserInfoResult>(HttpMethod.Get,
            $"https://my.115.com/?ct=ajax&ac=nav&_={NowDate}");

        return UserInfoResult?.State == true;
    }

    /// <summary>
    /// 检查是否为隐藏模式
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsHiddenModel()
    {
        const string url = "https://115.com/?ac=setting&even=saveedit&is_wl_tpl=1";
        var values = new Dictionary<string, string>
        {
            { "last_file_type", " folder"},
            {"last_file_id"," 1865386445801900763" }
        };
        var content = new FormUrlEncodedContent(values);

        var driveSetting = await Client.SendAsync<_115Setting>(HttpMethod.Post, url, content);

        var result = false;
        if (driveSetting?.Info.Show == "1")
        {
            result = true;
        }
        else
        {
        }

        return result;
    }

    /// <summary>
    /// 导入CidList获取到的所有信息到数据库
    /// </summary>
    /// <param name="fileInfos"></param>
    /// <param name="token"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task GetAllFileInfoToDataAccess(List<FilesInfo> fileInfos, CancellationToken token, IProgress<GetFileProgressIProgress> progress = null)
    {
        var getFilesProgressInfo = new GetFilesProgressInfo();

        // 文件夹，获取文件夹下文件信息后添加
        foreach (var info in fileInfos.Where(x => x.Type == FilesInfo.FileType.Folder))
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            getFilesProgressInfo = await TryAddFolderToDataAccess(token, progress, (long)info.Id!, getFilesProgressInfo);
        }

        // 文件，直接添加
        foreach (var info in fileInfos.Where(x => x.Type == FilesInfo.FileType.File))
        {

            if (token.IsCancellationRequested)
            {
                return;
            }

            await DataAccess.Add.AddFilesInfoAsync(info.Datum);

            getFilesProgressInfo!.FilesCount++;

            progress?.Report(new GetFileProgressIProgress
            { getFilesProgressInfo = getFilesProgressInfo });
        }

        progress?.Report(token.IsCancellationRequested
            ? new GetFileProgressIProgress { status = ProgressStatus.cancel }
            // 完成
            : new GetFileProgressIProgress { status = ProgressStatus.done });
    }

    private async Task<GetFilesProgressInfo> TryAddFolderToDataAccess(CancellationToken token, IProgress<GetFileProgressIProgress> progress, long cid, GetFilesProgressInfo getFilesProgressInfo)
    {
        var lastDate = NowDate;

        await Task.Delay(1000, token);
        // 一开始只有cid，先获取cid的属性
        var cidCategory = await GetFolderCategory(cid);

        //正常不为空，为空说明有异常
        if (cidCategory == null)
        {
            progress?.Report(new GetFileProgressIProgress { getFilesProgressInfo = getFilesProgressInfo, status = ProgressStatus.error, sendCountPerMinutes = 1 });

            // 退出
            return null;
        }

        // 获取上一次已添加文件夹的pid（如果存在，且修改时间不变；不存在的默认值为string.empty）
        var pid = DataAccess.Get.GetLatestFolderPid(cidCategory.pick_code, cidCategory.utime);

        // 该文件已存在数据库里，且修改时间不变
        if (pid >= 0 && StaticData.IsJumpExistsFolder)
        {
            //如果修改时间未变，但移动了位置
            if (pid == cidCategory.paths.Last().FileId)
            {
                await DataAccess.Add.AddFilesInfoAsync(FileCategory.ConvertFolderToDatum(cidCategory, cid));
            }

            //统计上下级文件夹所含文件的数量

            //文件数量
            getFilesProgressInfo!.FilesCount += cidCategory.count;

            //文件夹数量
            getFilesProgressInfo!.FolderCount += cidCategory.folder_count;

            var cpm = 60 / (NowDate - lastDate);

            progress?.Report(new GetFileProgressIProgress
            { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });
        }
        //之前未添加或修改时间已改变
        else
        {
            //获取当前文件夹下所有文件信息和文件夹信息（从数据库或者网络）
            getFilesProgressInfo = await TraverseAllFileInfo(cid, getFilesProgressInfo, token, progress);

            var addToDataAccessList = getFilesProgressInfo.AddToDataAccessList;

            //删除后重新添加
            DataAccess.Delete.DeleteAllDirectoryAndFiles_InFilesInfoTable(cid);

            if (addToDataAccessList.Count > 0)
            {
                //需要添加进数据库的Datum
                foreach (var item in addToDataAccessList)
                {
                    await DataAccess.Add.AddFilesInfoAsync(item);
                }
            }

            //不添加有错误的目录进数据库（添加数据库时会跳过已经添加过的目录，对于出现错误的目录不添加方便后续重新添加）
            if (getFilesProgressInfo.FailCid.Count == 0)
            {
                await DataAccess.Add.AddFilesInfoAsync(FileCategory.ConvertFolderToDatum(cidCategory, cid));
            }
        }

        return getFilesProgressInfo;
    }

    /// <summary>
    /// 获取所有文件信息
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="getFilesProgressInfo"></param>
    /// <param name="token"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task<GetFilesProgressInfo> TraverseAllFileInfo(long? cid, GetFilesProgressInfo getFilesProgressInfo, CancellationToken token, IProgress<GetFileProgressIProgress> progress = null)
    {
        if (token.IsCancellationRequested) return getFilesProgressInfo;

        var lastDate = NowDate;

        await Task.Delay(1000, token);

        //查询下一级文件信息
        var webFileInfo = await GetFileAsync(cid, loadAll: true);

        if (webFileInfo.State)
        {
            //  下级目录为空
            if (webFileInfo.Data is null)
            {
                return getFilesProgressInfo;
            }

            foreach (var item in webFileInfo.Data)
            {
                if (token.IsCancellationRequested)
                {
                    return getFilesProgressInfo;
                }

                //文件夹
                if (item.Fid == default)
                {
                    getFilesProgressInfo.FolderCount++;

                    //先添加文件夹后添加文件，方便删除已有文件夹中的文件
                    getFilesProgressInfo.AddToDataAccessList.Add(item);

                    //查询数据库是否存在
                    if (DataAccess.Get.GetLatestFolderPid(item.PickCode, item.TimeEdit) >= 0 && StaticData.IsJumpExistsFolder)
                    {
                        //统计下级文件夹所含文件的数量
                        //通过数据库获取
                        var datumList = DataAccess.Get.GetAllFilesTraverse(item.Cid);

                        if (datumList == null)
                        {
                            continue;
                        }

                        getFilesProgressInfo.AddToDataAccessList.AddRange(datumList);

                        //文件数量
                        getFilesProgressInfo.FilesCount += datumList.Count;

                        var cpm = 60 / (NowDate - lastDate);
                        progress?.Report(new GetFileProgressIProgress() { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });
                    }
                    else
                    {
                        getFilesProgressInfo = await TraverseAllFileInfo(item.Cid, getFilesProgressInfo, token, progress);
                    }
                }
                //文件
                else
                {
                    getFilesProgressInfo.FilesCount++;

                    var cpm = 60 / (NowDate - lastDate);
                    progress?.Report(new GetFileProgressIProgress { getFilesProgressInfo = getFilesProgressInfo, sendCountPerMinutes = cpm });

                    getFilesProgressInfo.AddToDataAccessList.Add(item);
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
    /// <param name="cid"></param>
    /// <param name="orderBy"></param>
    /// <param name="asc"></param>
    /// <returns></returns>
    public async Task ChangedShowType(long cid, OrderBy orderBy = OrderBy.UserProduceTime, int asc = 0)
    {
        const string url = "https://webapi.115.com/files/order";
        var values = new Dictionary<string, string>
        {
            { "user_order", orderBy.GetDescription()},
            {"file_id", cid.ToString() },
            {"user_asc",asc.ToString() },
            {"fc_mix","0" },
        };
        var content = new FormUrlEncodedContent(values);

        var _ = await Client.SendAsync<string>(HttpMethod.Post, url, content);

    }

    /// <summary>
    /// 删除115文件
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="fids"></param>
    /// <param name="ignoreWarn"></param>
    /// <returns></returns>
    public async Task<bool> DeleteFiles(long cid, long[] fids, int ignoreWarn = 1)
    {
        return await DeleteFiles(Client, cid, fids, ignoreWarn);
    }

    public static async Task<bool> DeleteFiles(HttpClient client, long cid, long[] fids, int ignoreWarn = 1)
    {
        const string url = "https://webapi.115.com/rb/delete";

        var values = new Dictionary<string, string>
        {
            { "pid", cid.ToString()},
        };

        for (var i = 0; i < fids.Length; i++)
        {
            values.Add($"fid[{i}]", fids[i].ToString());
        }

        values.Add("ignore_warn", ignoreWarn.ToString());

        var content = new FormUrlEncodedContent(values);

        var result = await client.SendAsync<DeleteFilesResult>(HttpMethod.Post, url, content);

        return result.State;

    }

    /// <summary>
    /// 从115回收站恢复文件
    /// </summary>
    /// <param name="rids"></param>
    /// <returns></returns>
    public async Task RevertFiles(List<string> rids)
    {
        const string url = "https://webapi.115.com/rb/revert";
        Dictionary<string, string> values = [];

        for (var i = 0; i < rids.Count; i++)
        {
            values.Add($"rid[{i}]", rids[i]);
        }

        var content = new FormUrlEncodedContent(values);

        var _ = await Client.SendAsync<string>(HttpMethod.Post, url, content);
    }

    public async Task<Tuple<bool, string>> CreateTorrentOfflineDown(long cid, string torrentPath)
    {
        var torrentFile = new FileInfo(torrentPath);
        if (torrentFile.Length > 2097152)
        {
            // TODO: 转换为ed2k后添加
            Debug.WriteLine("不支持添加超2M的种子任务");
            return new Tuple<bool, string>(false, "不支持添加超2M的torrent任务");
        }

        // 计算torrent文件的sha1
        var sha1 = HashHelper.ComputeSha1ByPath(torrentPath);

        // 检查网盘中是否有该torrent
        var info = await SearchInfoBySha1(sha1);

        // 获取上传信息
        var uploadInfo = await GetUploadInfo();

        // 无该信息
        if (info is not { State: true })
        {
            // 获取种子文件应该存放的目录cid
            var torrentCidInfo = await GetTorrentCid();

            if (torrentCidInfo != null && uploadInfo != null)
            {
                await FileUploadService.SimpleUpload(torrentPath, torrentCidInfo.Cid, uploadInfo.UserId, uploadInfo.UserKey);
            }

            // 再次检查网盘中是否有该torrent
            info = await SearchInfoBySha1(sha1);
        }

        // 上传后仍然为空
        if (info is not { State: true } || uploadInfo == null) return new Tuple<bool, string>(false, "上传torrent失败");

        var offlineSpaceInfo = await GetOfflineSpaceInfo(uploadInfo.UserKey, uploadInfo.UserId);

        // 获取种子信息
        var torrentInfo = await GetTorrentInfo(info.Data.PickCode, info.Sha1, info.UserId.ToString(), offlineSpaceInfo.Sign,
            offlineSpaceInfo.Time.ToString());

        if (torrentInfo == null) return new Tuple<bool, string>(false, "获取torrent信息失败");

        // 最小大小,10 MB
        const long minSize = 10485760;
        List<int> selectedIndexList = [];
        for (var i = 0; i < torrentInfo.TorrentFileListWeb.Length; i++)
        {
            var fileInfo = torrentInfo.TorrentFileListWeb[i];

            if (fileInfo.Size > minSize)
            {
                selectedIndexList.Add(i);
            }
        }

        // 添加任务
        var taskInfo = await AddTaskBt(torrentInfo.InfoHash, string.Join(",", selectedIndexList), cid, uploadInfo.UserId.ToString(), offlineSpaceInfo.Sign, offlineSpaceInfo.Time.ToString());

        if (taskInfo is not { State: true }) return new Tuple<bool, string>(false, "添加torrent任务失败");

        Debug.WriteLine("添加任务成功");
        return new Tuple<bool, string>(true, "torrent任务添加成功");
    }

    private async Task<AddTaskBtResult> AddTaskBt(string infoHash, string wanted, long cid, string uid, string sign, string time, string savePath = "")
    {
        const string url = "https://115.com/web/lixian/?ct=lixian&ac=add_task_bt";
        var values = new Dictionary<string, string>
        {
            { "info_hash", infoHash},
            {"wanted",wanted },
            {"savepath",savePath },
            {"wp_path_id",cid.ToString() },
            {"uid",uid },
            {"sign",sign },
            {"time",time }
        };

        var content = new FormUrlEncodedContent(values);

        return await Client.SendAsync<AddTaskBtResult>(HttpMethod.Post,
            url, content);
    }

    private async Task<TorrentInfoResult> GetTorrentInfo(string pickCode, string sha1, string uid, string sign, string time)
    {
        const string url = "https://115.com/web/lixian/?ct=lixian&ac=torrent";
        var values = new Dictionary<string, string>
        {
            { "pickcode", pickCode},
            {"sha1",sha1 },
            {"uid",uid },
            {"sign",sign },
            {"time",time },
        };

        var content = new FormUrlEncodedContent(values);

        return await Client.SendAsync<TorrentInfoResult>(HttpMethod.Post, url, content);

    }

    private async Task<TorrentCidResult> GetTorrentCid()
    {
        const string url = "http://115.com/?ct=lixian&ac=get_id&torrent=1";

        return await Client.SendAsync<TorrentCidResult>(HttpMethod.Get, url);
    }

    private async Task<ShaSearchResult> SearchInfoBySha1(string sha1)
    {
        return await Client.SendAsync<ShaSearchResult>(HttpMethod.Get, $"https://webapi.115.com/files/shasearch?sha1={sha1}&_={NowDate}");
    }

    /// <summary>
    /// 请求创建文件夹
    /// </summary>
    /// <param name="cid">在当前pid下创建文件夹</param>
    /// <param name="name">文件夹名称</param>
    /// <returns>目标cid,创建失败则为null</returns>
    public async Task<MakeDirRequest> RequestMakeDir(long cid, string name)
    {
        const string url = "https://webapi.115.com/files/add";

        var values = new Dictionary<string, string>
        {
            { "pid", cid.ToString()},
            {"cname",name }
        };
        var content = new FormUrlEncodedContent(values);

        return await Client.SendAsync<MakeDirRequest>(HttpMethod.Post, url, content);
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<MakeDirRequest> MoveFiles(long cid, long?[] ids)
    {
        ids = ids.Where(id => id != null).ToArray();

        if (ids is not { Length: > 0 }) return null;

        const string url = "https://webapi.115.com/files/move";

        var values = new Dictionary<string, string>
        {
            { "pid", cid.ToString()},
            { "move_proid", $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_-{new Random().Next(1,99)}_0"}
        };

        for (var i = 0; i < ids.Length; i++)
        {
            values.Add($"fid[{i}]", ids[i].ToString());
        }

        var content = new FormUrlEncodedContent(values);

        return await Client.SendAsync<MakeDirRequest>(HttpMethod.Post, url, content);
    }

    /// <summary>
    /// 重命名文件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public async Task<RenameRequest> RenameFile(long? id, string newName)
    {
        if (id == null) return null;

        const string url = "https://webapi.115.com/files/batch_rename";

        var values = new Dictionary<string, string>
        {
            { $"files_new_name[{id}]", newName},
        };

        var content = new FormUrlEncodedContent(values);

        return await Client.SendAsync<RenameRequest>(HttpMethod.Post, url, content);
    }

    public async Task<bool> RenameForce(FilesInfo data, string newName)
    {
        if (data is not { NoId: false }) return false;

        var uploadInfo = await GetUploadInfo();
        var userId = uploadInfo.UserId;
        var userKey = uploadInfo.UserKey;

        // 通过秒传上传一份
        var result = await FileUploadService.UploadAgainByFastUpload(FileUploadService.Client, data.PickCode, data.Cid, data.Sha1, newName, data.Size, userId, userKey);
        if (result == null || string.IsNullOrEmpty(result.PickCode)) return false;

        // 删除原文件
        return await DeleteFiles(FileUploadService.Client, data.Cid, new[] { (long)data.Id! });
    }
    public enum OrderBy
    {
        [Description("file_name")]
        FileName,

        [Description("file_size")]
        FileSize,

        [Description("user_ptime")]
        UserProduceTime
    }

    /// <summary>
    /// 获取文件信息，获取data可能要访问两次（web接口1和web接口2）
    /// 后续考虑使用 android/desktop/tv 端接口访问
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="limit"></param>
    /// <param name="offset"></param>
    /// <param name="useApi2"></param>
    /// <param name="loadAll"></param>
    /// <param name="orderBy"></param>
    /// <param name="asc"></param>
    /// <param name="isOnlyFolder"></param>
    /// <returns></returns>
    public async Task<WebFileInfo> GetFileAsync(long? cid, int limit = 40, int offset = 0, bool useApi2 = false,
        bool loadAll = false, string orderBy = "user_ptime", int asc = 0, bool isOnlyFolder = false)
    {
        var url = !useApi2
            ? $"https://webapi.115.com/files?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json"
            :
            //旧接口只有t，没有修改时间（te），创建时间（tp）
            $"https://aps.115.com/natsort/files.php?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json&fc_mix=0&type=&star=&is_share=&suffix=&custom_order=";

        if (isOnlyFolder) url += "&nf=1";

        var webFileInfoResult = await Client.SendAsync<WebFileInfo>(HttpMethod.Get, url);

        if (webFileInfoResult == null) return null;

        //te，tp简单用t替换，接口2没有te,tp
        if (!string.IsNullOrEmpty(webFileInfoResult.Error))
        {
            Toast.TryToast("115",webFileInfoResult.Error);
            return null;
        }

        webFileInfoResult.Data?.ForEach(item =>
        {
            int dateInt;
            //item.t 可能是 "1658999027" 也可能是 "2022-07-28 17:03"

            //"1658999027"
            if (item.Time.IsNumber())
            {
                dateInt = int.Parse(item.Time);
                item.Time = DateHelper.ConvertInt32ToDateTime(dateInt);

            }
            //"2022-07-28 17:03"
            else
            {
                dateInt = DateHelper.ConvertDateTimeToInt32(item.Time);
            }

            if (useApi2)
            {
                item.TimeEdit = item.TimeProduce = dateInt;
            }
        });



        //接口1出错，使用接口2
        if (webFileInfoResult.ErrNo == 20130827 && useApi2 == false)
        {
            webFileInfoResult = await GetFileAsync(cid, limit, offset, true, loadAll, webFileInfoResult.Order, webFileInfoResult.IsAsc, isOnlyFolder: isOnlyFolder);
        }
        //需要加载全部，但未加载全部
        else if (loadAll && webFileInfoResult.Count > limit)
        {
            webFileInfoResult = await GetFileAsync(cid, webFileInfoResult.Count, offset, useApi2, true, orderBy, asc);
        }

        return webFileInfoResult;
    }

    /// <summary>
    /// 获取文件夹属性（含大小和数量）
    /// </summary>
    /// <param name="cid"></param>
    /// <returns></returns>
    public async Task<FileCategory> GetFolderCategory(long cid)
    {
        if (cid == 0)
        {
            return new FileCategory { file_name = "根目录" };
        }

        return await Client.SendAsync<FileCategory>(HttpMethod.Get, $"https://webapi.115.com/category/get?cid={cid}");
    }

    /// <summary>
    /// 检查二维码登录验证状态，若登录成功则存储cookie;
    /// </summary>
    /// <returns></returns>
    public async Task GetNetworkVerifyTokenAsync()
    {
        const string url = "https://passportapi.115.com/app/1.0/web/1.0/login/qrcode";

        var values = new Dictionary<string, string>
        {
            { "account", QrCodeInfoResult.data.Uid }
        };
        var content = new FormUrlEncodedContent(values);

        _result = await Client.SendAsync<Result>(HttpMethod.Post, url, content);

        if (_result.State != 1) return;

        //存储cookie至本地
        var cookieList = _result.Data.Cookie.GetType().GetProperties().Select(item
            => $"{item.Name}={item.GetValue(_result.Data.Cookie)}").ToList();

        var cookie = string.Join(";", cookieList);
        AppSettings._115_Cookie = cookie;
        Client.DefaultRequestHeaders.Remove("Cookie");
        Client.DefaultRequestHeaders.Add("Cookie", cookie);
    }

    /// <summary>
    /// 检查二维码扫描状态
    /// </summary>
    /// <returns></returns>
    public async Task<QRCodeStatusResult> GetQrCodeStatusAsync()
    {
        var url = $"https://qrcodeapi.115.com/get/status/?uid={QrCodeInfoResult.data.Uid}&time={QrCodeInfoResult.data.Time}&sign={QrCodeInfoResult.data.Sign}&_={NowDate}";


        return await Client.SendAsync<QRCodeStatusResult>(HttpMethod.Get, url);
    }

    /// <summary>
    /// 获取登录二维码信息
    /// </summary>
    /// <returns></returns>
    public async Task<QRCodeInfoResult> GetQrCodeInfo()
    {
        const string url = "https://qrcodeapi.115.com/api/1.0/web/1.0/token";

        QrCodeInfoResult = await Client.SendAsync<QRCodeInfoResult>(HttpMethod.Get, url);
        return QrCodeInfoResult;
    }

    public enum DownType { _115, Bc, Aria2 };
    public async Task<bool> RequestDown(List<Datum> videoInfoList, DownType downType = DownType._115, string savePath = null, string topFolderName = null)
    {
        var success = downType switch
        {
            //115只支持文件
            DownType._115 => await RequestDownBy115Browser(videoInfoList),

            //BitComet支持文件和文件夹
            DownType.Bc => await RequestDownByBitComet(videoInfoList, DbNetworkHelper.DownUserAgent,
                savePath: savePath, topFolderName: topFolderName),

            //Aria2也支持文件和文件夹
            DownType.Aria2 => await RequestDownByAria2(videoInfoList, DbNetworkHelper.DownUserAgent,
                savePath: savePath, topFolderName: topFolderName),
            _ => false
        };

        return success;
    }

    /// <summary>
    /// 请求115浏览器下载，似乎不能用了
    /// </summary>
    /// <param name="videoInfoList"></param>
    /// <returns></returns>
    async Task<bool> RequestDownBy115Browser(List<Datum> videoInfoList)
    {
        var downRequest = new Browser115Request
        {
            //UID
            Uid = videoInfoList[0].Uid
        };

        //KEY
        if (QrCodeInfoResult == null)
        {
            await GetQrCodeInfo();
        }
        downRequest.Key = QrCodeInfoResult?.data.Uid;

        //PARAM
        downRequest.Param = new ParamRequest
        {
            Count = videoInfoList.Count,
            List = [],
            RefUrl = $"https://115.com/?cid={videoInfoList[0].Cid}&offset=0&mode=wangpan"
        };

        foreach (var videoInfo in videoInfoList)
        {
            var isDir = videoInfo.Uid == 0;
            downRequest.Param.List.Add(new DownRequest()
            { Name = videoInfo.Name, PickCode = videoInfo.PickCode, IsDir = isDir });
        }

        var url = string.Empty;
        var jsonString = JsonConvert.SerializeObject(downRequest);

        var jObject = JObject.Parse(jsonString);
        var nameValues = jObject
            .Properties()
            .Select(x => $"{x.Name}={WebUtility.UrlEncode(x.Value.ToString().Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty))}");

        url += "browser115://download?" + string.Join("&", nameValues);

        // The URI to launch
        var uriDown = new Uri(url);

        // Set the option to show a warning
        var options = new Windows.System.LauncherOptions
        {
            DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseLess
        };

        // Launch the URI
        return await Windows.System.Launcher.LaunchUriAsync(uriDown, options);
    }

    /// <summary>
    /// 请求比特彗星下载
    /// </summary>
    /// <param name="videoInfoList"></param>
    /// <param name="ua"></param>
    /// <param name="savePath"></param>
    /// <param name="topFolderName"></param>
    /// <returns></returns>
    private async Task<bool> RequestDownByBitComet(List<Datum> videoInfoList, string ua, string savePath, string topFolderName = null)
    {
        var success = true;

        var bitCometSettings = AppSettings.BitCometSettings;

        if (bitCometSettings == null)
            return false;

        var baseUrl = bitCometSettings.ApiUrl;

        var handler = new HttpClientHandler()
        {
            UseDefaultCredentials = true,
            Credentials = new NetworkCredential(bitCometSettings.UserName, bitCometSettings.Password),
        };

        var client = new HttpClient(handler);

        //存储路径
        savePath ??= AppSettings.BitCometSavePath;

        //应用设置中没有，则从比特彗星的设置中读取
        if (string.IsNullOrEmpty(savePath))
            savePath = await GetBitCometDefaultSavePath(client, baseUrl);

        if (topFolderName != null)
            savePath = Path.Combine(savePath, topFolderName);

        foreach (var datum in videoInfoList)
        {
            //文件夹
            if (datum.Fid is null)
            {
                var newSavePath = Path.Combine(savePath, datum.Name);
                //遍历文件夹并下载
                GetAllFilesTraverseAndDownByBitComet(client, baseUrl, datum, ua, newSavePath);
            }
            //文件
            else
            {
                var isOk = await pushOneFileDownRequestToBitComet(client, baseUrl, datum, ua, savePath);

                if (!isOk)
                    success = false;
            }
        }
        client.Dispose();
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

        return await Client.SendAsync<OfflineDownPathRequest>(HttpMethod.Get, url);
    }

    public async Task<SearchInfo> GetSearchResult(long cid, string value)
    {
        var url = $"https://webapi.115.com/files/search?offset=0&limit=30&search_value={value}&date=&aid=1&cid={cid}&pick_code=&type=&count_folders=1&source=&format=json";

        return await Client.SendAsync<SearchInfo>(HttpMethod.Get, url);
    }

    public async Task<UploadInfoResult> GetUploadInfo(bool isUpdate = false)
    {
        if (!isUpdate && _uploadInfoResult != null) return _uploadInfoResult;

        const string url = "https://proapi.115.com/app/uploadinfo";
        _uploadInfoResult = await Client.SendAsync<UploadInfoResult>(HttpMethod.Get, url);
        return _uploadInfoResult;
    }

    public async Task<OfflineSpaceInfo> GetOfflineSpaceInfo(string userKey, int userId)
    {
        var tm = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        var url = $"https://115.com/?ct=offline&ac=space&sign={userKey}&time={tm / 1000}&user_id={userId}&_={tm}";

        return await Client.SendAsync<OfflineSpaceInfo>(HttpMethod.Get, url);
    }

    public async Task<AddTaskUrlInfo> AddTaskUrl(List<string> linkList, long wpPathId, int uid, string sign, long time)
    {
        if (linkList.Count == 0) return null;

        var content = new MultipartFormDataContent
        {
            { new StringContent(""), "savepath" },
            { new StringContent(wpPathId.ToString()), "wp_path_id" },
            { new StringContent(uid.ToString()), "uid" },
            { new StringContent(sign), "sign"},
            { new StringContent(time.ToString()), "time"}
        };

        string url;
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

        return await Client.SendAsync<AddTaskUrlInfo>(HttpMethod.Get, url, content);
    }


    /// <summary>
    /// 请求Aria2下载
    /// </summary>
    /// <param name="videoInfoList"></param>
    /// <param name="ua"></param>
    /// <param name="savePath"></param>
    /// <param name="topFolderName"></param>
    /// <returns></returns>
    async Task<bool> RequestDownByAria2(List<Datum> videoInfoList, string ua, string savePath, string topFolderName = null)
    {
        var aria2Settings = AppSettings.Aria2Settings;

        if (aria2Settings == null) return false;

        //存储路径
        if (string.IsNullOrEmpty(savePath)) savePath = AppSettings.BitCometSavePath;

        //应用设置中没有，则从Aria2的设置中读取
        if (string.IsNullOrEmpty(savePath))
            savePath = await GetAria2DefaultSavePath(aria2Settings.ApiUrl, aria2Settings.Password);

        if (topFolderName != null) savePath = Path.Combine(savePath, topFolderName);

        return await GetAllFilesTraverseAndDownByAria2(videoInfoList, aria2Settings.ApiUrl, aria2Settings.Password, savePath, ua);
    }

    public async Task<bool> GetAllFilesTraverseAndDownByAria2(List<Datum> videoInfoList, string apiUrl, string password, string savePath, string ua)
    {
        var success = true;

        Dictionary<string, string> fileList = new();

        foreach (var datum in videoInfoList)
        {
            //文件夹
            if (datum.Fid == null)
            {
                //获取该文件夹下的文件和文件夹
                var webFileInfo = await GetFileAsync(datum.Cid, loadAll: true);
                if (webFileInfo.Count == 0)
                {
                    success = false;
                    continue;
                }

                var newSavePath = Path.Combine(savePath, datum.Name);
                var isOk = await GetAllFilesTraverseAndDownByAria2(webFileInfo.Data.ToList(), apiUrl, password, newSavePath, ua);


                if (!isOk)
                    success = false;
            }

            //文件
            else
            {
                //一般只有一个
                var downUrlList = await GetDownUrl(datum.PickCode, ua);
                if (downUrlList.Count == 0)
                {
                    success = false;
                    continue;
                }

                ////用来标记aria2的任务id，如果没有就用时间戳代替
                //string aria2TaskId = datum.pc != null? datum.pc : DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

                fileList.Add(datum.PickCode, downUrlList.FirstOrDefault().Value);
            }
        }

        //文件
        if (fileList.Count > 0)
        {
            foreach (var file in fileList)
            {

                var isOk = await PushDownRequestToAria2(apiUrl, password, new List<string>() { file.Value }, ua, savePath, file.Key);

                if (!isOk)
                    success = false;
            }
        }

        return success;
    }

    /// <summary>
    /// 链接只能一个一个添加，添加多个视为为同一文件的不同源
    /// </summary>
    /// <param name="apiUrl"></param>
    /// <param name="password"></param>
    /// <param name="urls"></param>
    /// <param name="ua"></param>
    /// <param name="savePath"></param>
    /// <param name="sha1"></param>
    /// <returns></returns>
    private static async Task<bool> PushDownRequestToAria2(string apiUrl, string password, IEnumerable<string> urls, string ua, string savePath, string sha1 = null)
    {

        savePath = savePath.Replace("\\", "/");

        var gid = sha1 ?? DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        string myContent = "{\"jsonrpc\":\"2.0\"," +
                           "\"method\": \"aria2.addUri\"," +
                           "\"id\": \"" + gid + "\"," +
                           "\"params\": [ \"" + password + "\"," +
                           "[\"" + string.Join("\",\"", urls) + "\"]," +
                           "{\"referer\": \"https://115.com/?cid=0&offset=0&tab=&mode=wangpan\"," +
                           "\"header\": [\"User-Agent: " + ua + "\"]," +
                           "\"dir\": \"" + savePath + "\"}]}";

        var client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var content = new StringContent(myContent, Encoding.UTF8, "application/json");

        var success = false;
        try
        {
            var rep = await client.PostAsync(apiUrl, content);

            if (rep.IsSuccessStatusCode)
            {
                var strResult = await rep.Content.ReadAsStringAsync();

                Debug.WriteLine(strResult);

                if (!string.IsNullOrWhiteSpace(strResult))
                    success = true;
            }
        }
        catch
        {
            success = false;
        }
        finally
        {
            client.Dispose();
        }

        return success;
    }

    private async Task<bool> pushOneFileDownRequestToBitComet(HttpClient client, string baseUrl, Datum datum, string ua, string savePath)
    {
        var urlList = await GetDownUrl(datum.PickCode, ua);

        if (urlList.Count == 0)
            return false;

        return await PushDownRequestToBitComet(client,
            baseUrl,
            urlList.First().Value,
            savePath,
            datum.Name,
            $"https://115.com/?cid={datum.Cid}=0&tab=download&mode=wangpan",
            ua);
    }

    public async void GetAllFilesTraverseAndDownByBitComet(HttpClient client, string baseUrl, Datum datum, string ua, string savePath)
    {
        //获取该文件夹下的文件和文件夹
        var webFileInfo = await GetFileAsync(datum.Cid, loadAll: true);

        foreach (var data in webFileInfo.Data)
        {
            //文件夹
            if (data.Fid == null)
            {
                var newSavePath = Path.Combine(savePath, data.Name);
                GetAllFilesTraverseAndDownByBitComet(client, baseUrl, data, ua, newSavePath);

                //延迟1s;
                await Task.Delay(1000);
            }
            //文件
            else
            {
                await pushOneFileDownRequestToBitComet(client, baseUrl, data, ua, savePath);
            }
        }
    }



    /// <summary>
    /// 向比特彗星发送下载请求
    /// </summary>
    /// <param name="client">带user和passwd的HttpClient</param>
    /// <param name="baseUrl">比特彗星接口地址</param>
    /// <param name="downUrl">文件下载地址</param>
    /// <param name="savePath">文件保存路径</param>
    /// <param name="filename">文件名称</param>
    /// <param name="referrer">下载需要的referrer</param>
    /// <param name="userAgent">下载需要的user_agent</param>
    /// <param name="cookie">个别需要的Cookie</param>
    /// <returns></returns>
    private static async Task<bool> PushDownRequestToBitComet(HttpClient client, string baseUrl, string downUrl, string savePath, string filename = "", string referrer = "", string userAgent = "", string cookie = "")
    {
        var values = new Dictionary<string, string>
        {
            { "url", downUrl},
            {"save_path",savePath },
            {"connection","200" },
            {"file_name",filename },
            {"referrer",referrer },
            {"user_agent",userAgent },
            {"cookie",cookie },
            {"mirror_url_list",""}
        };
        var content = new FormUrlEncodedContent(values);

        var response = await client.PostAsync(baseUrl + "/panel/task_add_httpftp_result", content);

        return response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// 获取网页中比特彗星的默认存储地址
    /// </summary>
    /// <param name="client"></param>
    /// <param name="baseUrl"></param>
    /// <returns></returns>
    private async Task<string> GetBitCometDefaultSavePath(HttpClient client, string baseUrl)
    {
        var response = await client.GetAsync(baseUrl + "/panel/task_add_httpftp");

        if (!response.IsSuccessStatusCode || response.StatusCode != HttpStatusCode.OK) return null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(await response.Content.ReadAsStringAsync());

        var savePathNode = htmlDoc.DocumentNode.SelectSingleNode("//input[@id='save_path']");

        return savePathNode?.GetAttributeValue("value", null);
    }

    private static async Task<string> GetAria2DefaultSavePath(string apiUrl, string password)
    {
        var savePath = string.Empty;

        var myContent = "{\"jsonrpc\":\"2.0\"," +
                        "\"method\": \"aria2.getGlobalOption\"," +
                        "\"id\": " + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "," +
                        "\"params\": [ \"" + password + "\"] }";

        var client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var content = new StringContent(myContent, Encoding.UTF8, "application/json");

        try
        {
            var rep = await client.PostAsync(apiUrl, content);

            if (rep.IsSuccessStatusCode)
            {
                var strResult = await rep.Content.ReadAsStringAsync();

                var aria2GlobalOptionRequest =
                    JsonConvert.DeserializeObject<Aria2GlobalOptionRequest>(strResult);

                savePath = aria2GlobalOptionRequest?.Result?.Dir;
            }
        }
        catch
        {
            Debug.WriteLine("获取网页中比特彗星的默认存储地址时出错");
        }
        finally
        {
            client.Dispose();
        }

        return savePath;
    }

    /// <summary>
    /// 检查Cookie是否可用后更新（Client及localSettings）
    /// </summary>
    /// <returns></returns>
    public async Task<bool> TryRefreshCookie(string cookie)
    {
        bool isSucceed;

        //先保存之前的Cookie，若Cookie无效则恢复原有Cookie
        var haveCookie = Client.DefaultRequestHeaders.TryGetValues("Cookie", out var value);
        if (haveCookie)
        {
            var oldCookie = value.SingleOrDefault();
            ResetCookie(cookie);

            //使用新Cookie登录不成功，恢复默认值
            isSucceed = await UpdateLoginInfoAsync();
            if (isSucceed)
            {
                AppSettings._115_Cookie = cookie;
            }
            else
            {
                ResetCookie(oldCookie);
            }
        }
        else
        {
            ResetCookie(cookie);
            isSucceed = await UpdateLoginInfoAsync();
            AppSettings._115_Cookie = cookie;
        }

        return isSucceed;
    }

    /// <summary>
    /// 退出账号
    /// </summary>
    public async void LogoutAccount()
    {
        if (UserInfoResult == null) return;

        const string url =
            "https://passportapi.115.com/app/1.0/web/1.0/logout/logout/?goto=https%3A%2F%2F115.com%2F";

        //退出账号
        await Client.GetAsync(url);

        //清空账号信息
        QrCodeInfoResult = null;
        UserInfoResult = null;
        DeleteCookie();
    }

    /// <summary>
    /// 删除Cookie
    /// </summary>
    public static void DeleteCookie()
    {
        AppSettings._115_Cookie = null;
    }

    public async Task<Dictionary<string, string>> GetDownUrl(string pickCode, string ua)
    {
        return await GetDownUrl(Client, pickCode, ua, AppSettings.IsRecordDownRequest);
    }

    /// <summary>
    /// 获取下载链接
    /// </summary>
    /// <param name="pickCode"></param>
    /// <param name="ua"></param>
    /// <param name="client"></param>
    /// <param name="isRecodeDownRequest"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, string>> GetDownUrl(HttpClient client, string pickCode, string ua, bool isRecodeDownRequest = true)
    {
        client ??= GlobalWebApi.Client;

        Dictionary<string, string> downUrlList = new();

        var tm = NowDate;

        if (isRecodeDownRequest)
        {
            var downUrlInfo = DataAccess.Get.GetDownHistoryByPcAndUa(pickCode, ua);

            //检查链接是否失效
            if (downUrlInfo != null && tm - downUrlInfo.AddTime < AppSettings.DownUrlOverdueTime)
            {
                downUrlList.Add(downUrlInfo.FileName, downUrlInfo.TrueUrl);
                return downUrlList;
            }
        }

        var src = $"{{\"pickcode\":\"{pickCode}\"}}";
        var (data, keyBytes) = M115Helper.Encode(src, tm);

        var dataString = Encoding.ASCII.GetString(data);
        var dataUrlEncode = HttpUtility.UrlEncode(dataString);

        var url = $"http://proapi.115.com/app/chrome/downurl?t={tm}";
        var body = $"data={dataUrlEncode}";

        var content = new StringContent(body)
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded") }
        };

        var downUrlBase64EncryptInfo = await client.SendAsync<DownUrlBase64EncryptInfo>(HttpMethod.Post, url, content);

        if (downUrlBase64EncryptInfo is { State: true })
        {
            var base64Text = downUrlBase64EncryptInfo.Data;

            var srcBase64 = Convert.FromBase64String(base64Text);

            var rep = M115Helper.Decode(srcBase64, keyBytes);

            //JObject json = JsonConvert.DeserializeObject<JObject>(rep);
            var json = JObject.Parse(rep);

            //如使用的pc是属于文件夹，url为false
            foreach (var children in json)
            {
                var videoInfo = children.Value;

                if (!videoInfo["url"]!.HasValues) continue;

                var downUrl = videoInfo["url"]?["url"]?.ToString();
                downUrlList.Add(videoInfo["file_name"]?.ToString() ?? string.Empty, downUrl);
            }
        }

        //添加下载记录
        if (isRecodeDownRequest && downUrlList.Count != 0)
        {
            DataAccess.Add.AddDownHistory(new DownInfo
            {
                FileName = downUrlList.First().Key,
                TrueUrl = downUrlList.First().Value,
                PickCode = pickCode,
                Ua = ua
            });
        }

        return downUrlList;
    }



    /// <summary>
    /// 解析m3u8内容
    /// </summary>
    /// <param name="m3U8Info"></param>
    /// <returns></returns>
    public async Task<M3U8Info> GetM3U8Content(M3U8Info m3U8Info)
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

        for (var i = 0; i < lineList.Length; i++)
        {
            var lineText = lineList[i].Trim('\r');

            var re = Regex.Match(lineText, @"^#EXTINF:(\d*\.\d*),$");
            if (!re.Success) continue;

            var strUrl = lineList[i + 1];
            var doubleSecond = Convert.ToDouble(re.Groups[1].Value);
            m3U8Info.TsInfoList.Add(new tsInfo() { Second = doubleSecond, Url = strUrl });
        }

        return m3U8Info;
    }

    public async Task<List<M3U8Info>> GetM3U8InfoByPickCode(string pickCode)
    {
        Debug.WriteLine($"获取{pickCode}中的m3u8链接");
        List<M3U8Info> m3U8Infos = [];

        string strResult;
        try
        {
            strResult = await Client.GetStringAsync($"https://v.anxia.com/site/api/video/m3u8/{pickCode}.m3u8");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("获取m3u8链接时发生错误：" + ex.Message);
            return [];
        }

        if (string.IsNullOrWhiteSpace(strResult)) return [];

        var lineList = strResult.Split(['\n']);
        for (var i = 0; i < lineList.Length; i++)
        {
            // TODO: 部分m3u8文件中的Audio与Video分开，目前未针对这种情况进行处理
            var lineText = lineList[i].Trim('\r');

            var re = Regex.Match(lineText, @"BANDWIDTH=(\d*),RESOLUTION=(\w*),.*,NAME=""(\w*)""");
            if (re.Success)
            {
                m3U8Infos.Add(M3U8Info.CreateInstance(re.Groups[3].Value, re.Groups[1].Value, re.Groups[2].Value, lineList[i + 1].Trim('\r')));
            }
        }

        // 检查账号是否异常
        if (m3U8Infos.Count == 0 && strResult.Contains(Constants.Account.AccountAnomalyTip))
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

        Debug.WriteLine("成功获取M3U8链接");

        return m3U8Infos;
    }


    public static Window CreateWindowToVerifyAccount() => VerifyAccountPage.CreateVerifyAccountWindow();


    /// <summary>
    /// 原画播放
    /// </summary>
    /// <param name="playItems"></param>
    /// <param name="playerType"></param>
    /// <param name="xamlRoot"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task PlayVideoWithPlayer(IList<MediaPlayItem> playItems, PlayerType playerType, XamlRoot xamlRoot, IProgress<int> progress = null)
    {
        string savePath;
        string ua;

        //检查播放器设置
        switch (playerType)
        {
            case PlayerType.PotPlayer:
                savePath = AppSettings.PotPlayerExePath;
                ua = DbNetworkHelper.DownUserAgent;
                break;
            case PlayerType.Mpv:
                savePath = AppSettings.MpvExePath;
                ua = DbNetworkHelper.DownUserAgent;
                break;
            case PlayerType.Vlc:
                savePath = AppSettings.VlcExePath;
                ua = DbNetworkHelper.DownUserAgent;
                break;
            case PlayerType.WebView:
            case PlayerType.MediaElement:
            case PlayerType.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
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

        var quality = AppSettings.DefaultPlayQuality;

        //打开一层文件夹并添加可播放文件到List中
        playItems = await MediaPlayItem.OpenFolderThenInsertVideoFileToMediaPlayItem(playItems, GlobalWebApi);

        for (var i = 0; i < playItems.Count; i++)
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
        switch (playerType)
        {
            case PlayerType.PotPlayer:
                PlayVideoHelper.Play115SourceVideoWithPotPlayer(playItems, userAgent: ua, savePath, quality);
                break;
            case PlayerType.Mpv:
                PlayVideoHelper.Play115SourceVideoWithMpv(playItems, userAgent: ua, savePath, quality, false);
                break;
            case PlayerType.Vlc:
                //vlc不支持带“; ”的user-agent
                PlayVideoHelper.Play115SourceVideoWithVlc(playItems, userAgent: ua, savePath, quality, false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(playerType), playerType, null);
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

        var ua = DbNetworkHelper.DownUserAgent;

        //不存在则获取下载链接并下载
        var subUrlList = await GetDownUrl(pickCode, ua);
        if (subUrlList.Count == 0)
        {
            return null;
        }

        var subUrl = subUrlList.First().Value;

        subFile = await DbNetworkHelper.DownloadFile(subUrl, subSavePath, fileName, false, new Dictionary<string, string>
        {
            {"User-Agent", ua }
        });

        return subFile;

    }

}