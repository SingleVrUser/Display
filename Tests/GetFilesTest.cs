using Display.Extensions;
using Display.Helper.Crypto;
using Display.Models.Data;
using Display.Services.Upload;
using Tests.Helper;
using FileInfo = Tests.Models.FileInfo;

namespace Tests
{
    [TestClass]
    public class GetFilesTest
    {
        private const long RootCid = 0;
        private const int UserId = UserInfo.UserId;
        private const string UserKey = UserInfo.UserKey;
        private const string AppVer = Const.DefaultSettings.Network._115.UploadAppVersion;

        private readonly HttpClient _client = RequestHelper.UploadClient;

        private readonly CancellationTokenSource _cts = new();
        private CancellationToken Token => _cts.Token;

        /// <summary>
        /// 有加密，格式与Web有所不同
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetFilesByDesktopTest()
        {   
            var aesKey = UploadKey.Instance.AesKey;
            var aesIv = UploadKey.Instance.AesIv;
            var clientPublicKey = UploadKey.Instance.ClientPublicKey;

            var kec = UploadEncryptHelper.GetKEc(clientPublicKey, RootCid);
            Assert.IsNotNull(kec);

            var dataForm = new Dictionary<string, string>
            {
                {"user_id", UserId.ToString()},
                {"show_dir", "1"},
                {"limit", "100"},
                {"offset", "0"},
                {"aid", "1"},
                {"cid", RootCid.ToString()},
                {"asc", "1"},
                {"record_open_time", "1"},
                {"custom_order", "0"},
                {"k_ec", kec}
            };

            var data = string.Join("&", dataForm.Select(x => $"{x.Key}={x.Value}"));

            var url = $"https://proapi.115.com/pc/ufile/files?{data}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await RequestHelper.UploadClient.SendAsync(request, Token);

            var contentBytes = await response.Content.ReadAsByteArrayAsync(Token);

            var result = UploadEncryptHelper.DecryptReceiveData<FileInfo.Desktop.Info>(contentBytes, aesKey: aesKey, aesIv: aesIv);
            
            Assert.AreEqual(result.state, true);
            Assert.AreEqual(result.cid, RootCid);
            Assert.AreEqual(result.offset, 0);
        }

        /// <summary>
        /// 无加密，格式与Web有所不同
        /// </summary>
        /// <param name="cid"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(2665320937582231158)]
        public async Task GetFilesByTvTest(long cid)
        {
            var url = $"https://proapi.115.com/box/files?ver=2&offset=0&user_id={UserId}&limit=16&show_dir=1&aid=1&cid={cid}&app_ver=16.3.5";

            var result = await _client.SendAsync<FileInfo.Tv.Info>(HttpMethod.Get, url, Token);

            Assert.IsNotNull(result);
        }


    }
}
