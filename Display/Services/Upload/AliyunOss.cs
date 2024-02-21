﻿using Display.Extensions;
using Display.Models.Data;
using Display.Models.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Services.Upload
{
    internal class AliyunOss:IDisposable
    {
        private const string GetTokenUrl = "https://uplb.115.com/3.0/gettoken.php";

        private readonly HttpClient _httpClient;
        private readonly FileStream _stream;
        private readonly long _fileSize;
        private readonly FastUploadResult _fastUploadResult;
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

                _uploadMethod = _fileSize < Upload.MultipartUpload.NormalPartSize ? UploadMethod.Simple : UploadMethod.Multipart;
                return _uploadMethod;
            }
        }


        private UploadState _state;
        public UploadState State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;

                StateChanged?.Invoke(value);
            }
        }

        public event Action<UploadState> StateChanged;

        private static HttpClient SingleOssClient => GetInfoFromNetwork.CommonClient;

        /// <summary>
        /// 初始化AliyunOss
        /// </summary>
        /// <param name="client">需要带有cookie，用于获取ossToken</param>
        /// <param name="stream"></param>
        /// <param name="fastUploadResult"></param>
        /// <param name="progress"></param>
        public AliyunOss(HttpClient client, FileStream stream, FastUploadResult fastUploadResult, IProgress<long> progress)
        {
            _httpClient = client;
            _stream = stream;
            _fileSize = stream.Length;
            _fastUploadResult = fastUploadResult;
            _progress = progress;

        }

        private async Task<OssUploadResult> SimpleUpload()
        {
            State = UploadState.OssUploading;

            // 上传测试
            var simpleUpload = new SimpleUpload(SingleOssClient, _stream, _ossToken, _fastUploadResult);
            var uploadResult = await simpleUpload.Start();

            State = uploadResult is { state: true } ? UploadState.Succeed : UploadState.Faulted;

            return uploadResult;
        }

        private async Task<OssUploadResult> MultipartUpload()
        {
            State = UploadState.OssUploading;

            _eTagList ??= new List<string>();
            _multipartUpload = new MultipartUpload(SingleOssClient, _stream, _ossToken, _fastUploadResult, _progress, _eTagList);

            // 获取UploadId
            if (string.IsNullOrEmpty(_uploadId)) _uploadId = await _multipartUpload.GetUploadId();
            else _multipartUpload.SetUploadId(_uploadId);

            // UploadId为空，无法继续
            if (string.IsNullOrEmpty(_uploadId))
            {
                State = UploadState.Faulted;
                return null;
            }

            // 获取partSize 和 partCount
            if (_partSize == 0 || _partCount == 0) (_partSize, _partCount) = _multipartUpload.GetPartSizeAndCount();
            else _multipartUpload.SetPartSizeAndCount(_partSize, _partCount);

            var uploadResult = await _multipartUpload.Start();

            if (State != UploadState.Paused)
            {
                State = uploadResult is { state: true } ? UploadState.Succeed : UploadState.Faulted;
            }
           
            return uploadResult;
        }

        public async Task<OssUploadResult> Start()
        {
            if (!_isInitSucceed) return null;

            return UploadMethod switch
            {
                UploadMethod.Simple => await SimpleUpload(),
                UploadMethod.Multipart => await MultipartUpload(),
                _ => null
            };
        }

        public void Dispose()
        {
            _source.Cancel();
            _source.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task Init()
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

        public void Pause()
        {
            if (UploadMethod != UploadMethod.Multipart) return;

            State = UploadState.Paused;
            _eTagList = _multipartUpload.ETagList;
            _multipartUpload.Stop();
        }

        public Task Stop()
        {
            if (UploadMethod != UploadMethod.Multipart) return Task.CompletedTask;

            State = UploadState.Canceled;
            Dispose();
            return Task.CompletedTask;
        }

    }

    internal enum UploadMethod
    {
        None,
        Simple,
        Multipart
    }
}
