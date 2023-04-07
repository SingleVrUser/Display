using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace MediaPlayerElement_Test.Models
{
    internal class HttpRandomAccessStream : IRandomAccessStreamWithContentType
    {
        private readonly Uri _requestedUri;
        private HttpClient _client;
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

            return AsyncInfo.Run(async (_) =>
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
            if (Position != position)
            {
                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }

                Debug.WriteLine("Seek: {0:N0} -> {1:N0}", Position, position);
                Position = position;
            }
        }

        public void Dispose()
        {
            Debug.WriteLine("开始销毁stream");

            if (_isDisposing)
            {
                return;
            }

            Debug.WriteLine("开始销毁_inputStream");
            _isDisposing = true;
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }

            if (_client == null) return;

            _client?.Dispose();
            _client = null;
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            if (_isDisposing || !CanRead)
            {
                return default;
            }

            return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                if (_isDisposing)
                {
                    return default;
                }

                progress.Report(0);

                try
                {
                    // _inputStream为空，重新获取_inputStream
                    if (_inputStream == null)
                    {
                        await SendRequestAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                if (_inputStream == null)
                {
                    return default;
                }

                try
                {

                    var result = await _inputStream.ReadAsync(buffer, count, options).AsTask(cancellationToken, progress).ConfigureAwait(false);

                    // Move position forward.
                    Position += result.Length;
                    //Debug.WriteLine("requestedPosition = {0:N0}", Position);
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return default;
            });
        }

        public IAsyncOperation<bool> FlushAsync()
            => throw new NotImplementedException();

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
            => throw new NotImplementedException();

        private async Task SendRequestAsync()
        {
            if (_isDisposing || !CanRead)
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

            if (_client == null)
            {
                return;
            }

            HttpResponseMessage response;

            //设置超时时间5s
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
            try
            {
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

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"访问网页出错：{response.StatusCode}");
                CanRead = false;
                Dispose();
                return;
            }

            if (response.Content.Headers.ContentType != null)
            {
                ContentType = response.Content.Headers.ContentType.MediaType;
            }

            _size = response.Content?.Headers?.ContentLength ?? 0;

            //Debug.WriteLine($"总大小：{_size}");

            if (string.IsNullOrEmpty(_etagHeader) && response.Headers.ContainsKey("ETag"))
            {
                _etagHeader = response.Headers["ETag"];
            }

            if (string.IsNullOrEmpty(_lastModifiedHeader) && response.Content.Headers.ContainsKey("Last-Modified"))
            {
                _lastModifiedHeader = response.Content.Headers["Last-Modified"];
            }

            if (response.Content.Headers.ContainsKey("Content-Type"))
            {
                ContentType = response.Content.Headers["Content-Type"];
            }

            _inputStream = await response.Content.ReadAsInputStreamAsync().AsTask().ConfigureAwait(false);
        }
    }
}
