using Display.Helper.Crypto;
using Display.Models.Upload;
using Display.Services.Upload;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Display.Helper.Network;

internal static class UploadHelper
{
    /// <summary>
    /// 获取115秒传必备的result
    /// </summary>
    /// <param name="dataForm"></param>
    /// <param name="timeSpan"></param>
    /// <param name="aesKey"></param>
    /// <param name="aesIv"></param>
    /// <param name="clientPublicKey"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<FastUploadResult> GetUpload115Result(Dictionary<string, string> dataForm, long timeSpan, byte[] aesKey, byte[] aesIv, byte[] clientPublicKey, CancellationToken token)
    {
        var sendData = UploadEncryptHelper.GetData(dataForm, aesKey, aesIv);
        var kec = UploadEncryptHelper.GetKEc(clientPublicKey, timeSpan);

        var url = $"https://uplb.115.com/4.0/initupload.php?k_ec={kec}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Content = new ByteArrayContent(sendData)
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded") }
        };

        var response = await FileUploadService.Client.SendAsync(request, token);

        if (!response.IsSuccessStatusCode) return null;

        var contentBytes = await response.Content.ReadAsByteArrayAsync(token);

        return UploadEncryptHelper.DecryptReceiveData<FastUploadResult>(contentBytes, aesKey: aesKey, aesIv: aesIv);
    }

    /// <summary>
    /// 构建上传所需表单
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="fileId"></param>
    /// <param name="name"></param>
    /// <param name="length"></param>
    /// <param name="userId"></param>
    /// <param name="userKey"></param>
    /// <param name="userIdMd5"></param>
    /// <returns></returns>
    public static Dictionary<string, string> BuildDataForm(long cid, string fileId, string name, long length, int userId, string userKey, out string userIdMd5)
    {
        var target = "U_1_" + cid;
        var sign = UploadEncryptHelper.GetSign(userId, fileId, target, userKey);

        userIdMd5 = HashHelper.ComputeMd5ByContent(userId.ToString()).ToLower();

        return new Dictionary<string, string>
        {
            { "appid", "0" },
            { "appversion", FileUploadService.AppVer },
            { "filename", HttpUtility.UrlEncode(name, Encoding.UTF8) },
            { "filesize", length.ToString() },
            { "fileid", fileId },
            { "target", target },
            { "userid", userId.ToString() },
            { "sig", sign },
        };
    }
}