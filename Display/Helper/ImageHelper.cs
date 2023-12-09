using Display.Data;
using Display.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Display.Helper
{
    internal class ImageHelper
    {
        private static readonly HttpClient Client = WebApi.GlobalWebApi.Client;

        public static async Task<WebFileInfo> GetFileAsync(long? cid, int limit = 40, int offset = 0, bool useApi2 = false, bool loadAll = false, string orderBy = "user_ptime", int asc = 0, bool isOnlyFolder = false)
        {
            var url = !useApi2 ? $"https://webapi.115.com/files?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json" :
                //旧接口只有t，没有修改时间（te），创建时间（tp）
                $"https://aps.115.com/natsort/files.php?aid=1&cid={cid}&o={orderBy}&asc={asc}&offset={offset}&show_dir=1&limit={limit}&code=&scid=&snap=0&natsort=1&record_open_time=1&source=&format=json&fc_mix=0&type=&star=&is_share=&suffix=&custom_order=";

            if (isOnlyFolder)
                url += "&nf=1";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await Client.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            var webFileInfoResult = JsonConvert.DeserializeObject<WebFileInfo>(content);

            if (webFileInfoResult == null) return null;

            //接口1出错，使用接口2
            if (webFileInfoResult.errNo == 20130827 && useApi2 == false)
            {
                webFileInfoResult = await GetFileAsync(cid, limit, offset, true, loadAll, webFileInfoResult.order, webFileInfoResult.is_asc, isOnlyFolder: isOnlyFolder);
            }
            //需要加载全部，但未加载全部
            else if (loadAll && webFileInfoResult.count > limit)
            {
                webFileInfoResult = await GetFileAsync(cid, webFileInfoResult.count, offset, useApi2, true, orderBy, asc);
            }

            return webFileInfoResult;
        }

        public static async Task<ImageInfo> GetImageInfo(string pickCode)
        {
            var url = $"https://webapi.115.com/files/image?pickcode={pickCode}&_={DateTimeOffset.Now.ToUnixTimeSeconds()}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(result)) return null;

            return JsonConvert.DeserializeObject<ImageInfo>(result);
        }

        public static async Task<Tuple<Stream, long>> GetStreamFromUrl(string url, CancellationToken token = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            var tmpSize = response.Content.Headers.ContentLength;
            if (tmpSize == null) return null;

            var size = (long)tmpSize;
            return !response.IsSuccessStatusCode ? null : Tuple.Create(await response.Content.ReadAsStreamAsync(token), size);
        }

        public static async Task<IRandomAccessStream> GetIRandomAccessStreamFromUrl(string url, IProgress<int> progress, CancellationToken token = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode) return null;

            var tmpSize = response.Content.Headers.ContentLength;
            if (tmpSize == null) return null;

            var size = (long)tmpSize;
            await using var stream = await response.Content.ReadAsStreamAsync(token);

            var buffer = new byte[81920];

            int bytesRead;
            var position = 0;

            var memStream = new MemoryStream();
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) != 0)
            {
                await memStream.WriteAsync(buffer, 0, bytesRead, token);

                position += bytesRead;
                progress?.Report((int)((double)position / size * 100));
            }

            memStream.Position = 0;
            return memStream.AsRandomAccessStream();
        }
    }
}
