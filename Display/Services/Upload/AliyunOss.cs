using Aliyun.OSS;
using Display.Data;
using Display.Extensions;
using Display.Models;
using OpenCvSharp.XPhoto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Services.Upload
{
    internal class AliyunOss : UploadBase
    {
        private const string Endpoint = "http://oss-cn-shenzhen.aliyuncs.com";
        private const string GetTokenUrl = "https://uplb.115.com/3.0/gettoken.php";

        private readonly FileStream _stream;
        private readonly HttpClient _httpClient;
        private readonly Upload115Result _upload115Result;
        private readonly IProgress<long> _progress;
        private readonly CancellationTokenSource _source = new();
        private CancellationToken Token => _source.Token;

        private string _uploadId;
        private long _partSize;
        private long _partCount;
        private OssToken _ossToken;
        private bool _isInitSucceed;

        private List<string> _eTagList;
        private MultipartUpload _multipartUpload;

        private UploadMethod _uploadMethod;

        private UploadMethod UploadMethod
        {
            get
            {
                if(_uploadMethod != UploadMethod.None) return _uploadMethod;

                _uploadMethod = Length < Upload.MultipartUpload.NormalPartSize ? UploadMethod.Simple : UploadMethod.Multipart;
                return _uploadMethod;
            }
        }

        //private static HttpClient _ossClient;
        private static HttpClient SingleOssClient => GetInfoFromNetwork.CommonClient;

        /// <summary>
        /// 初始化AliyunOss
        /// </summary>
        /// <param name="client">需要带有cookie，用于获取ossToken</param>
        /// <param name="stream"></param>
        /// <param name="upload115Result"></param>
        /// <param name="progress"></param>
        public AliyunOss(HttpClient client, FileStream stream, Upload115Result upload115Result, IProgress<long> progress)
        {
            _httpClient = client;
            _stream = stream;
            Length = stream.Length;
            _upload115Result = upload115Result;
            _progress = progress;

        }

        public override void Dispose()
        {
            _source.Cancel();
            _source.Dispose();

            GC.SuppressFinalize(this);
        }

        public override ValueTask DisposeAsync()
        {
            _source.Cancel();
            _source.Dispose();

            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask; ;
        }

        public override async Task Init()
        {
            if (_isInitSucceed) return;

            State = UploadState.Initializing;


            // 获取上传需要的 ossToken
            var ossToken = await _httpClient.SendAsync<OssToken>(HttpMethod.Get, GetTokenUrl, Token);

            if (ossToken != null)
            {
                _ossToken = ossToken;
                _isInitSucceed = true;
                State = UploadState.Initialized;
            }
            else
            {
                State = UploadState.Faulted;
            }
        }

        private async Task<bool> MultipartUpload()
        {
            State = UploadState.MultipartUploading;

            _eTagList ??= new List<string>();
            _multipartUpload = new MultipartUpload(SingleOssClient, _stream, Endpoint, _ossToken, _upload115Result, _progress, _eTagList);

            // 获取UploadId
            if (string.IsNullOrEmpty(_uploadId)) _uploadId = await _multipartUpload.GetUploadId();
            else _multipartUpload.SetUploadId(_uploadId);

            if (string.IsNullOrEmpty(_uploadId))
            {
                State = UploadState.Faulted;
                return false;
            }

            // 获取partSize 和 partCount
            if (_partSize == 0 || _partCount == 0) (_partSize, _partCount) = _multipartUpload.GetPartSizeAndCount();
            else _multipartUpload.SetPartSizeAndCount(_partSize, _partCount);

            var result = await _multipartUpload.Start();
            if (result) State = UploadState.Succeed;

            return result;
        }

        public override async Task<bool> Start()
        {
            if (!_isInitSucceed) return false;

            return UploadMethod switch
            {
                UploadMethod.Simple => await SimpleUpload(),
                UploadMethod.Multipart => await MultipartUpload(),
                _ => false
            };
        }

        public override void Pause()
        {
            if (UploadMethod != UploadMethod.Multipart) return;

            _eTagList = _multipartUpload.ETagList;
            _multipartUpload.Pause();
            State = UploadState.Paused;
        }

        public override Task Stop()
        {
            if (UploadMethod != UploadMethod.Multipart) return Task.CompletedTask;

            State = UploadState.Canceled;
            return Task.CompletedTask;
        }

        private async Task<bool> SimpleUpload()
        {
            // 上传测试
            var simpleUpload = new SimpleUpload(SingleOssClient, Endpoint, _ossToken, _upload115Result);

            return await simpleUpload.Start(_stream, Token);
        }
    }

    internal enum UploadMethod
    {
        None,
        Simple,
        Multipart
    }
}
