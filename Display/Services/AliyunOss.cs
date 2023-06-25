using Aliyun.OSS;
using Display.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Services
{
    internal class AliyunOss : UploadBase
    {

        private readonly FileStream _stream;
        private readonly HttpClient _httpClient;
        private readonly Upload115Result _upload115Result;
        private readonly IProgress<long> _progress;
        private readonly CancellationTokenSource _source = new();
        private CancellationToken Token => _source.Token;

        private List<string> _eTagList = new();
        private string _uploadId;
        private long _partSize;
        private long _partCount;
        private OssToken _ossToken;
        private bool _isInitSucceed;
        private MultipartUpload _upload;

        private static HttpClient _ossClient;
        private static HttpClient SingleOssClient
        {
            get
            {
                _ossClient ??= new HttpClient();

                return _ossClient;
            }
        }

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
            throw new NotImplementedException();
        }

        public override ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public override Task Init()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Start()
        {
            throw new NotImplementedException();
        }

        public override void Pause()
        {
            throw new NotImplementedException();
        }

        public override Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
