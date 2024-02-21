using Display.Extensions;
using Display.Helper.Crypto;
using Display.Models.Data;
using Display.Models.Upload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static System.String;

namespace Display.Services.Upload
{
    internal class FileUpload : UploadBase
    {
        private const string AppVer = Const.DefaultSettings.Network._115.UploadAppVersion;

        private static HttpClient _client;
        public static HttpClient Client
        {
            get
            {
                if (_client != null) return _client;

                var headers = new Dictionary<string, string> { { "user-agent", Const.DefaultSettings.Network._115.UploadUserAgent } };

                var cookie = AppSettings._115_Cookie;
                //cookie不为空且可用
                if (!IsNullOrEmpty(cookie))
                {
                    headers.Add("Cookie", cookie);
                }

                _client = GetInfoFromNetwork.CreateClient(headers);

                return _client;
            }
        }

        private readonly FileInfo _fileInfo;
        private readonly string _fileSizeString;
        private readonly FileStream _stream;
        private readonly long _saveFolderCid;
        private readonly CancellationTokenSource _source = new();

        private int _userId;
        private string _userKey;
        private string _totalSha1;
        private bool _isInitSucceed;
        private AliyunOss _aliyunOss;
        private CancellationToken Token => _source.Token;

        public FileUploadResult FileUploadResult;
        private bool IsGetUploadInfo => _userId != 0 && !IsNullOrEmpty(_userKey);

        public override void Dispose()
        {
            _source?.Cancel();
            _source?.Dispose();
            _stream?.Dispose();

            GC.SuppressFinalize(this);
        }

        public override ValueTask DisposeAsync()
        {
            _source?.Cancel();
            _source?.Dispose();
            _aliyunOss?.Dispose();

            var result = _stream?.DisposeAsync() ?? ValueTask.CompletedTask;
            GC.SuppressFinalize(this);

            return result;
        }

        public FileUpload(string path, long cid, int userId = 0, string userKey = "")
        {
            _saveFolderCid = cid;
            _userId = userId;
            _userKey = userKey;

            _fileInfo = new FileInfo(path);
            if (!_fileInfo.Exists) return;

            Position = 0;
            Length = _fileInfo.Length;

            FileUploadResult = new FileUploadResult(_fileInfo.Name)
            {
                FileSize = Length,
                Cid = cid
            };

            _fileSizeString = Length.ToByteSizeString();
            Content = _fileSizeString;

            _stream = _fileInfo.OpenRead();
        }

        public static async Task SimpleUpload(string path, long cid, int userId = 0, string userKey = "")
        {
            if (!File.Exists(path))
            {
                Debug.WriteLine("需要上传的文件不存在");

                return;
            }

            var fileUpload = new FileUpload(path, cid, userId, userKey);

            await fileUpload.Init();

            await fileUpload.Start();
        }


        private async Task<bool> UploadFile()
        {
            var upload115Result = await UploadByFastUpload();

            // 秒传失败或者没获取到需要的参数
            if (State == UploadState.Faulted || upload115Result == null)
            {
                await DisposeAsync();
                return false;
            }

            // 秒传成功
            var uploadResult = State == UploadState.Succeed;
            if (uploadResult)
            {
                FileUploadResult.PickCode = upload115Result.pickcode;
                FileUploadResult.Sha1 = _totalSha1;
                FileUploadResult.Success = true;
                
                await DisposeAsync();
                return true;
            }

            // 秒传未完成但获取到了需要的参数

            //转换callback、callbackVar为base64格式
            upload115Result.callback.callback =
                Convert.ToBase64String(Encoding.Default.GetBytes(upload115Result.callback.callback));
            upload115Result.callback.callback_var =
                Convert.ToBase64String(Encoding.Default.GetBytes(upload115Result.callback.callback_var));

            var ossUploadResult = await UploadByAliyunOss(upload115Result);
            uploadResult = State == UploadState.Succeed;
            if (!uploadResult) return false;

            FileUploadResult.SetFromOssUploadResult(ossUploadResult);
            FileUploadResult.Success = true;
            return true;
        }

        public static Dictionary<string, string> BuildDataForm(long cid, string fileId, string name, long length,int userId, string userKey,out string userIdMd5)
        {
            var target = "U_1_" + cid;
            var sign = UploadEncryptHelper.GetSign(userId, fileId, target, userKey);

            userIdMd5 = HashHelper.ComputeMd5ByContent(userId.ToString()).ToLower();

            return new Dictionary<string, string>
            {
                { "appid", "0" },
                { "appversion", AppVer },
                { "filename", HttpUtility.UrlEncode(name, Encoding.UTF8) },
                { "filesize", length.ToString() },
                { "fileid", fileId },
                { "target", target },
                { "userid", userId.ToString() },
                { "sig", sign },
            };
        }

