using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Display.Extensions;
using Display.Models.Enums;
using Display.Models.Upload;
using Display.Providers.Downloader;

namespace Display.Services.Upload.AliyunOssUpload;

/// <summary>
/// 阿里云上传服务
/// </summary>
internal class AliyunOssService : IDisposable
{
    private const string GetTokenUrl = "https://uplb.115.com/3.0/gettoken.php";

    private readonly HttpClient _httpClient;
    private readonly FileStream _stream;
    private readonly long _fileSize;
    private readonly FastUploadResult _fastUploadResult;
    private readonly IProgress<long> _progress;

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
            if (_uploadMethod != UploadMethod.None) return _uploadMethod;

            _uploadMethod = _fileSize < AliyunOssUpload.MultipartUpload.NormalPartSize ? UploadMethod.Simple : UploadMethod.Multipart;
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
    /// <param name="token"></param>
    public AliyunOssService(HttpClient client, FileStream stream, FastUploadResult fastUploadResult, IProgress<long> progress)
    {
        _httpClient = client;
        _stream = stream;
        _fileSize = stream.Length;
        _fastUploadResult = fastUploadResult;
        _progress = progress;
    }

    private async Task<OssUploadResult> SimpleUpload(CancellationToken token)
    {
        State = UploadState.OssUploading;

        var simpleUpload = new SimpleUpload(SingleOssClient, _stream, _ossToken, _fastUploadResult, token);
        var uploadResult = await simpleUpload.Start();

        State = uploadResult is { state: true } ? UploadState.Succeed : UploadState.Faulted;

        return uploadResult;
    }

    private async Task<OssUploadResult> MultipartUpload(CancellationToken token)
    {
        State = UploadState.OssUploading;

        _eTagList ??= [];
        _multipartUpload = new MultipartUpload(SingleOssClient, _stream, _ossToken, _fastUploadResult, _progress, _eTagList, token);

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

    public async Task<OssUploadResult> Start(CancellationToken token)
    {
        if (!_isInitSucceed) return null;

        return UploadMethod switch
        {
            UploadMethod.Simple => await SimpleUpload(token),
            UploadMethod.Multipart => await MultipartUpload(token),
            _ => null
        };
    }

    public void Dispose()
    {
        //_source.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task Init(CancellationToken token)
    {
        if (_isInitSucceed) return;

        State = UploadState.Initializing;

        // 获取上传需要的 ossToken
        var ossToken = await _httpClient.SendAsync<OssToken>(HttpMethod.Get, GetTokenUrl, token);

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

        //_multipartUpload.Stop();
    }

    public Task Stop()
    {
        if (UploadMethod != UploadMethod.Multipart) return Task.CompletedTask;

        State = UploadState.Canceled;
        //_source.Cancel();

        return Task.CompletedTask;
    }
}

internal enum UploadMethod
{
    None,
    Simple,
    Multipart
}