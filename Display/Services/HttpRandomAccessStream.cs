using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;

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
        private bool _isDisposing;

        // No public constructor, factory methods instead to handle async tasks.
        private HttpRandomAccessStream(HttpClient client, Uri uri)
        {
            _client = client;
            _requestedUri = uri;
            Position = 0;
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

        public static Task<HttpRandomAccessStream> CreateAsync(HttpClient client, Uri uri)
        {
            var randomStream = new HttpRandomAccessStream(client, uri);

            return AsyncInfo.Run(async _ =>
            {
                await randomStream.SendRequestAsync();
                return randomStream;
            }).AsTask();
        }

        public IRandomAccessStream CloneStream() => this;

        public IInputStream GetInputStreamAt(ulong position)
            => _inputStream;

        public IOutputStream GetOutputStreamAt(ulong position)
            => throw new NotImplementedException();

        public void Seek(ulong position)
        {
            if (Position == position) return;

            // 重置_inputStream
            Debug.WriteLine("重置_inputStream");
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }

            //Debug.WriteLine("Seek: {0:N0} -> {1:N0}", Position, position);
            Position = position;
        }

        public void Dispose()
        {
            if (_isDisposing)
            {
                return;
            }

            _isDisposing = true;

            Debug.WriteLine("开始销毁_inputStream");

            _inputStream?.Dispose();
            _inputStream = null;

            //Debug.WriteLine("开始销毁_client");
            //_client?.Dispose();
            //_client = null;
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            if (_isDisposing || !CanRead)
            {
                return default;
            }

            var result = AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                if (_isDisposing)
                {
                    return null;
                }

                progress.Report(0);

                try
                {
                    // _inputStream为空，重新获取_inputStream
                    if (_inputStream == null && !_isDisposing)
                    {
                        Debug.WriteLine("_inputStream为空,尝试从Url中获取");
                        await SendRequestAsync();
                    }

                    if (_inputStream == null || _isDisposing)
                    {
                        Debug.WriteLine("尝试获取_inputStream后依旧未为空，返回默认值");

                        return null;
                    }


                    var result = await _inputStream.ReadAsync(buffer, count, options).AsTask(cancellationToken, progress).ConfigureAwait(false);

                    // Move position forward.
                    Position += result.Length;
                    //Debug.WriteLine("requestedPosition = {0:N0}", Position);
                    return result;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    return null;
                }

                return null;
            });



            return result;
        }

        public IAsyncOperation<bool> FlushAsync()
            => throw new NotImplementedException();

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
            => throw new NotImplementedException();

        private async Task SendRequestAsync()
        {
            if (_isDisposing || !CanRead || _client == null)
            {
                return;
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
                Debug.WriteLine("_client访问");

                response = await _client.SendRequestAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead).AsTask(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误:{ex.Message}");
                CanRead = false;
                Dispose();
                return;
            }

            _size = response.Content?.Headers?.ContentLength ?? 0;

            if (response.Content?.Headers == null || !response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"访问视频出错：{response.StatusCode}");
                CanRead = false;
                Dispose();
                return;
            }

            if (response.Content.Headers.ContentType != null)
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

            _inputStream = await response.Content.ReadAsInputStreamAsync().AsTask(cancellationToken).ConfigureAwait(false);

        }
    }
}