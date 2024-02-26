using Display.Extensions;
using Display.Helper.Encode;
using Display.Models.Upload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using HttpHeaders = Display.Models.Data.Constant.HttpHeaders;

namespace Display.Services.Upload
{
    internal class MultipartUpload : OssUploadBase
    {
        private const int MaxPartCount = 999;
        public const long NormalPartSize = 16777216;

        private long _partSize = NormalPartSize;
        private long _partCount = 1;

        private readonly IProgress<long> _progress;
        public readonly List<string> ETagList;

        private string _uploadId;
        private int CurrentCompletedPartNum => ETagList.Count;

        public MultipartUpload(HttpClient client, FileStream stream, OssToken ossToken, FastUploadResult fastUploadResult, IProgress<long> progress, List<string> eTagList)
            : base(client, stream, ossToken, fastUploadResult)
        {
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
            if (FileSize < _partSize)
            {
                _partCount = 1;
            }
            else
            {
                // 向上取整
                _partCount = FileSize / _partSize;
                if (FileSize % _partSize != 0)
                {
                    _partCount++;
                }

                // 分片数大于 MaxPartCount 时，指定分片数为 MaxPartCount，并修改partSize
                if (_partCount > MaxPartCount)
                {
                    _partCount = MaxPartCount;

                    // 向上取整
                    _partSize = FileSize / _partCount;
                    if (FileSize % _partCount != 0)
                    {
                        _partSize++;
                    }
                }
            }

            return new Tuple<long, long>(_partSize, _partCount);
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

                // 成功获取UploadId
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


            Dispose();
            return null;
        }

        public void SetUploadId(string uploadId)
        {
            _uploadId = uploadId;
        }

        public override async Task<OssUploadResult> Start()
        {
            var position = CurrentCompletedPartNum * _partSize;

            Stream.Seek(position, SeekOrigin.Begin);

            var readSize = (int)_partSize;
            var isSucceed = true;
            for (var i = CurrentCompletedPartNum + 1; i <= _partCount; i++)
            {
                if (Token.IsCancellationRequested) return null;

                if (i == _partCount)
                {
                    readSize = (int)(FileSize % _partSize);
                }

                var url = $"{BaseUrl}?partNumber={i}&uploadId={_uploadId}";
                var progress = new Progress<long>(p =>
                {
                    var currentPosition = position + p;
                    _progress?.Report(currentPosition);
                    //Debug.WriteLine($"本地文件大小：{FileSize} 上传大小：{currentPosition}  当前进度为:{(double)currentPosition / FileSize:P}");
                });
                var subStream = new SubStream(Stream, 0, readSize, progress);

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
                    //Debug.WriteLine($"分片{i} 上传成功, ETag:{etag}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"上传出错：{ex.Message}");
                    isSucceed = false;
                    break;
                }
            }

            if (isSucceed)
            {
                Debug.WriteLine("分片上传完成");
                return await End();
            }

            Dispose();
            return null;
        }

        public override void Stop()
        {
            Dispose();
        }

        private async Task<OssUploadResult> End()
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

            if (Token.IsCancellationRequested) return null;

            OssUploadResult result = null;
            try
            {
                result = await Client.SendAsync<OssUploadResult>(request, Token);

                Debug.WriteLine($"上传结果：{result}");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"上传出错：{ex.Message}");
            }
            finally
            {
                await xmlStream.DisposeAsync();
            }

            Dispose();
            return result;
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

    }
}
