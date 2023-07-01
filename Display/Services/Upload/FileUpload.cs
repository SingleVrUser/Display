using Display.Data;
using Display.Extensions;
using Display.Helper.Crypto;
using Display.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static System.String;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Display.Services.Upload
{
    internal class FileUpload : UploadBase
    {
        private const string AppVer = Const.DefaultSettings.Network._115.UploadAppVersion;

        private static HttpClient _client;
        private static HttpClient Client
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

        private async Task<FastUploadResult> UploadByFastUpload()
        {
            State = UploadState.FastUploading;

            var aesKey = UploadKey.Instance.AesKey;
            var aesIv = UploadKey.Instance.AesIv;
            var clientPublicKey = UploadKey.Instance.ClientPublicKey;

            var target = "U_1_" + _saveFolderCid;
            var fileName = _fileInfo.Name;
            var fileSize = _fileInfo.Length;
            var fileId = _totalSha1;
            var signKey = Empty;
            var signVal = Empty;
            var sign = UploadEncryptHelper.GetSign(_userId, fileId, target, _userKey);
            var userIdMd5 = HashHelper.ComputeMd5ByContent(_userId.ToString()).ToLower();

            var dataForm = new Dictionary<string, string>
            {
                {"appid", "0"},
                {"appversion", AppVer},
                {"filename", HttpUtility.UrlEncode(fileName, Encoding.UTF8)},
                {"filesize", fileSize.ToString()},
                {"fileid", fileId},
                {"target", target},
                {"userid", _userId.ToString()},
                {"sig", sign},
            };

            for (var i = 0; i < 3; i++)
            {
                var timeSpan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                dataForm["t"] = timeSpan.ToString();
                dataForm["token"] = UploadEncryptHelper.GetToken(fileId, fileSize, signKey, signVal, timeSpan, _userId, userIdMd5, AppVer);

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