        public static async Task<FastUploadResult> UploadAgainByFastUpload(HttpClient client,string pickCode, long cid, string totalSha1, string newName, long length, int userId, string userKey, CancellationToken token = default)
        {
            var signKey = string.Empty; 
            var signVal = string.Empty; 
            var dataForm = FileUpload.BuildDataForm(cid, totalSha1, newName, length, userId, userKey, out var userIdMd5);

            var aesKey = UploadKey.Instance.AesKey;
            var aesIv = UploadKey.Instance.AesIv;
            var clientPublicKey = UploadKey.Instance.ClientPublicKey;

            for (var i = 0; i < 3; i++)
            {
                var timeSpan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                dataForm["t"] = timeSpan.ToString();
                dataForm["token"] = UploadEncryptHelper.GetToken(totalSha1, length, signKey, signVal, timeSpan, userId, userIdMd5, AppVer);

                if (!string.IsNullOrEmpty(signKey) && !string.IsNullOrEmpty(signVal))
                {
                    dataForm["sign_key"] = signKey;
                    dataForm["sign_val"] = signVal;
                }

                FastUploadResult fastUploadResult;
                try
                {
                    fastUploadResult = await GetUpload115Result(dataForm, timeSpan, aesKey, aesIv, clientPublicKey, token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"获取Upload115Result时发生错误：{ex.Message}");
                    break;
                }

                if (fastUploadResult == null)
                {
                    Debug.WriteLine("可能是解密出错");
                    continue;
                }

                if (string.IsNullOrEmpty(fastUploadResult.sign_key) || string.IsNullOrEmpty(fastUploadResult.sign_check))
                {
                    // 不能秒传，需要上传
                    if (!string.IsNullOrEmpty(fastUploadResult.Object))
                    {
                        return null;
                    }

                    // 秒传成功
                    if (fastUploadResult.status == 2)
                    {
                        return fastUploadResult;
                    }

                    Debug.WriteLine($"上传时发生错误：{fastUploadResult.statusmsg}");

                    return null;
                }

                signKey = fastUploadResult.sign_key;
                signVal = await GetRangSha1FromInternet(client,pickCode, fastUploadResult.sign_check, token);
            }

            return null;
        }

        private static async Task<string> GetRangSha1FromInternet(HttpClient client,string pickCode, string signCheck, CancellationToken token)
        {
            // 获取下载链接
            var downUrls = await WebApi.GetDownUrl(client, pickCode, GetInfoFromNetwork.DownUserAgent, false);

            if (downUrls is not { Count: > 0 })
            {
                return null;
            }

            var downUrl = downUrls.First().Value;

            // 根据下载链接获取分段Sha1
            return await GetRangSha1FromDownUrl(client, downUrl, signCheck, token);
        }

        private static async Task<string> GetRangSha1FromDownUrl(HttpClient client,string url, string signCheck, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("Range", "bytes=" + signCheck);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode) return null;

            var contentLength = response.Content.Headers.ContentLength;
            if (contentLength == null) return null;

            var stream = await response.Content.ReadAsStreamAsync(token);

            var sha1 = await HashHelper.ComputeSha1ByStream(stream, token);

            return sha1;
        }

        private async Task<FastUploadResult> UploadByFastUpload()
        {
            State = UploadState.FastUploading;
            var signKey = Empty;
            var signVal = Empty;

            var dataForm = BuildDataForm(_saveFolderCid, _totalSha1, _fileInfo.Name, _fileInfo.Length, _userId,_userKey, out var userIdMd5);

            var aesKey = UploadKey.Instance.AesKey;
            var aesIv = UploadKey.Instance.AesIv;
            var clientPublicKey = UploadKey.Instance.ClientPublicKey;

            for (var i = 0; i < 3; i++)
            {
                var timeSpan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                dataForm["t"] = timeSpan.ToString();
                dataForm["token"] = UploadEncryptHelper.GetToken(_totalSha1, _fileInfo.Length, signKey, signVal, timeSpan, _userId, userIdMd5, AppVer);

                if (!IsNullOrEmpty(signKey) && !IsNullOrEmpty(signVal))
                {
                    dataForm["sign_key"] = signKey;
                    dataForm["sign_val"] = signVal;
                }

                FastUploadResult fastUploadResult;
                try
                {
                    fastUploadResult = await GetUpload115Result(dataForm, timeSpan, aesKey, aesIv, clientPublicKey, Token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"获取Upload115Result时发生错误：{ex.Message}");
                    break;
                }

                if (fastUploadResult == null)
                {
                    Debug.WriteLine("可能是解密出错");
                    continue;
                }

                if (IsNullOrEmpty(fastUploadResult.sign_key) || IsNullOrEmpty(fastUploadResult.sign_check))
                {
                    // 不能秒传，需要上传
                    if (!IsNullOrEmpty(fastUploadResult.Object))
                    {
                        // 使用AliyunOss上传
                        return fastUploadResult;
                    }

                    // 秒传成功
                    if (fastUploadResult.status == 2)
                    {
                        Position = _fileInfo.Length;
                        State = UploadState.Succeed;
                        return fastUploadResult;
                    }

                    Debug.WriteLine($"上传时发生错误：{fastUploadResult.statusmsg}");
                    State = UploadState.Faulted;

                    return null;
                }

                signKey = fastUploadResult.sign_key;
                signVal = HashHelper.ComputeSha1RangeByStream(_stream, fastUploadResult.sign_check);
            }

            State = UploadState.Faulted;
            return null;
        }

