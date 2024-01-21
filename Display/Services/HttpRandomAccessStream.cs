using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Org.BouncyCastle.Math;

namespace Display.Services
{
    internal class HttpRandomAccessStream : IRandomAccessStreamWithContentType
    {
        private readonly Uri _requestedUri;
        private readonly HttpClient _client;
        private IInputStream _inputStream;
        private ulong _size;
        private string _etagHeader;
        private string _lastModifiedHeader;
        //private bool _isDisposing;

        // No public constructor, factory methods instead to handle async tasks.
        private HttpRandomAccessStream(HttpClient client, Uri uri)
        {
            _client = client;
            _requestedUri = uri;
        }

        public ulong Position { get; private set; }

        public string ContentType { get; private set; } = string.Empty;

        public bool CanRead { get; private set; } = true;

        public bool CanWrite => false;

        public ulong Size
        {
            get => _size;
            set => throw new NotImplementedException();
        }

        public IAsyncOperation<bool> FlushAsync()
            => throw new NotImplementedException();

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
            => throw new NotImplementedException();

        public IOutputStream GetOutputStreamAt(ulong position)
            => throw new NotImplementedException();

        public IRandomAccessStream CloneStream() => this;

        public IInputStream GetInputStreamAt(ulong position)
            => _inputStream;


        /// <summary>
        /// 创建的同时获取到视频Size(position为0时的length)
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Task<HttpRandomAccessStream> CreateAsync(HttpClient client, Uri uri)
        {
            var randomStream = new HttpRandomAccessStream(client, uri);

            return AsyncInfo.Run(async _ =>
            {
                await randomStream.SendRequestAsync(0);
                return randomStream;
            }).AsTask();
        }

        //public static HttpRandomAccessStream Create(HttpClient client, Uri uri)
        //{
        //    return new HttpRandomAccessStream(client, uri);
        //}

        public void Seek(ulong position)
        {
            if (Position == position) return;

            Debug.WriteLine("Seek: {0:N0} -> {1:N0}", Position, position);

            // Dispose inputStream，销毁前一个_inputStream，访问量过大会触发403 Forbidden
            _inputStream?.Dispose();
            _inputStream = null;

            Position = position;
        }
        
        void IDisposable.Dispose()
        {
            _inputStream?.Dispose();
            _inputStream = null;
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                //progress.Report(0);

                if (_inputStream is null)
                {
                    Debug.WriteLine("_inputStream为空,尝试从Url中获取");
                    await SendRequestAsync();
                }

                if (_inputStream is null)
                {
                    Debug.WriteLine("_inputStream为空");
                    return await Task.FromCanceled<IBuffer>(cancellationToken);
                }

                var result = await _inputStream.ReadAsync(buffer, count, options).AsTask(cancellationToken, progress);

                // Move position forward.
                Position += result.Length;
                //Debug.WriteLine("requestedPosition = {0:N0}", Position);
                return result;

            });
        }

        private async Task SendRequestAsync(int waitSecondWhenRequestZeroPosition = 2)
        {
            if (!CanRead || _client == null)
            {
                return;
            }

            if (Position == 0 && waitSecondWhenRequestZeroPosition != 0)
            {
                await Task.Delay(waitSecondWhenRequestZeroPosition * 1000);
            }

            var request = new HttpRequestMessage(HttpMethod.Get, _requestedUri);

            request.Headers.Add("Range", $"bytes={Position}-");
            request.Headers.Add("Connection", "Keep-Alive");

            if (!string.IsNullOrEmpty(_etagHeader))
            {
                request.Headers.Add("If-Match", _etagHeader);
            }

            if (!string.IsNullOrEmpty(_lastModifiedHeader))
            {
                request.Headers.Add("If-Unmodified-Since", _lastModifiedHeader);
            }

            HttpResponseMessage response;

            //设置超时时间5s
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;

            try
            {

                response = await _client.SendRequestAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead).AsTask(cancellationToken: cancellationToken);
                
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine($"任务超时：{ex.Message}");
                CanRead = false;
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"访问视频错误:{ex.Message}");
                CanRead = false;
                return;
            }

            var contentLength = response.Content?.Headers?.ContentLength ?? 0;

            if (response.Content?.Headers == null || !response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"视频格式错误：{response.StatusCode}");

                if (contentLength < 1024)
                {
                    var result = await response.Content?.ReadAsStringAsync();
                    Debug.WriteLine($"服务器响应结果：${result}");
                }
                CanRead = false;
                return;
            }

            if (Position == 0)
            {
                _size = contentLength;
                Debug.WriteLine("视频总大小:{0:N0}", _size);
            }

            if (response.Content.Headers.ContentType is not null)
            {
                ContentType = response.Content.Headers.ContentType.MediaType;
            }

            if (string.IsNullOrEmpty(_etagHeader) && response.Headers.TryGetValue("ETag", out var etagHeader))
            {
                _etagHeader = etagHeader;
            }

            if (string.IsNullOrEmpty(_lastModifiedHeader) && response.Content.Headers.TryGetValue("Last-Modified", out var lastModifiedHeader))
            {
                _lastModifiedHeader = lastModifiedHeader;
            }

            if (response.Content.Headers.TryGetValue("Content-Type", out var contentType))
            {
                ContentType = contentType;
            }

            _inputStream = await response.Content.ReadAsInputStreamAsync().AsTask(cancellationToken);

        }
    }
}