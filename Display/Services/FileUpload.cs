using Display.Data;
using Display.Extensions;
using Display.Helper;
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

namespace Display.Services
{
    internal class FileUpload: UploadBase
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
                if (!string.IsNullOrEmpty(cookie))
                {
                    headers.Add("Cookie", cookie);
                }

                _client = GetInfoFromNetwork.CreateClient(headers);

                return _client;
            }
        }

        private readonly string _path;
        private readonly FileInfo _fileInfo;
        private readonly string _fileSizeString;
        private readonly FileStream _stream;
        private readonly long _saveFolderCid;
        private readonly CancellationTokenSource _source = new();

        private CancellationToken Token => _source.Token;
        private int _userId;
        private string _userKey;
        private string _totalSha1;
        private bool _isInitSucceed;

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
            var result = _stream?.DisposeAsync() ?? ValueTask.CompletedTask;
            GC.SuppressFinalize(this);

            return result;
        }

        public FileUpload(string path, long cid, int userId = 0, string userKey = "")
        {
            _path = path;
            _saveFolderCid = cid;
            _userId = userId;
            _userKey = userKey;

            _fileInfo = new FileInfo(_path);
            if (!_fileInfo.Exists) return;

            Position = 0;
            _fileSizeString = _fileInfo.Length.ToByteSizeString();

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

        private bool IsGetUploadInfo => _userId != 0 && !string.IsNullOrEmpty(_userKey);

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
            _totalSha1 = await HashHelper.ComputeSha1ByStream(_stream, Token, progress: new Progress<long>(i =>
                {
                    Content = $"{i.ToByteSizeString()}/{_fileSizeString}";
                }
            ));

            if (string.IsNullOrEmpty(_totalSha1))
            {
                State = UploadState.Canceled;
            }
            else
            {
                Length = _fileInfo.Length;
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

        public override async Task Stop()
        {
            await DisposeAsync();

            State = UploadState.Canceled;
        }


        public async Task<bool> UploadFile()
        {
            var upload115Result = await UploadByFastUpload();

            if (upload115Result == null) return State == UploadState.Succeed;

            //转换callback、callbackVar为base64格式
            upload115Result.callback.callback =
                Convert.ToBase64String(Encoding.Default.GetBytes(upload115Result.callback.callback));
            upload115Result.callback.callback_var =
                Convert.ToBase64String(Encoding.Default.GetBytes(upload115Result.callback.callback_var));

            return await UploadByAliyunOss(upload115Result);
        }

        private async Task<Upload115Result> UploadByFastUpload()
        {
            State = UploadState.FastUploading;

            var aesKey = UploadKey.Instance.AesKey;
            var aesIv = UploadKey.Instance.AesIv;
            var clientPublicKey = UploadKey.Instance.ClientPublicKey;

            var target = "U_1_" + _saveFolderCid;
            var fileName = _fileInfo.Name;
            var fileSize = _fileInfo.Length;
            var fileId = _totalSha1;
            var signKey = string.Empty;
            var signVal = string.Empty;
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

                if (!string.IsNullOrEmpty(signKey) && !string.IsNullOrEmpty(signVal))
                {
                    dataForm["sign_key"] = signKey;
                    dataForm["sign_val"] = signVal;
                }

                Upload115Result upload115Result;
                try
                {
                    upload115Result = await GetUpload115Result(dataForm, timeSpan, aesKey, aesIv, clientPublicKey, Token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"获取Upload115Result时发生错误：{ex.Message}");
                    break;
                }

                if (upload115Result == null)
                {
                    Debug.WriteLine("可能是解密出错");
                    continue;
                }

                if (string.IsNullOrEmpty(upload115Result.sign_key) || string.IsNullOrEmpty(upload115Result.sign_check))
                {
                    // 不能秒传，需要上传
                    if (!string.IsNullOrEmpty(upload115Result.Object))
                    {
                        // 使用AliyunOss上传
                        return upload115Result;
                    }

                    // 秒传成功
                    if (upload115Result.status == 2)
                    {
                        Position = _fileInfo.Length;
                        State = UploadState.Succeed;
                    }
                    else
                    {
                        Debug.WriteLine($"上传时发生错误：{upload115Result.statusmsg}");
                        State = UploadState.Faulted;
                    }

                    return null;
                }

                signKey = upload115Result.sign_key;

                signVal = HashHelper.ComputeSha1RangeByStream(_stream, upload115Result.sign_check);
            }

            State = UploadState.Faulted;
            return null;
        }

        private static async Task<Upload115Result> GetUpload115Result(Dictionary<string, string> dataForm, long timeSpan, byte[] aesKey, byte[] aesIv, byte[] clientPublicKey,CancellationToken token)
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

            var response = await Client.SendAsync(request,token);

            if (!response.IsSuccessStatusCode) return null;

            var contentBytes = await response.Content.ReadAsByteArrayAsync(token);

            return UploadEncryptHelper.DecryptReceiveData(contentBytes, aesKey: aesKey, aesIv: aesIv);
        }

        private AliyunOss _aliyunOss;
        private async Task<bool> UploadByAliyunOss(Upload115Result upload115Result)
        {
            if (_aliyunOss == null)
            {
                _aliyunOss = new AliyunOss(Client, _stream, upload115Result, progress: new Progress<long>(
                    p =>
                    {
                        Position = p;
                        Content = $"{p.ToByteSizeString()}/{_fileSizeString}";
                    }));

                _aliyunOss.StateChanged += state =>
                {
                    if (state is not (UploadState.Initialized or UploadState.Initialized))
                    {
                        State = state;
                    }
                };
            }

            await _aliyunOss.Init();

            return await StartUpload();
        }
        private async Task<bool> StartUpload()
        {
            var result = await _aliyunOss.Start();

            // 成功
            if (result) await DisposeAsync();
            else if (State == UploadState.Faulted) await DisposeAsync();

            return result;
        }
        
    }
}