        private static async Task<FastUploadResult> GetUpload115Result(Dictionary<string, string> dataForm, long timeSpan, byte[] aesKey, byte[] aesIv, byte[] clientPublicKey, CancellationToken token)
        {
            var sendData = UploadEncryptHelper.GetData(dataForm, aesKey, aesIv);
            var kec = UploadEncryptHelper.GetKEc(clientPublicKey, timeSpan);

            var url = $"https://uplb.115.com/4.0/initupload.php?k_ec={kec}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new ByteArrayContent(sendData)
                {
                    Headers = { ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded") }
                }
            };

            var response = await Client.SendAsync(request, token);

            if (!response.IsSuccessStatusCode) return null;

            var contentBytes = await response.Content.ReadAsByteArrayAsync(token);

            return UploadEncryptHelper.DecryptReceiveData<FastUploadResult>(contentBytes, aesKey: aesKey, aesIv: aesIv);
        }

        private async Task<OssUploadResult> UploadByAliyunOss(FastUploadResult fastUploadResult)
        {
            if (_aliyunOss == null)
            {
                _aliyunOss = new AliyunOss(Client, _stream, fastUploadResult, progress: new Progress<long>(
                    p =>
                    {
                        Position = p;
                        Content = $"{p.ToByteSizeString()}/{_fileSizeString}";
                    }));

                _aliyunOss.StateChanged += state =>
                {
                    if (state is not (UploadState.Initializing or UploadState.Initialized))
                    {
                        State = state;
                    }
                };
            }

            await _aliyunOss.Init();

            return await StartUpload();
        }

        private async Task<OssUploadResult> StartUpload()
        {
            var result = await _aliyunOss.Start();

            // 完成或失败后释放
            if (State is UploadState.Succeed or UploadState.Faulted) await DisposeAsync();

            return result;
        }

        /// <summary>
        /// 获取上传所需的用户信息，计算本地Sha1
        /// </summary>
        /// <returns></returns>
        public override async Task Init()
        {
            State = UploadState.Initializing;

            // 获取 _userId 和 _userKey
            if (!IsGetUploadInfo)
            {
                var uploadInfo = await WebApi.GlobalWebApi.GetUploadInfo();
                _userId = uploadInfo.user_id;
                _userKey = uploadInfo.userkey;
            }

            // 计算本地Sha1
            var progress = new Progress<long>(i =>
                {
                    Content = $"{i.ToByteSizeString()}/{_fileSizeString}";
                    //Debug.WriteLine(i);
                }
            );
            _totalSha1 = await HashHelper.ComputeSha1ByStream(_stream, Token,progress);

            if (IsNullOrEmpty(_totalSha1))
            {
                State = UploadState.Canceled;
            }
            else
            {
                State = UploadState.Initialized;
                _isInitSucceed = true;
            }
        }

        public override async Task<bool> Start()
        {
            if (!_isInitSucceed) return false;

            return await UploadFile();
        }

        public override void Pause()
        {
            _aliyunOss?.Pause();
        }

        /// <summary>
        /// 暂停后重新开始上传
        /// </summary>
        /// <returns></returns>
        public async Task Resume()
        {
            await _aliyunOss.Init();

            await StartUpload();
        }

        public override async Task Stop()
        {
            await DisposeAsync();

            State = UploadState.Canceled;
        }
    }


    public class FileUploadResult
    {
        public readonly string Name;
        public long? Id;
        public long FileSize;
        public long Cid;
        public string PickCode;
        public string Sha1;
        public string ThumbUrl;
        public int Aid;

        public FilesInfo.FileType Type = FilesInfo.FileType.File;
        public int Time = (int)DateTimeOffset.Now.ToUnixTimeSeconds();

        public bool Success;

        private bool? _isVideo;

        public bool IsVideo
        {
            get
            {
                if(_isVideo!=null) return (bool)_isVideo;

                _isVideo = FilesInfo.GetTypeFromIcon(Name.Split('.')[^1]) == "video";
                return (bool)_isVideo;
            }
            set=>_isVideo = value;
        }

        public FileUploadResult(string name)
        {
            Name = name;
        }

        public void SetFromOssUploadResult(OssUploadResult uploadResult)
        {
            var data = uploadResult?.data;
            if (data == null) return;

            PickCode = data.pick_code;
            FileSize = data.file_size;
            Id = data.file_id;
            ThumbUrl = data.thumb_url;
            Sha1 = data.sha1;
            Aid = data.aid;
            Cid = data.cid;
            IsVideo = data.is_video == 1;
        }
    }
}
