using Display.Data;
using Display.Services.Upload;
using Tests.Helper;

namespace Tests
{
    [TestClass]
    public class RenameTest
    {
        private const int UserId = UserInfo.UserId;
        private const string UserKey = UserInfo.UserKey;
        private const string AppVer = Const.DefaultSettings.Network._115.UploadAppVersion;

        private readonly HttpClient _uploadClient = RequestHelper.UploadClient;
        private readonly HttpClient _downloadClient = RequestHelper.DownloadClient;

        private readonly CancellationTokenSource _cts = new();
        private CancellationToken Token => _cts.Token;

        [TestMethod]
        [DataRow("apgml7r6jsno5h415", 2669756539857796870, 2669737609118350523, "92CDC30B5117CA3AEDE4286A54366E99D0477E73", "new1.mp4", 104062793)]
        public async Task RenameForced(string pickCode, long id, long cid, string totalSha1, string newName, long length)
        {
            // 通过秒传上传一份
            var result = await FileUpload.UploadAgainByFastUpload(_downloadClient,pickCode, cid, totalSha1, newName, length,UserId, UserKey, token: Token);
            Assert.IsNotNull(result);

            var newPickCode = result.pickcode;
            Assert.IsNotNull(newPickCode);

            // 删除原文件
            var deleteResult = await WebApi.DeleteFiles(_downloadClient, cid, new[] { id });
            Assert.IsTrue(deleteResult);
        }

    }
}
