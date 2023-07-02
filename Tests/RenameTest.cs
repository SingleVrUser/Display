using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using Tests.Helper;
using Display.Extensions;
using Display.Helper.Crypto;
using System.IO;
using Display.Models;
using Display.Services.Upload;
using Microsoft.UI.Xaml.Controls;
using System.Text;
using System.Web;
using Display.Data;
using Windows.Media.Protection.PlayReady;

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

        //private async Task<string> GetRangSha1FromInternet(string pickCode, string signCheck)
        //{
        //    // 获取下载链接
        //    var downUrls = await WebApi.GetDownUrl(_downloadClient, pickCode, GetInfoFromNetwork.DownUserAgent, false);

        //    if (downUrls is not { Count: > 0 })
        //    {
        //        Assert.Fail("未获取到下载地址");
        //    }

        //    var downUrl = downUrls.First().Value;

        //    // 根据下载链接获取分段Sha1
        //    return await GetRangSha1FromDownUrl(downUrl, signCheck);
        //}

        //private async Task<FastUploadResult?> UploadByFastUpload(string pickCode, long cid, string totalSha1, string newName, long length)
        //{
        //    var signKey = string.Empty;
        //    var signVal = string.Empty;
        //    var dataForm = FileUpload.BuildDataForm(cid, totalSha1, newName, length, UserId, UserKey, out var userIdMd5);

        //    var aesKey = UploadKey.Instance.AesKey;
        //    var aesIv = UploadKey.Instance.AesIv;
        //    var clientPublicKey = UploadKey.Instance.ClientPublicKey;

        //    for (var i = 0; i < 3; i++)
        //    {
        //        var timeSpan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        //        dataForm["t"] = timeSpan.ToString();
        //        dataForm["token"] = UploadEncryptHelper.GetToken(totalSha1, length, signKey, signVal, timeSpan, UserId, userIdMd5, AppVer);

        //        if (!string.IsNullOrEmpty(signKey) && !string.IsNullOrEmpty(signVal))
        //        {
        //            dataForm["sign_key"] = signKey;
        //            dataForm["sign_val"] = signVal;
        //        }

        //        FastUploadResult? fastUploadResult;
        //        try
        //        {
        //            fastUploadResult = await GetUpload115Result(dataForm, timeSpan, aesKey, aesIv, clientPublicKey, Token);
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine($"获取Upload115Result时发生错误：{ex.Message}");
        //            break;
        //        }

        //        if (fastUploadResult == null)
        //        {
        //            Debug.WriteLine("可能是解密出错");
        //            continue;
        //        }

        //        if (string.IsNullOrEmpty(fastUploadResult.sign_key) || string.IsNullOrEmpty(fastUploadResult.sign_check))
        //        {
        //            // 不能秒传，需要上传
        //            if (!string.IsNullOrEmpty(fastUploadResult.Object))
        //            {
        //                return null;
        //            }

        //            // 秒传成功
        //            if (fastUploadResult.status == 2)
        //            {
        //                return fastUploadResult;
        //            }

        //            Debug.WriteLine($"上传时发生错误：{fastUploadResult.statusmsg}");

        //            return null;
        //        }

        //        signKey = fastUploadResult.sign_key;
        //        signVal = await GetRangSha1FromInternet(pickCode, fastUploadResult.sign_check);
        //    }

        //    return null;
        //}

    }
}
