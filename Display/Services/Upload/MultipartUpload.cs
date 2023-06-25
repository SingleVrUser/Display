using Display.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Display.Helper.Encode;
using HttpHeaders = Display.Data.Const.HttpHeaders ;
using Display.Extensions;

namespace Display.Services.Upload
{
    internal class MultipartUpload : OssUploadBase, IDisposable
    {
        private const int MaxPartCount = 999;
        public const long NormalPartSize = 16777216;

        private long _partSize = NormalPartSize;
        private long _partCount = 1;

        private readonly FileStream _stream;
        private readonly long _fileSize;

        private readonly IProgress<long> _progress;
        public readonly List<string> ETagList;

        private readonly CancellationTokenSource _cts = new();
        private CancellationToken Token => _cts.Token;

        private string _uploadId;
        private int CurrentCompletedPartNum => ETagList.Count;

        public MultipartUpload(HttpClient client, FileStream stream, string endpoint, string accessKeyId, string accessKeySecret, string securityToken, string bucketName, string objectName, string callbackBase64, string callbackVarBase64, IProgress<long> progress, List<string> eTagList)
            : base(client, endpoint, accessKeyId, accessKeySecret, securityToken, bucketName, objectName, callbackBase64, callbackVarBase64)
        {
            _stream = stream;
            _fileSize = stream.Length;
            _progress = progress;
            ETagList = eTagList;
        }

        public MultipartUpload(HttpClient client, FileStream stream, string endpoint, OssToken ossToken, Upload115Result upload115Result, IProgress<long> progress, List<string> eTagList)
            : base(client, endpoint, ossToken, upload115Result)
        {
            _stream = stream;
            _fileSize = stream.Length;
            _progress = progress;
            ETagList = eTagList;
        }

        public void SetPartSizeAndCount(long partSize, long partCount)
        {
            _partSize = partSize;
            _partCount = partCount;
        }

        public Tuple<long, long> GetPartSizeAndCount()
        {
            // 文件大小小于16M
            if (_fileSize < _partSize)
            {
                _partCount = 1;
            }
            else
            {
                // 向上取整
                _partCount = _fileSize / _partSize;
                if (_fileSize % _partSize != 0)
                {
                    _partCount++;
                }

                // 分片数大于 MaxPartCount 时，指定分片数为 MaxPartCount，并修改partSize
                if (_partCount > MaxPartCount)
                {
                    _partCount = MaxPartCount;

                    // 向上取整
                    _partSize = _fileSize / _partCount;
                    if (_fileSize % _partCount != 0)
                    {
                        _partSize++;
                    }
                }
            }

            return new Tuple<long, long>(_partSize, _partCount);
        }

        public void SetUploadId(string uploadId)
        {
            _uploadId = uploadId;
        }

        public async Task<string> GetUploadId()
        {
            var url = $"{BaseUrl}?uploads&encoding-type=url";
            var request = CreateRequest(HttpMethod.Post, url);

            try
            {
                var content = await Client.SendAsync<string>(request, Token);

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);

                if (xmlDoc.DocumentElement == null) return null;

                _uploadId = xmlDoc.DocumentElement.SelectSingleNode("UploadId")?.InnerText;

                if (!string.IsNullOrEmpty(_uploadId))
                {
                    return _uploadId;
                }

                Debug.WriteLine($"上传结果：{content}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"上传出错：{ex.Message}");
            }

            return null;
        }


        public void Pause()
        {
            Dispose();
        }

        public async Task<bool> Start()
        {
            var position = CurrentCompletedPartNum * _partSize;

            _stream.Seek(position, SeekOrigin.Begin);

            int readSize = (int)_partSize;
            for (int i = CurrentCompletedPartNum + 1; i <= _partCount; i++)
            {
                if (Token.IsCancellationRequested) return false;

                if (i == _partCount)
                {
                    readSize = (int)(_fileSize % _partSize);
                }

                var url = $"{BaseUrl}?partNumber={i}&uploadId={_uploadId}";
                var progress = new Progress<long>(p =>
                {
                    var currentPosition = position + p;
                    _progress?.Report(currentPosition);
                    Debug.WriteLine($"本地文件大小：{_fileSize} 上传大小：{currentPosition}  当前进度为:{(double)currentPosition / _fileSize:P}");
                });
                var subStream = new SubStream(_stream, 0, readSize, progress);

                var content = new StreamContent(subStream)
                {
                    Headers = { ContentLength = readSize }
                };

                var request = CreateRequest(HttpMethod.Put, url, content: content);
                try
                {
                    var response = await Client.SendAsync(request, Token);

                    var etag = response.Headers.ETag?.Tag;

                    position += readSize;

                    ETagList.Add(etag);
                    Debug.WriteLine($"分片{i} 上传成功, ETag:{etag}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"上传出错：{ex.Message}");

                    return false;
                }
            }

            Debug.WriteLine("分片上传完成");

            return await End();
        }

        private async Task<bool> End()
        {
            var headers = new Dictionary<string, string>()
            {
                {HttpHeaders.Callback, CallbackBase64},
                {HttpHeaders.CallbackVar, CallbackVarBase64},
            };
            var url = $"{BaseUrl}?uploadId={_uploadId}";

            var xmlDoc = CreateCompleteMultipartUploadContent();
            var xmlStream = XamlHelper.ConvertXmlToStream(xmlDoc);
            var content = new StreamContent(xmlStream);
            var request = CreateRequest(HttpMethod.Post, url, headers, content);

            if (Token.IsCancellationRequested) return false;

            var isSucceed = false;
            try
            {
                var result = await Client.SendAsync<MultipartUploadResult>(request, Token);

                Debug.WriteLine($"上传结果：{result}");

                isSucceed = result.state;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"上传出错：{ex.Message}");
            }
            finally
            {
                await xmlStream.DisposeAsync();
            }

            return isSucceed;
        }

        private XmlDocument CreateCompleteMultipartUploadContent()
        {
            var xml = new XmlDocument();
            var completeMultipartUpload = xml.CreateElement("CompleteMultipartUpload");
            xml.AppendChild(completeMultipartUpload);

            for (int i = 0; i < ETagList.Count; i++)
            {
                var etag = ETagList[i];

                var part = xml.CreateElement("Part");
                completeMultipartUpload.AppendChild(part);

                var partNumber = xml.CreateElement("PartNumber");
                partNumber.InnerText = (i + 1).ToString();
                part.AppendChild(partNumber);

                var eTag = xml.CreateElement("ETag");
                eTag.InnerText = etag;
                part.AppendChild(eTag);
            }

            return xml;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
